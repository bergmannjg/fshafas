namespace FsHafas.Printf

#if !FABLE_COMPILER

module Long =

    open System
    open FSharp.Reflection
    open FsHafas.Reflect

    let mutable private printSome = false
    let mutable private printNone = false
    let private ident (depth: int) = 2 * depth

    let private printRecordFieldname (depth: int) (name: string) (typ: Type) =
        fprintfn stdout "%s" (sprintf "%*s%s: (%s)" (ident depth) "" name (typ.ToString()))

    let private printCaseInfo (depth: int) (case: UnionCaseInfo) =
        if printSome || case.Name <> "Some" then
            fprintfn stdout "%s" (sprintf "%*s%s: (%s)" (ident depth) "" case.Name (case.DeclaringType.ToString()))

    let private printField (depth: int) (name: string) (o: obj) =
        if not (isNull o) then
            let typ = o.GetType()
            fprintfn stdout "%s" (sprintf "%*s%s: (%s) %A" (ident depth) "" name typ.Name o)
        else if printNone then
            fprintfn stdout "%s" (sprintf "%*s%s: %s" (ident depth) "" name "null")

    let private printMapField (depth: int) (name: string) (o: Map<string, bool>) =
        let v =
            o
            |> Seq.filter (fun kv -> kv.Value)
            |> Seq.map (fun kv -> kv.Key)
            |> String.concat ","

        fprintfn stdout "%s" (sprintf "%*s%s: (%s) %s" (ident depth) "" name (o.GetType().Name) v)

    let private printEmptyArray (depth: int) (name: string) =
        fprintfn stdout "%s" (sprintf "%*s%s: %s" (ident depth) "" name "[]")

    let private printUnkownType (depth: int) (t: Type) (name: string) (o: obj) =
        if name = "properties" then // todo
            ()
        else if name = "icon" then // todo
            ()
        else
            fprintfn stderr "%s" (sprintf "common: unkown type %s, field '%s', value '%A'" t.FullName name o)

    let private evt: Traverse.TraverseEvent =
        { onRecordFieldname = printRecordFieldname
          onCaseInfo = printCaseInfo
          onField = printField
          onUnkownType = printUnkownType
          onEmptyArray = printEmptyArray
          onMapField = printMapField }

    let print (o: obj) = Traverse.traverse evt o

#endif
