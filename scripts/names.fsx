// generate list of field names and the corresponding dashified names

#r "nuget:Polyliner.Net"
#r "nuget:FSlugify"
#r "nuget:NodaTime"
#r "../src/fshafas/target.dotnet/bin/Debug/net6.0/fshafas.dll"

open System
open System.Reflection
open System.Text.RegularExpressions

// see https://github.com/fable-compiler/Fable/blob/beyond/src/Fable.Transforms/Python/Prelude.fs
let dashify (separator: string) (input: string) =
    Regex.Replace(
        input,
        "[a-z]?[A-Z]",
        fun m ->
            if m.Value.Length = 1 then
                m.Value.ToLowerInvariant()
            else
                m.Value.Substring(0, 1)
                + separator
                + m.Value.Substring(1, 1).ToLowerInvariant()
    )

let undashify (input: string) =
    Regex.Replace(input, "_[a-z]", (fun m -> m.Value.Substring(1, 1).ToUpperInvariant()))

let dashifyProps (t: Type) =
    t.GetProperties()
    |> Array.map (fun p -> (p.Name, dashify "_" p.Name))
    |> Array.filter (fun (n, d) -> n <> d)

typeof<FsHafas.Raw.Cfg>.Module.GetTypes ()
|> Array.filter (fun t -> t.Namespace = "FsHafas.Raw")
|> Array.collect (fun t -> dashifyProps t)
|> Array.sortBy (fun (n, d) -> n)
|> Array.distinct
|> Array.iter (fun (n, d) ->
    let undashed = undashify d

    if n <> undashed then
        printfn "(\"%s\",\"%s\")" d n)
