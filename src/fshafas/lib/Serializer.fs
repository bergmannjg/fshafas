module internal Serializer

open System.Text.Json
open System.Text.Json.Serialization

let deserializeOptions = JsonSerializerOptions()

let serializeOptions = JsonSerializerOptions(IgnoreNullValues = true)

let addConverters (deserializeConverters: JsonConverter array) =
    if (deserializeOptions.Converters.Count = 0) then
        for converter in deserializeConverters do
            deserializeOptions.Converters.Add(converter)

        deserializeOptions.Converters.Add(
            JsonFSharpConverter(
                JsonUnionEncoding.InternalTag
                ||| JsonUnionEncoding.UnwrapRecordCases
                ||| JsonUnionEncoding.UnwrapOption,
                unionTagName = "type",
                unionTagCaseInsensitive = true
            )
        )

        serializeOptions.Converters.Add(JsonFSharpConverter())

let Deserialize<'a> (response: string) =
    JsonSerializer.Deserialize<'a>(response, deserializeOptions)

let Serialize<'a> (o: 'a) =
    JsonSerializer.Serialize<'a>(o, serializeOptions)

let SerializeWithConverter<'a> (o: 'a) (converter: JsonConverter) =
    let serializeOptions = JsonSerializerOptions(IgnoreNullValues = true)

    serializeOptions.Converters.Add(converter)
    serializeOptions.Converters.Add(JsonFSharpConverter())

    JsonSerializer.Serialize<'a>(o, serializeOptions)
