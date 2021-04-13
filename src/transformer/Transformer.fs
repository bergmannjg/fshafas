/// Transform interface types to record types
module Transformer

open System.Collections.Generic
open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.Text
open FSharp.Compiler.SyntaxTree
open FSharp.Compiler.XmlDoc
open System.IO
open System

let private checker = FSharpChecker.Create()

type TransformerOptions =
    { prelude: string option
      postlude: string option
      useRecursiveTypes: bool
      escapeIdent: string -> string
      transformType: string -> string
      excludesType: string -> bool
      transformsTypeVal: string -> string -> string option
      transformsTypeDefn: string -> string option }

let private parse fileName text =
    let checker = FSharpChecker.Create()

    let opts =
        { FSharpParsingOptions.Default with
              SourceFiles = [| fileName |] }

    let parseFileResults =
        checker.ParseFile(fileName, text, opts)
        |> Async.RunSynchronously

    match parseFileResults.ParseTree with
    | Some tree -> tree
    | None -> failwith "Something went wrong during parsing!"

let private toString (lid: LongIdent) =
    String.Join(".", lid |> List.map (fun ident -> ident.idText))

let rec private visitSnyType synType options =
    match synType with
    | SynType.LongIdent (longIdentWithDots) ->
        let (LongIdentWithDots (id, _)) = longIdentWithDots
        options.transformType (toString id)
    | SynType.Tuple (_, elementTypes, _) ->
        let line = List()

        for (_, elementType) in elementTypes do
            visitSnyType elementType options |> line.Add

        "(" + (line |> String.concat ",") + ")"
    | SynType.Fun (argType, returnType, _) ->
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
    let (SynValSig.ValSpfn (_, id, _, synType, _, _, _, xmlDoc, _, _, _)) = slotSig
    let xmldoc = xmlDoc.ToXmlDoc(false, None)

    let line = List()

    let lines =
        xmldoc.UnprocessedLines
        |> Array.map (fun l -> sprintf "///%s" l)

    line.AddRange lines

    let escText = options.escapeIdent id.idText

    let strsynType =
        match options.transformsTypeVal typename escText with
        | Some transform -> transform
        | None -> visitSnyType synType options

    line.Add(sprintf "%s: %s" escText strsynType)

    line

let private visitTypeMembers (typename: string) members options =
    let line = List()

    for m in members do
        match m with
        | SynMemberDefn.AbstractSlot (slotSig, _, _) -> line.AddRange(visitValSig typename slotSig options)
        | _ -> failwith (sprintf " - not supported SynMemberDefn: %A" m)

    line

let private visitSimple simpleRepr options =
    let line = List()

    match simpleRepr with
    | SynTypeDefnSimpleRepr.Union (_, cases, _) ->
        for case in cases do
            let (UnionCase (_, id, _, _, _, _)) = case
            line.Add(sprintf "| %s" id.idText)
    | _ -> ()

    line

let private visitTypeDefn typeDefn options =
    let (SynTypeDefn.TypeDefn (typeInfo, typeRepr, members, range)) = typeDefn
    let (SynComponentInfo.ComponentInfo (_, __, _, id, xmlDoc, _, _, _)) = typeInfo

    let lines = List()
    let xmlDoc = xmlDoc.ToXmlDoc(false, None)

    lines.AddRange(
        xmlDoc.UnprocessedLines
        |> Array.map (fun l -> sprintf "///%s" l)
    )

    let strMembers =
        match typeRepr with
        | SynTypeDefnRepr.ObjectModel (_, members, _) -> visitTypeMembers (toString id) members options
        | SynTypeDefnRepr.Simple (simpleRepr, _) -> visitSimple simpleRepr options
        | _ -> failwith (sprintf " - not supported SynTypeDefnRepr: %A" typeRepr)

    let isRecord =
        not (
            strMembers.Count > 0
            && strMembers.[0].StartsWith "|"
        )

    let transform = options.transformsTypeDefn (toString id)

    let typeSymbol =
        if options.useRecursiveTypes then
            "and"
        else
            "type"

    if (transform.IsSome) then
        sprintf "%s %s = %s" typeSymbol (toString id) transform.Value
        |> lines.Add
    else
        () /// problems with 'Format Document'

        if not (options.excludesType (toString id)) then
            (sprintf "%s %s = " typeSymbol (toString id))
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
                lines.Add(sprintf "open %s" (toString longDotId))
        | SynModuleDecl.Types (typeDefns, range) ->
            for typeDefn in typeDefns do
                lines.AddRange(visitTypeDefn typeDefn options)
        | SynModuleDecl.NestedModule (lid, __, decls0, _, _) ->
            let (SynComponentInfo.ComponentInfo (_, __, _, id, _, _, _, _)) = lid
            lines.AddRange(visitDeclarations decls0 options)
        | _ -> failwith (sprintf " - not supported SynModuleDecl: %A" declaration)

    lines

let private visitModulesAndNamespaces modulesOrNss options =
    let lines = List()

    for moduleOrNs in modulesOrNss do
        let (SynModuleOrNamespace (lid, isRec, isMod, decls, xml, attrs, _, m)) = moduleOrNs

        if options.prelude.IsNone then
            lines.Add(sprintf "module %s" (toString lid))

        lines.AddRange(visitDeclarations decls options)

    lines

let transform (fromFile: string) (toFile: string) (options: TransformerOptions) =
    let sw = new StreamWriter(path = toFile)
    sw.AutoFlush <- true

    let tree =
        parse fromFile (SourceText.ofString (File.ReadAllText fromFile))

    match tree with
    | ParsedInput.ImplFile (implFile) ->
        let (ParsedImplFileInput (fn, script, name, _, _, modules, _)) = implFile

        let lines =
            visitModulesAndNamespaces modules options

        let prefix =
            if
                options.prelude.IsSome
                && options.prelude.Value.Contains("module")
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
