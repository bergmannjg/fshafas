namespace FsHafas.Client

open System

#if FABLE_COMPILER
open Fable.Core
open Fable.Core.JS
#endif

#if !FABLE_COMPILER
type Promise<'T> = Async<'T>

[<AttributeUsage(AttributeTargets.Class)>]
type TypeScriptTaggedUnionAttribute(key: string) =
    inherit Attribute()

[<AttributeUsage(AttributeTargets.Class)>]
type StringEnumAttribute() =
    inherit Attribute()
#endif

type Log() =
    static let mutable debug = false

    static member Debug
        with get () = debug
        and set (v) = debug <- v

    static member Print (msg: string) (o: obj) = if debug then printfn "%s %A" msg o

and HafasError(code: string, msg: string) =
    inherit Exception(msg)
    member e.code = code
    member e.isHafasError = true

and Icon =
    { ``type``: string
      title: string option }

#if FABLE_JS
type IndexMap<'s, 'b when 's: comparison>(defaultValue: 'b) =
    [<EmitIndexer>]
    member __.Item
        with get (s: 's): 'b = jsNative
        and set s b = jsNative

    [<Emit("(Object.keys($0))")>]
    member __.Keys: 's [] = jsNative
#else
#if FABLE_PY
module internal Dict =
    [<Emit("dict()")>]
    let dict () : obj = jsNative

    [<Emit("$0.get($1)")>]
    let get<'a> (d: obj) (k: obj) : 'a = jsNative

    [<Emit("$0[$1]=$2")>]
    let set (d: obj) (k: obj) (v: obj) : unit = jsNative

    [<Emit("list($0)")>]
    let keys<'a> (d: obj) : 'a [] = jsNative

type IndexMap<'s, 'b when 's :> obj and 'b: equality>(defaultValue: 'b) =
    let dict = Dict.dict ()

    member __.Item
        with get (s: 's) =
            let v = Dict.get<'b> dict s

            if (box v) <> null then
                v
            else
                defaultValue
        and set s b =
            Dict.set dict s b
            ()

    member __.Keys = Dict.keys<'s> dict
#else
type IndexMap<'s, 'b when 's: comparison>(defaultValue: 'b) =
    let mutable map: Map<'s, 'b> = Map.empty

    member __.Item
        with get (s: 's) =
            match map.TryFind s with
            | Some v -> v
            | None -> defaultValue
        and set s b =
            map <- map.Add(s, b)
            ()

    member __.Keys = map |> Seq.map (fun kv -> kv.Key) |> Seq.toArray
#endif
#endif
