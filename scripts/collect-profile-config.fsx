#r "nuget:FSharp.SystemTextJson"
#r "../src/fshafas/target.dotnet/bin/Debug/net6.0/fshafas.dll"

open System.Text.RegularExpressions
open FSharp.Collections
open FsHafas
open FsHafas.Client
open FsHafas.Api

Api.HafasAsyncClient.initSerializer ()

let fromStdin () =
    fun _ -> stdin.ReadLine()
    |> Seq.initInfinite // (1)
    |> Seq.takeWhile ((<>) null)
    |> String.concat "\n"

module Products =
    let prefix (profile: string) =
        @"namespace FsHafas.Profiles."
        + profile
        + "Config

    module internal Products =

        open FsHafas.Client

        let products: ProductType [] =
            [|
"

    let toString<'a> (profile: string) (arr: array<'a>) =
        let transform (s: string) =
            s
                .Replace("default", "``default``")
                .Replace("\n", "\n        ")

        arr
        |> Array.fold
            (fun s x ->
                s
                + "        "
                + (transform (sprintf "%A" x))
                + "\n")
            (prefix (profile))
        |> fun s -> s + "        |]"

    let mkProducts (profile: string) (json: string) =
        let products = Serializer.Deserialize<array<ProductType>> json

        printfn "%s" (toString<ProductType> profile products)

module BaseProfile =

    type BaseRequestAuth =
        { ``type``: string option
          aid: string option }

    type BaseRequestClient =
        { ``type``: string option
          id: string option
          v: string option
          name: string option }

    type Base =
        { defaultLanguage: string option
          ext: string option
          ver: string option
          endpoint: string
          client: BaseRequestClient option
          auth: BaseRequestAuth option }

    let prefix (profile: string) =
        @"namespace FsHafas.Profiles."
        + profile
        + "Config

    module internal Request =

        open FsHafas.Raw

        let request: RawRequest =
            {
              lang = \"de\"
              svcReqL = [||]"

    let toString (profile: string) (o: Base) =
        let transform (s: string option) =
            match s with
            | Some s -> if s = "{" then s else ("\"" + s + "\"")
            | None -> "\"\""

        let blank = "\n              "

        let field (name: string) (value: string option) =
            blank + name + " = " + (transform value)

        let optionField (name: string) (value: string option) =
            let t = transform value

            blank
            + name
            + " = "
            + (if t <> "\"\"" then
                   "Some " + t
               else
                   "None")

        prefix (profile)
        + field "client" (Some "{")
        + field "id" o.client.Value.id
        + field "v" o.client.Value.v
        + field "``type``" o.client.Value.``type``
        + field "name" o.client.Value.name
        + blank
        + "}"
        + optionField "ext" o.ext
        + field "ver" o.ver
        + field "auth" (Some "{")
        + field "``type``" o.auth.Value.``type``
        + field "aid" o.auth.Value.aid
        + blank
        + "}"
        + "\n            }"

    let mkBaseProfile (profile: string) (json: string) =
        let baseProfile =
            Serializer.Deserialize<Base>(Regex.Replace(json, "\"v\"\:\s*([0-9]+)", "\"v\": \"$1\""))

        printfn "%s" (toString profile baseProfile)


let args = fsi.CommandLineArgs |> Array.skip 1

match args with
| [| "--products"; profile |] -> Products.mkProducts profile (fromStdin ())
| [| "--base"; profile |] -> BaseProfile.mkBaseProfile profile (fromStdin ())
| _ -> printfn "unkown args %A" args
