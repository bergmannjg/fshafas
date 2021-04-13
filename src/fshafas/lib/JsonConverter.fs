namespace FsHafas.Api

open FsHafas.Client

/// <exclude>Converter</exclude>
module Converter =

    open System
    open System.Text.Json.Serialization
    open System.Text.Json

    type UnionCaseSelection =
        | Disabled
        | ByTagName of string /// tag value is typename

    let private readJsonObjectWithStringProperty (reader: byref<Utf8JsonReader>, name: string) =
        use jsonDocument = JsonDocument.ParseValue(&reader)

        let value =
            match jsonDocument.RootElement.TryGetProperty name with
            | true, jsonElement -> jsonElement.GetString()
            | _ -> raise (JsonException("Property not found: " + name))

        let jsonObject = jsonDocument.RootElement.GetRawText()

        (value, jsonObject)

    type U2EraseValueConverter<'S, 'T>(uc: UnionCaseSelection) =
        inherit JsonConverter<U2<'S, 'T>>()

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())

            match uc with
            | ByTagName unionTagName ->
                let (value, jsonObject) =
                    readJsonObjectWithStringProperty (&reader, unionTagName)

                if value = typedefof<'S>.Name.ToLower () then
                    U2.Case1(Serializer.Deserialize<'S>(jsonObject))
                else if value = typedefof<'T>.Name.ToLower () then
                    U2.Case2(Serializer.Deserialize<'T>(jsonObject))
                else
                    raise (JsonException("Invalid tag value encoding: " + value))
            | Disabled -> raise (JsonException("U2EraseValueConverter read disabled"))

        override this.Write(writer: Utf8JsonWriter, value: U2<'S, 'T>, options: JsonSerializerOptions) =
            match value with
            | U2.Case1 v -> JsonSerializer.Serialize(writer, v, options)
            | U2.Case2 v -> JsonSerializer.Serialize(writer, v, options)

    type U2EraseConverter<'S, 'T>(uc: UnionCaseSelection) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool =
            t.IsGenericType
            && t.GetGenericTypeDefinition() = typedefof<U2<'S, 'T>>

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            let types =
                typeToConvert.GetGenericArguments()
                |> Array.take 2

            let converterType =
                typedefof<U2EraseValueConverter<'S, 'T>>.MakeGenericType (types)

            Activator.CreateInstance(converterType, uc) :?> JsonConverter

    type U3EraseValueConverter<'S, 'T, 'U>(uc: UnionCaseSelection) =
        inherit JsonConverter<U3<'S, 'T, 'U>>()

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())

            match uc with
            | ByTagName unionTagName ->
                let (value, jsonObject) =
                    readJsonObjectWithStringProperty (&reader, unionTagName)

                if value = typedefof<'S>.Name.ToLower () then
                    U3.Case1(Serializer.Deserialize<'S>(jsonObject))
                else if value = typedefof<'T>.Name.ToLower () then
                    U3.Case2(Serializer.Deserialize<'T>(jsonObject))
                else
                    raise (JsonException("Invalid tag value encoding: " + value))
            | Disabled -> raise (JsonException("U2EraseValueConverter read disabled"))

        override this.Write(writer: Utf8JsonWriter, value: U3<'S, 'T, 'U>, options: JsonSerializerOptions) =
            match value with
            | U3.Case1 v -> JsonSerializer.Serialize(writer, v, options)
            | U3.Case2 v -> JsonSerializer.Serialize(writer, v, options)
            | U3.Case3 v -> JsonSerializer.Serialize(writer, v, options)

    type U3EraseConverter<'S, 'T, 'U>(uc: UnionCaseSelection) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool =
            t.IsGenericType
            && t.GetGenericTypeDefinition() = typedefof<U3<'S, 'T, 'U>>

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            let types =
                typeToConvert.GetGenericArguments()
                |> Array.take 3

            let converterType =
                typedefof<U3EraseValueConverter<'S, 'T, 'U>>.MakeGenericType (types)

            Activator.CreateInstance(converterType, uc) :?> JsonConverter

    type U13EraseValueConverter<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm>(uc: UnionCaseSelection) =
        inherit JsonConverter<U13<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm>>()

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            raise (System.NotImplementedException(""))

        override this.Write
            (
                writer: Utf8JsonWriter,
                value: U13<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm>,
                options: JsonSerializerOptions
            ) =
            match value with
            | U13.Case1 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case2 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case3 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case4 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case5 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case6 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case7 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case8 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case9 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case10 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case11 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case12 v -> JsonSerializer.Serialize(writer, v, options)
            | U13.Case13 v -> JsonSerializer.Serialize(writer, v, options)

    type U13EraseConverter<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm>(uc: UnionCaseSelection) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool =
            t.IsGenericType
            && t.GetGenericTypeDefinition() = typedefof<U13<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm>>

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            let types =
                typeToConvert.GetGenericArguments()
                |> Array.take 13

            let converterType =
                typedefof<U13EraseValueConverter<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm>>.MakeGenericType
                    (types)

            Activator.CreateInstance(converterType, uc) :?> JsonConverter

    type IndexMapValueConverter<'S, 'T when 'S: comparison>(defaultValue: 'T) =
        inherit JsonConverter<IndexMap<'S, 'T>>()

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            JsonSerializer.Deserialize<Map<'S, 'T>>(&reader, options)
            |> Seq.fold
                (fun m kv ->
                    m.[kv.Key] <- kv.Value
                    m)
                (IndexMap<'S, 'T>(defaultValue))

        override this.Write(writer: Utf8JsonWriter, value: IndexMap<'S, 'T>, options: JsonSerializerOptions) =
            raise (System.NotImplementedException(""))

    type IndexMapConverter<'S, 'T when 'S: comparison>(defaultValue: 'T) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool =
            t.IsGenericType
            && t.GetGenericTypeDefinition() = typedefof<IndexMap<'S, 'T>>

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            let types =
                typeToConvert.GetGenericArguments()
                |> Array.take 2

            let converterType =
                typedefof<IndexMapValueConverter<'S, 'T>>.MakeGenericType (types)

            Activator.CreateInstance(converterType, defaultValue) :?> JsonConverter
