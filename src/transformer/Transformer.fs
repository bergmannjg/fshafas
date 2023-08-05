/// Transform interface types to record types
module Transformer

open System.Collections.Generic
open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Text
open FSharp.Compiler.Syntax
open System.IO
open System

type TransformerOptions =
    { prelude: string option
      postlude: string option
      useRecursiveTypes: bool
      escapeIdent: string -> string
      transformType: string -> string
      excludesType: string -> bool
      toMemberType: string -> bool
      isIntegerTypeVal: string -> string -> bool
      isCase1OfU2TypeVals: string -> string -> bool
      transformsTypeVal: string -> string -> string option
      transformsTypeDefn: string -> string option
      getIntersectionTypes: string -> string [] }

let mutable private types: Map<string, string List> = Map.empty

let private parse fileName text =
    let checker = FSharpChecker.Create()

    let opts = { FSharpParsingOptions.Default with SourceFiles = [| fileName |] }

    let parseFileResults =
        checker.ParseFile(fileName, text, opts)
        |> Async.RunSynchronously

    if parseFileResults.ParseHadErrors then
        failwith "Something went wrong during parsing!"
    else
        parseFileResults.ParseTree

let private SynLongIdentToString (synlid: SynLongIdent) =
    let (SynLongIdent (lid, _, _)) = synlid
    String.Join(".", lid |> List.map (fun ident -> ident.idText))

let private LongIdentToString (lid: LongIdent) =
    String.Join(".", lid |> List.map (fun ident -> ident.idText))

let rec private visitSnyType synType options =
    match synType with
    | SynType.LongIdent (longIdentWithDots) ->
        let (SynLongIdent (id, _, _)) = longIdentWithDots
        options.transformType (LongIdentToString id)
    | SynType.Tuple (_, elementTypes, _) ->
        let line = List()

        for (_, elementType) in elementTypes do
            visitSnyType elementType options |> line.Add

        "(" + (line |> String.concat ",") + ")"
    | SynType.Fun (argType, returnType, _, _) ->
        let strargType = visitSnyType argType options
        let strreturnType = visitSnyType returnType options
        strargType + "->" + strreturnType
    | SynType.Paren (innerType, _) -> visitSnyType innerType options
    | SynType.App (typeName, _, typeArgs, _, _, _, _) ->
        let strtypeName = visitSnyType typeName options
        let line = List()

        for typeArg in typeArgs do
            visitSnyType typeArg options |> line.Add

        if line.Count = 1 && strtypeName = "option" then
            line.[0] + " option"
        else if strtypeName = "U2"
                && line.Count = 2
                && (options.isCase1OfU2TypeVals line.[0] line.[1]) then
            line.[0]
        else if line.Count > 0 then
            (options.transformType (
                strtypeName
                + "<"
                + (line |> String.concat ",")
                + ">"
            ))
        else
            strtypeName
    | _ -> failwith (sprintf " - not supported SynType: %A" synType)

let private visitValSig (typename: string) slotSig options =
    let (SynValSig (_, synid, _, synType, _, _, _, xmlDoc, _, _, _, _)) = slotSig
    let xmldoc = xmlDoc.ToXmlDoc(false, None)

    let line = List()

    let lines =
        xmldoc.UnprocessedLines
        |> Array.map (fun l -> sprintf "///%s" l)

    line.AddRange lines

    let (SynIdent (id, _)) = synid
    let escText = options.escapeIdent id.idText

    let strsynTypeOrg = visitSnyType synType options

    let strsynType =
        match options.transformsTypeVal typename escText with
        | Some transform -> transform
        | None ->
            if strsynTypeOrg.Contains "float"
               && options.isIntegerTypeVal typename escText then
                strsynTypeOrg.Replace("float", "int")
            else
                strsynTypeOrg

    line.Add(sprintf "%s: %s" escText strsynType)

    line

let private visitTypeMembers (typename: string) members options =
    let line = List()

    for m in members do
        match m with
        | SynMemberDefn.AbstractSlot (slotSig, _, _) -> line.AddRange(visitValSig typename slotSig options)
        | SynMemberDefn.Inherit (baseType, _, _) ->
            let name = visitSnyType baseType options

            match types |> Map.tryFind name with
            | Some members -> line.AddRange(members)
            | None -> ()

        | _ -> failwith (sprintf " - not supported SynMemberDefn: %A" m)

    line

let getAttribute (attributes: SynAttributes) =
    if attributes.Length > 0
       && attributes.[0].Attributes.Length > 0 then
        Some attributes.[0].Attributes.[0]
    else
        None

let hasAttribute (name: string) (attributes: SynAttributes) =
    match getAttribute attributes with
    | Some attr when (SynLongIdentToString attr.TypeName) = name -> true
    | _ -> false

let getAttributeValue (name: string) (defValue: string) (attributes: SynAttributes) =
    match getAttribute attributes with
    | Some attr when (SynLongIdentToString attr.TypeName) = name ->
        match attr.ArgExpr with
        | SynExpr.Const (c, _) ->
            match c with
            | SynConst.String (s, _, _) -> "[<CompiledName \"" + s + "\">] "
            | _ -> ""
        | _ -> ""
    | _ -> "[<CompiledName \"" + defValue + "\">] "

let private visitSimple simpleRepr useCompiledNameAttr options =
    let line = List()

    match simpleRepr with
    | SynTypeDefnSimpleRepr.Union (_, cases, _) ->
        for case in cases do
            let (SynUnionCase (attributes, synid, _, _, _, _, _)) = case
            let (SynIdent (id, _)) = synid

            let compiledNameAttr =
                if useCompiledNameAttr then
                    getAttributeValue "CompiledName" (id.idText.ToLower()) attributes
                else
                    ""

            line.Add(sprintf "| %s%s" compiledNameAttr id.idText)
    | _ -> ()

    line

