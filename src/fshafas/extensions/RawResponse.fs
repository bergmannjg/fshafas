namespace FsHafas.Extensions

#if !FABLE_COMPILER
open FsHafas.Api
#endif

open FsHafas.Raw

#if FABLE_COMPILER
open Fable.Core
#endif

#if FABLE_JS
open Thoth.Json
#endif

#if FABLE_PY
open System.Text.RegularExpressions

open Fable.SimpleJson.Python
#endif

module internal RawResponseEx =

#if FABLE_JS
    let decode (json: string) : RawResponse =
        let decoded = Decode.Auto.fromString<RawResponse> (json, caseStrategy = CamelCase)

        match decoded with
        | Ok response -> response
        | Error decodingError -> failwith (sprintf "was unable to decode: %s. Reason: %s" json decodingError)
#else
#if FABLE_PY
    // see https://github.com/fable-compiler/Fable/blob/main/src/Fable.Transforms/Python/Prelude.fs
    let private dashify (separator: string) (input: string) =
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

    let private toDashed (xs: string list) = Some(dashify "_" (List.last xs))

    // workaround: use CompiledName atttribute
    let decode (input: string) : RawResponse =
        try
            input
            |> SimpleJson.parseNative
            |> SimpleJson.mapKeysByPath toDashed
            |> Json.convertFromJsonAs<RawResponse>
        with
        | e ->
            printf "error decode: %s" e.Message
            raise (System.Exception(e.Message))

#else

    let decode (json: string) : RawResponse =
        Serializer.Deserialize<RawResponse>(json)

#endif
#endif
