namespace FsHafas.Api

open FsHafas.Client
open System
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Core

module internal Converter =

    open System
    open System.Text.Json.Serialization
    open System.Text.Json

    let private emptyJsonObject = "{}"

    type UnionValueConverter<'a>() =
        inherit JsonConverter<'a>()

        static member fromString<'a>(s: string) =
            match FSharpType.GetUnionCases typeof<'a>
                  |> Array.filter (fun case -> String.Compare(case.Name, s, StringComparison.OrdinalIgnoreCase) = 0)
                with
            | [| case |] -> Some(FSharpValue.MakeUnion(case, [||]) :?> 'a)
            | _ -> None

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            let s = JsonSerializer.Deserialize<string>(&reader, options)

            match UnionValueConverter.fromString<'a> s with
            | Some v -> v
            | None -> failwith (sprintf "connot convert value '%s'" s)

        override this.Write(writer: Utf8JsonWriter, value: 'a, options: JsonSerializerOptions) =
            JsonSerializer.Serialize(writer, value.ToString(), options)

    type UnionConverter<'a>() =
        inherit JsonConverterFactory()
        override this.CanConvert(t: Type) : bool = t.Name = typedefof<'a>.Name

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            let converterType =
                typedefof<UnionValueConverter<_>>.MakeGenericType (typeToConvert)

            Activator.CreateInstance(converterType) :?> JsonConverter

    type UnionCaseSelection =
        | Disabled
        | ByTagName of string

    /// tag value is typename

    let private readJsonObjectWithStringProperty (reader: byref<Utf8JsonReader>, name: string) =
        use jsonDocument = JsonDocument.ParseValue(&reader)

        let value =
            match jsonDocument.RootElement.TryGetProperty name with
            | true, jsonElement -> jsonElement.GetString()
            | _ -> raise (JsonException("Property not found: " + name))

        let jsonObject = jsonDocument.RootElement.GetRawText()

        (value, jsonObject)

    let private maybeReadJsonObjectWithStringProperty
        (
            reader: byref<Utf8JsonReader>,
            name: string,
            acceptEmptyObjectAsNullValue: bool
        ) =
        use jsonDocument = JsonDocument.ParseValue(&reader)

        let value =
            match jsonDocument.RootElement.TryGetProperty name with
            | true, jsonElement -> jsonElement.GetString()
            | _ ->
                if acceptEmptyObjectAsNullValue
                   && jsonDocument.RootElement.GetRawText() = emptyJsonObject then
                    jsonDocument.RootElement.GetRawText()
                else
                    raise (JsonException("Property not found: " + name))

        let jsonObject = jsonDocument.RootElement.GetRawText()

        (value, jsonObject)

    type U2EraseValueConverter<'S, 'T>(uc: UnionCaseSelection) =
        inherit JsonConverter<U2<'S, 'T>>()

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())

            match uc with
            | ByTagName unionTagName ->
                let (value, jsonObject) = readJsonObjectWithStringProperty (&reader, unionTagName)

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

            let converterType = typedefof<U2EraseValueConverter<'S, 'T>>.MakeGenericType (types)

            Activator.CreateInstance(converterType, uc) :?> JsonConverter

    type U3EraseValueConverter<'S, 'T, 'U>(uc: UnionCaseSelection) =
        inherit JsonConverter<U3<'S, 'T, 'U>>()

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())

            match uc with
            | ByTagName unionTagName ->
                let (value, jsonObject) = readJsonObjectWithStringProperty (&reader, unionTagName)

                if value = typedefof<'S>.Name.ToLower () then
                    U3.Case1(Serializer.Deserialize<'S>(jsonObject))
                else if value = typedefof<'T>.Name.ToLower () then
                    U3.Case2(Serializer.Deserialize<'T>(jsonObject))
                else if value = typedefof<'U>.Name.ToLower () then
                    U3.Case3(Serializer.Deserialize<'U>(jsonObject))
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

    type OptionU3EraseValueConverter<'S, 'T, 'U>(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverter<U3<'S, 'T, 'U> option>()

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            if reader.TokenType = JsonTokenType.None
               || reader.TokenType = JsonTokenType.Null then
                None
            else if reader.TokenType <> JsonTokenType.StartObject then
                raise (JsonException())
            else
                match uc with
                | ByTagName unionTagName ->
                    let (value, jsonObject) =
                        maybeReadJsonObjectWithStringProperty (&reader, unionTagName, acceptEmptyObjectAsNullValue)

                    if value = emptyJsonObject then
                        None
                    else if value = typedefof<'S>.Name.ToLower () then
                        Some(U3.Case1(Serializer.Deserialize<'S>(jsonObject)))
                    else if value = typedefof<'T>.Name.ToLower () then
                        Some(U3.Case2(Serializer.Deserialize<'T>(jsonObject)))
                    else if value = typedefof<'U>.Name.ToLower () then
                        Some(U3.Case3(Serializer.Deserialize<'U>(jsonObject)))
                    else
                        raise (JsonException("Invalid tag value encoding: " + value))
                | Disabled -> raise (JsonException("U2EraseValueConverter read disabled"))

        override this.Write(writer: Utf8JsonWriter, value: U3<'S, 'T, 'U> option, options: JsonSerializerOptions) =
            raise (System.NotImplementedException(""))

    type OptionU3EraseConverter<'S, 'T, 'U>(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool =
            t.IsGenericType
            && t.Name = "FSharpOption`1"
            && t.GenericTypeArguments.[0].Name = "U3`3"
            && t.GenericTypeArguments.[0].GenericTypeArguments.[0] = typedefof<'S>
            && t.GenericTypeArguments.[0].GenericTypeArguments.[1] = typedefof<'T>
            && t.GenericTypeArguments.[0].GenericTypeArguments.[2] = typedefof<'U>

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            let types =
                typeToConvert.GenericTypeArguments.[0]
                    .GenericTypeArguments
                |> Array.take 3

            let converterType =
                typedefof<OptionU3EraseValueConverter<'S, 'T, 'U>>.MakeGenericType (types)

            Activator.CreateInstance(converterType, uc, acceptEmptyObjectAsNullValue) :?> JsonConverter

    type U14EraseValueConverter<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm, 'n>(uc: UnionCaseSelection) =
        inherit JsonConverter<U14<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm, 'n>>()

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            raise (System.NotImplementedException(""))

        override this.Write
            (
                writer: Utf8JsonWriter,
                value: U14<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm, 'n>,
                options: JsonSerializerOptions
            ) =
            match value with
            | U14.Case1 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case2 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case3 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case4 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case5 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case6 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case7 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case8 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case9 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case10 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case11 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case12 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case13 v -> JsonSerializer.Serialize(writer, v, options)
            | U14.Case14 v -> JsonSerializer.Serialize(writer, v, options)

    type U14EraseConverter<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm, 'n>(uc: UnionCaseSelection) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool =
            t.IsGenericType
            && t.GetGenericTypeDefinition() = typedefof<U14<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm, 'n>>

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            let types =
                typeToConvert.GetGenericArguments()
                |> Array.take 14

            let converterType =
                typedefof<U14EraseValueConverter<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm, 'n>>.MakeGenericType
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
