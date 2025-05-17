namespace DbVendo.Api

/// <exclude>Serializer</exclude>
module Serializer =

    open FsHafas.Client

#if FABLE_COMPILER
    open Fable.Core
#endif

#if FABLE_PY
    open System.Text.RegularExpressions
    open Fable.SimpleJson.Python
#endif

#if FABLE_JS
    open Thoth.Json

    let inline Serialize<'a> (input: 'a) : string =
        let encoder (u: 'a) =
            u |> Encode.Auto.generateEncoderCached<'a> (caseStrategy = CamelCase)

        input |> encoder |> Encode.toString 0

    module U5 =
        let inline Serialize (input: U5<'a, 'b, 'c, 'd, 'e>) : string =
            let encoder (u: U5<'a, 'b, 'c, 'd, 'e>) =
                match u with
                | U5.Case1 r -> r |> Encode.Auto.generateEncoderCached<'a> (caseStrategy = CamelCase)
                | U5.Case2 r -> r |> Encode.Auto.generateEncoderCached<'b> (caseStrategy = CamelCase)
                | U5.Case3 r -> r |> Encode.Auto.generateEncoderCached<'c> (caseStrategy = CamelCase)
                | U5.Case4 r -> r |> Encode.Auto.generateEncoderCached<'d> (caseStrategy = CamelCase)
                | U5.Case5 r -> r |> Encode.Auto.generateEncoderCached<'e> (caseStrategy = CamelCase)

            input |> encoder |> Encode.toString 0

    let inline Deserialize<'a> (input: string) : 'a =
        let decoded = Decode.Auto.fromString<'a> (input, caseStrategy = CamelCase)

        match decoded with
        | Ok response -> response
        | Error decodingError -> failwith (sprintf "was unable to decode: %s. Reason: %s" input decodingError)
#else
#if FABLE_PY
    // fable compiles field names to snake case
    // Raw.RequestBody field names are in camel case
    let fromSnakeCasetoCamelCase (input: string) =
        Regex.Replace(input, "_[a-z]", (fun m -> m.Value.Substring(1, 1).ToUpperInvariant()))

    let inline Serialize<'a> (input: 'a) : string =
        input |> Json.serialize |> fromSnakeCasetoCamelCase

    module U5 =
        let inline Serialize (input: U5<'a, 'b, 'c, 'd, 'e>) : string =
            match input with
            | U5.Case1 r -> Json.serialize r
            | U5.Case2 r -> Json.serialize r
            | U5.Case3 r -> Json.serialize r
            | U5.Case4 r -> Json.serialize r
            | U5.Case5 r -> Json.serialize r
            |> fromSnakeCasetoCamelCase

    // fable compiles field names to snake case
    // the field name is in camel case
    let fromCamelCasetoSnakeCase (field: string) =
        Regex.Replace(
            field,
            "[a-z]?[A-Z]",
            fun m ->
                if m.Value.Length = 1 then
                    m.Value.ToLowerInvariant()
                else
                    m.Value.Substring(0, 1) + "_" + m.Value.Substring(1, 1).ToLowerInvariant()
        )

    let inline Deserialize<'a> (input: string) : 'a =
        try
            input
            |> SimpleJson.parseNative
            |> SimpleJson.mapKeys fromCamelCasetoSnakeCase
            |> Json.convertFromJsonAs<'a>
        with e ->
            printf "error decode: %s" e.Message
            raise (System.Exception(e.Message))
#else

    open System.Text.Json
    open System.Text.Json.Serialization

    let private deserializeOptions =
        JsonSerializerOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)

    let private serializeOptions =
        JsonSerializerOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)

    let addDeserializeConverters (deserializeConverters: JsonConverter array) =
        for converter in deserializeConverters do
            deserializeOptions.Converters.Add(converter)

        deserializeOptions.Converters.Add(
            JsonFSharpConverter(
                JsonUnionEncoding.InternalTag
                ||| JsonUnionEncoding.UnwrapRecordCases
                ||| JsonUnionEncoding.UnwrapOption
                ||| JsonUnionEncoding.UnwrapFieldlessTags,
                unionTagName = "type",
                unionTagCaseInsensitive = true
            )
        )

    let addSerializeConverters (serializeConverters: JsonConverter array) =
        for converter in serializeConverters do
            serializeOptions.Converters.Add(converter)

        serializeOptions.Converters.Add(JsonFSharpConverter())

    let Deserialize<'a> (response: string) =
        JsonSerializer.Deserialize<'a>(response, deserializeOptions)

    let Serialize<'a> (o: 'a) =
        JsonSerializer.Serialize<'a>(o, serializeOptions)

    module U5 =
        let Serialize (o: U5<'a, 'b, 'c, 'd, 'e>) =
            JsonSerializer.Serialize<U5<'a, 'b, 'c, 'd, 'e>>(o, serializeOptions)
#endif
#endif