let mutable private hasFirstType = false

let private visitTypeDefn typeDefn options =
    let (SynTypeDefn (typeInfo, typeRepr, members, range, _, _)) = typeDefn

    let (SynComponentInfo (attributes, __, _, id, xmlDoc, _, _, _)) = typeInfo

    let stringEnumAttr =
        if hasAttribute "StringEnum" attributes then
            "[<StringEnum>] "
        else
            ""

    let useCompiledNameAttr = stringEnumAttr.Length > 0

    let lines = List()
    let xmlDoc = xmlDoc.ToXmlDoc(false, None)

    lines.AddRange(
        xmlDoc.UnprocessedLines
        |> Array.map (fun l -> sprintf "///%s" l)
    )

    let strMembers =
        match typeRepr with
        | SynTypeDefnRepr.ObjectModel (_, members, _) -> visitTypeMembers (LongIdentToString id) members options
        | SynTypeDefnRepr.Simple (simpleRepr, _) -> visitSimple simpleRepr useCompiledNameAttr options
        | _ -> failwith (sprintf " - not supported SynTypeDefnRepr: %A" typeRepr)

    types <- types.Add((LongIdentToString id), strMembers)

    let isRecord =
        not (
            strMembers.Count > 0
            && strMembers.[0].StartsWith "|"
        )

    let transform = options.transformsTypeDefn (LongIdentToString id)

    let intersectionTypes = options.getIntersectionTypes (LongIdentToString id)

    let typeSymbol () =
        if options.useRecursiveTypes && hasFirstType then
            "and"
        else
            hasFirstType <- true
            "type"

    if (transform.IsSome) then
        sprintf "%s %s = %s" (typeSymbol ()) (LongIdentToString id) transform.Value
        |> lines.Add
    else if intersectionTypes.Length > 0 then
        (sprintf "%s %s%s = " (typeSymbol ()) stringEnumAttr (LongIdentToString id))
        + "{"
        |> lines.Add

        for intersectionType in intersectionTypes do
            if types.ContainsKey intersectionType then
                let strMembers = types[intersectionType]
                for strMember in strMembers do
                    sprintf "    %s" strMember |> lines.Add

            ()

        sprintf "}" |> lines.Add

    else if (options.toMemberType (LongIdentToString id)) then
        sprintf "%s %s = " (typeSymbol ()) (LongIdentToString id)
        |> lines.Add

        for strMember in strMembers do
            if (strMember.StartsWith("//")) then
                sprintf "        %s" strMember |> lines.Add
            else
                let m =
                    if strMember.Contains "->"
                       && strMember.EndsWith " option" then
                        strMember.Substring(0, strMember.Length - 7) // no optional member functions
                    else
                        strMember

                sprintf "        abstract member %s" m
                |> lines.Add

        lines.Add("       ")
    else
        ()

        if not (options.excludesType (LongIdentToString id)) then
            (sprintf "%s %s%s = " (typeSymbol ()) stringEnumAttr (LongIdentToString id))
            + (if isRecord then "{" else "")
            |> lines.Add

            for strMember in strMembers do
                sprintf "    %s" strMember |> lines.Add

            if isRecord then
                sprintf "}" |> lines.Add

    lines

let rec private visitDeclarations decls options =
    let lines = List()

    for declaration in decls do
        match declaration with
        | SynModuleDecl.Open (SynOpenDeclTarget.ModuleOrNamespace (longDotId, _), range) ->
            if options.prelude.IsNone then
                lines.Add(sprintf "open %s" (LongIdentToString longDotId))
        | SynModuleDecl.Types (typeDefns, range) ->
            for typeDefn in typeDefns do
                lines.AddRange(visitTypeDefn typeDefn options)
        | SynModuleDecl.NestedModule (lid, __, decls0, _, _, _) ->
            let (SynComponentInfo (_, __, _, id, _, _, _, _)) = lid
            lines.AddRange(visitDeclarations decls0 options)
        | _ -> failwith (sprintf " - not supported SynModuleDecl: %A" declaration)

    lines

let private visitModulesAndNamespaces modulesOrNss options =
    let lines = List()

    for moduleOrNs in modulesOrNss do
        let (SynModuleOrNamespace (lid, isRec, isMod, decls, xml, attrs, _, m, _)) =
            moduleOrNs

        if options.prelude.IsNone then
            lines.Add(sprintf "module %s" (LongIdentToString lid))

        lines.AddRange(visitDeclarations decls options)

    lines

let transform (fromFile: string) (toFile: string) (options: TransformerOptions) =
    let sw = new StreamWriter(path = toFile)
    sw.AutoFlush <- true
    types <- Map.empty
    hasFirstType <- false

    let tree = parse fromFile (SourceText.ofString (File.ReadAllText fromFile))

    match tree with
    | ParsedInput.ImplFile (implFile) ->
        let (ParsedImplFileInput (fn, script, name, _, _, modules, _, _)) = implFile

        let lines = visitModulesAndNamespaces modules options

        let prefix =
            if
                options.prelude.IsSome
                && options.prelude.Value.Contains("module")
                && not (options.prelude.Value.Contains("module internal"))
            then
                "    "
            else
                ""

        if options.prelude.IsSome then
            fprintfn sw "%s" options.prelude.Value

        for line in lines do
            fprintfn sw "%s%s" prefix line

        if options.postlude.IsSome then
            fprintfn sw "%s" options.postlude.Value

    | _ -> failwith "F# Interface file (*.fsi) not supported."

    fprintfn sw ""
