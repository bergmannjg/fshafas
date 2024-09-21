namespace FsHafas.Api

open FsHafas.Parser
open FsHafas.Client

#if !FABLE_COMPILER
module internal ConverterEx =

    open System
    open System.Text.Json.Serialization
    open System.Text.Json

    open Converter

    type HintStatusWarningValueConverter(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverter<HintStatusWarning>()

        let converter = U3EraseValueConverter<Hint, Status, Warning>(uc)

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            match converter.Read(&reader, _typ, options) with
            | (U3.Case1 s) -> (HintStatusWarning.Hint s)
            | (U3.Case2 s) -> (HintStatusWarning.Status s)
            | (U3.Case3 s) -> (HintStatusWarning.Warning s)

        override this.Write(writer: Utf8JsonWriter, value: HintStatusWarning, options: JsonSerializerOptions) =
            raise (System.NotImplementedException(""))

    type HintStatusWarningConverter(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool = t.Name = "HintStatusWarning"

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            Activator.CreateInstance(typedefof<HintStatusWarningValueConverter>, uc, acceptEmptyObjectAsNullValue)
            :?> JsonConverter

    type StationStopValueConverter(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverter<StationStop>()

        let converter = U2EraseValueConverter<Station, Stop>(uc)

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            match converter.Read(&reader, _typ, options) with
            | (U2.Case1 s) -> (StationStop.Station s)
            | (U2.Case2 s) -> (StationStop.Stop s)

        override this.Write(writer: Utf8JsonWriter, value: StationStop, options: JsonSerializerOptions) =
            raise (System.NotImplementedException(""))

    type StationStopConverter(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool = t.Name = "StationStop"

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            Activator.CreateInstance(typedefof<StationStopValueConverter>, uc, acceptEmptyObjectAsNullValue)
            :?> JsonConverter

    type StopLocationValueConverter(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverter<StopLocation>()

        let converter = U2EraseValueConverter<Stop, Location>(uc)

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            match converter.Read(&reader, _typ, options) with
            | (U2.Case1 s) -> (StopLocation.Stop s)
            | (U2.Case2 s) -> (StopLocation.Location s)

        override this.Write(writer: Utf8JsonWriter, value: StopLocation, options: JsonSerializerOptions) =
            raise (System.NotImplementedException(""))

    type StopLocationConverter(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool = t.Name = "StopLocation"

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            Activator.CreateInstance(typedefof<StopLocationValueConverter>, uc, acceptEmptyObjectAsNullValue)
            :?> JsonConverter

    type StationStopLocationOptionValueConverter(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverter<StationStopLocation option>()

        let converter =
            OptionU3EraseValueConverter<Station, Stop, Location>(uc, acceptEmptyObjectAsNullValue)

        override this.Read(reader: byref<Utf8JsonReader>, _typ: Type, options: JsonSerializerOptions) =
            match converter.Read(&reader, _typ, options) with
            | Some(U3.Case1 s) -> Some(StationStopLocation.Station s)
            | Some(U3.Case2 s) -> Some(StationStopLocation.Stop s)
            | Some(U3.Case3 s) -> Some(StationStopLocation.Location s)
            | None -> None

        override this.Write(writer: Utf8JsonWriter, value: StationStopLocation option, options: JsonSerializerOptions) =
            raise (System.NotImplementedException(""))

    type StationStopLocationOptionConverter(uc: UnionCaseSelection, acceptEmptyObjectAsNullValue: bool) =
        inherit JsonConverterFactory()

        override this.CanConvert(t: Type) : bool =
            t.IsGenericType
            && t.Name = "FSharpOption`1"
            && t.GenericTypeArguments.[0].Name = "StationStopLocation"

        override this.CreateConverter(typeToConvert: Type, _options: JsonSerializerOptions) : JsonConverter =
            Activator.CreateInstance(
                typedefof<StationStopLocationOptionValueConverter>,
                uc,
                acceptEmptyObjectAsNullValue
            )
            :?> JsonConverter
#endif

/// <exclude>Parser</exclude>
module Parser =

    open System

    open FsHafas.Endpoint

    let internal defaultCommonData: CommonData =
        { operators = Array.empty
          locations = Array.empty
          lines = Array.empty
          hints = Array.empty
          icons = Array.empty
          polylines = Array.empty }

    let defaultOptions: Options =
        { remarks = true
          stopovers = true
          polylines = true
          scheduledDays = false
          subStops = true
          entrances = true
          linesOfStops = true
          firstClass = false }

#if !FABLE_COMPILER
    let Deserialize<'a> (response: string, acceptEmptyObjectAsNullValue: bool) =
        Serializer.addConverters (
            [| FsHafas.Api.Converter.UnionConverter<ProductTypeMode>()
               FsHafas.Api.Converter.U2EraseConverter<Station, Stop>(Converter.UnionCaseSelection.ByTagName "type")
               ConverterEx.HintStatusWarningConverter(
                   Converter.UnionCaseSelection.ByTagName "type",
                   acceptEmptyObjectAsNullValue
               )
               ConverterEx.StationStopConverter(
                   Converter.UnionCaseSelection.ByTagName "type",
                   acceptEmptyObjectAsNullValue
               )
               ConverterEx.StopLocationConverter(
                   Converter.UnionCaseSelection.ByTagName "type",
                   acceptEmptyObjectAsNullValue
               )
               ConverterEx.StationStopLocationOptionConverter(
                   Converter.UnionCaseSelection.ByTagName "type",
                   acceptEmptyObjectAsNullValue
               )
               FsHafas.Api.Converter.IndexMapConverter<string, bool>(false) |]
        )

        Serializer.Deserialize<'a>(response)
#endif

    let internal createContext
        (profile: FsHafas.Endpoint.Profile)
        (opt: Options)
        (res: FsHafas.Raw.RawResult)
        : Context =
        { profile = profile
          opt = opt
          common = defaultCommonData
          res = res }

    let internal parseCommon
        (profile: FsHafas.Endpoint.Profile)
        (opt: Options)
        (common: FsHafas.Raw.RawCommon option)
        (res: FsHafas.Raw.RawResult option)
        =
        match res, common with
        | Some(res), Some(common) ->
            let ctx = createContext profile opt res

            { ctx with
                common = ctx.profile.parseCommon ctx common }
            |> Some
        | Some(res), None ->
            let ctx = createContext profile opt res

            ctx |> Some
        | _ -> None

    let private parseRealtimeDataUpdatedAtFromPlanrtTS (planrtTS: string option) =
        match planrtTS with
        | Some planrtTS ->
            match System.Int32.TryParse planrtTS with
            | true, i -> Some i
            | false, _ -> None
        | None -> None

    let parseRealtimeDataUpdatedAt (res: FsHafas.Raw.RawResult option) =
        match res with
        | Some res -> parseRealtimeDataUpdatedAtFromPlanrtTS res.planrtTS
        | None -> None

    let internal parseLocation (locL: FsHafas.Raw.RawLoc option) (ctx: Context option) =
        match ctx, locL with
        | Some(ctx), Some(locL) ->
            let locs = [| locL |] |> ctx.profile.parseLocations ctx

            locs.[0]
        | _ -> StationStopLocation.Location Default.Location

    let internal parseLocations (locL: FsHafas.Raw.RawLoc[] option) (ctx: Context option) =
        match locL, ctx with
        | Some locL, Some ctx -> locL |> ctx.profile.parseLocations ctx
        | _ -> Array.empty

    let parseLocationsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (locL: FsHafas.Raw.RawLoc[] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseLocations locL (parseCommon profile options res.common (Some res))

    let internal parseMovements (jnyL: FsHafas.Raw.RawJny[] option) (ctx: Context option) =
        match ctx, jnyL with
        | Some ctx, Some jnyL -> jnyL |> Array.map (ctx.profile.parseMovement ctx)
        | _ -> Array.empty

    let parseMovementsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (jnyL: FsHafas.Raw.RawJny[] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        { Default.Radar with
            movements = parseMovements jnyL (parseCommon profile options res.common (Some res)) |> Some
            realtimeDataUpdatedAt = parseRealtimeDataUpdatedAtFromPlanrtTS res.planrtTS }

    let private addToMap (m: Map<int, array<int>>) (p: FsHafas.Raw.RawPos) =
        if m.ContainsKey p.dur then
            let l = m.[p.dur]
            let m0 = m.Remove p.dur
            m0.Add(p.dur, Array.append l [| p.locX |])
        else
            m.Add(p.dur, [| p.locX |])

    let private getLocations (ctx: Context) (locXs: array<int>) =
        locXs
        |> Array.map (fun locx -> Common.getElementAt locx ctx.common.locations)
        |> Array.choose id

    let internal parseDurations (posL: FsHafas.Raw.RawPos[]) (ctx: Context option) =
        match ctx with
        | Some(ctx) ->
            posL
            |> Array.fold addToMap Map.empty
            |> Map.toArray
            |> Array.map (fun (d, locXs) ->
                { duration = d
                  stations = getLocations ctx locXs })
        | _ -> Array.empty

    let parseDurationsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (posL: FsHafas.Raw.RawPos[])
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        { Default.DurationsWithRealtimeData with
            reachable = parseDurations posL (parseCommon profile options res.common (Some res))
            realtimeDataUpdatedAt = parseRealtimeDataUpdatedAtFromPlanrtTS res.planrtTS }

    let internal parseJourney (outConL: FsHafas.Raw.RawOutCon[] option) (ctx: Context option) =
        match ctx, outConL with
        | Some(ctx), Some(outConL) when outConL.Length > 0 ->
            { Default.JourneyWithRealtimeData with
                journey = ctx.profile.parseJourney ctx outConL.[0] }
        | _ -> Default.JourneyWithRealtimeData

    let internal parseJourneysArray (outConL: FsHafas.Raw.RawOutCon[] option) (ctx: Context option) =
        match ctx, outConL with
        | Some(ctx), Some(outConL) ->
            let journeys = outConL |> Array.map (fun o -> ctx.profile.parseJourney ctx o)

            journeys
        | _ -> [||]

    let parseJourneysArrayFromResult
        (profile: FsHafas.Endpoint.Profile)
        (outConL: FsHafas.Raw.RawOutCon[] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseJourneysArray outConL (parseCommon profile options res.common (Some res))

    let internal parseJourneys (outConL: FsHafas.Raw.RawOutCon[] option) (ctx: Context option) =
        match ctx, outConL with
        | Some(ctx), Some(outConL) ->
            let journeys = outConL |> Array.map (fun o -> ctx.profile.parseJourney ctx o)

            { Default.Journeys with
                earlierRef = ctx.res.outCtxScrB
                laterRef = ctx.res.outCtxScrF
                realtimeDataUpdatedAt = parseRealtimeDataUpdatedAtFromPlanrtTS ctx.res.planrtTS
                journeys = Some journeys }
        | _ -> Default.Journeys

    let parseJourneysFromResult
        (profile: FsHafas.Endpoint.Profile)
        (outConL: FsHafas.Raw.RawOutCon[] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseJourneys outConL (parseCommon profile options res.common (Some res))

    let internal parseTrip (journey: FsHafas.Raw.RawJny option) (ctx: Context option) =
        match ctx, journey with
        | Some(ctx), Some(journey) -> journey |> ctx.profile.parseTrip ctx
        | _ -> raise (System.ArgumentException("parseTrip failed"))

    let parseTripFromResult
        (profile: FsHafas.Endpoint.Profile)
        (journey: FsHafas.Raw.RawJny option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        { Default.TripWithRealtimeData with
            trip = parseTrip journey (parseCommon profile options res.common (Some res))
            realtimeDataUpdatedAt = parseRealtimeDataUpdatedAtFromPlanrtTS res.planrtTS }

    let internal parseTrips (journeys: FsHafas.Raw.RawJny[] option) (ctx: Context option) =
        match ctx, journeys with
        | Some(ctx), Some(journeys) -> journeys |> Array.map (fun j -> ctx.profile.parseTrip ctx j)
        | _ -> raise (System.ArgumentException("parseTrip failed"))

    let internal parseLine (l: FsHafas.Raw.RawLine) (ctx: Context) =
        let dirl =
            match ctx.res.common with
            | Some common ->
                match common.dirL with
                | Some dirL -> dirL
                | None -> Array.empty
            | None -> Array.empty

        let directions =
            l.dirRefL
            |> Option.fold (fun _ d -> d) [||]
            |> Array.map (fun d -> Common.getElementAt d dirl)
            |> Array.choose id
            |> Array.map (fun d -> d.txt)

        match Common.getElementAt l.prodX ctx.common.lines with
        | Some line ->
            Some
                { line with
                    directions = Some directions }
        | None -> None

    let internal parseLines (lines: FsHafas.Raw.RawLine[] option) (ctx: Context option) =
        match ctx, lines with
        | Some(ctx), Some(lines) -> lines |> Array.map (fun l -> parseLine l ctx) |> Array.choose id
        | Some(ctx), None -> Array.empty
        | _ -> raise (System.ArgumentException("parseLines failed"))

    let parseLinesFromResult
        (profile: FsHafas.Endpoint.Profile)
        (lines: FsHafas.Raw.RawLine[] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        { Default.LinesWithRealtimeData with
            lines = parseLines lines (parseCommon profile options res.common (Some res)) |> Some
            realtimeDataUpdatedAt = parseRealtimeDataUpdatedAtFromPlanrtTS res.planrtTS }

    let internal parseWarnings (msgL: FsHafas.Raw.RawHim[] option) (ctx: Context option) =
        match ctx, msgL with
        | Some(ctx), Some(msgL) -> msgL |> Array.map (fun j -> ctx.profile.parseWarning ctx j)
        | _ -> raise (System.ArgumentException("parseWarnings failed"))

    let parseWarningsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (msgL: FsHafas.Raw.RawHim[] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        { Default.WarningsWithRealtimeData with
            remarks = parseWarnings msgL (parseCommon profile options res.common (Some res))
            realtimeDataUpdatedAt = parseRealtimeDataUpdatedAtFromPlanrtTS res.planrtTS }

    let ParseIsoString (datetime: string) =
        let year = datetime.Substring(0, 4) |> int
        let month = datetime.Substring(5, 2) |> int
        let day = datetime.Substring(8, 2) |> int
        let hour = datetime.Substring(11, 2) |> int
        let minute = datetime.Substring(14, 2) |> int

#if FABLE_PY
        // workaround: missing code DateTimeOffset
        System.DateTime(year, month, day, hour, minute, 0)
#else
        let tzOffset = datetime.Substring(20, 2) |> int |> (*) 60

        System
            .DateTimeOffset(year, month, day, hour, minute, 0, System.TimeSpan(tzOffset / 60, 0, 0))
            .DateTime
#endif

    let internal parseDeparturesArrivals (``type``: string) (jnyL: FsHafas.Raw.RawJny[] option) (ctx: Context option) =

        let projection =
            fun (d: Alternative) ->
                try
                    match d.``when`` with
                    | Some v -> ParseIsoString v
                    | None -> System.DateTime.Now
                with ex ->
                    printfn "%s" ex.Message
                    System.DateTime.Now

        match ctx, jnyL with
        | Some ctx, Some jnyL ->
            try
                let parse =
                    if ``type`` = ArrivalOrDeparture.DEP then
                        ctx.profile.parseDeparture
                    else
                        ctx.profile.parseArrival

                jnyL
                |> Array.map (fun jny -> parse ctx jny)
                |> FsHafas.Extensions.ArrayEx.sortBy projection
            with ex ->
                printfn "%s" ex.Message
                Array.empty

        | _ -> Array.empty

    let parseDeparturesArrivalsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (``type``: string)
        (jnyL: FsHafas.Raw.RawJny[] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        : Departures =
        { departures = parseDeparturesArrivals ``type`` jnyL (parseCommon profile options res.common (Some res))
          realtimeDataUpdatedAt = parseRealtimeDataUpdatedAtFromPlanrtTS res.planrtTS }

    let internal parseServerInfo (res: FsHafas.Raw.RawResult option) (ctx: Context option) =
        match ctx, res with
        | Some(ctx), Some(res) ->

            let serverTime =
                match res.sD with
                | Some sD -> ctx.profile.parseDateTime ctx sD res.sT None
                | None -> None

            { Default.ServerInfo with
                hciVersion = res.hciVersion
                timetableStart = res.fpB
                timetableEnd = res.fpE
                serverTime = serverTime
                realtimeDataUpdatedAt = parseRealtimeDataUpdatedAtFromPlanrtTS res.planrtTS }
        | _ -> raise (System.ArgumentException("ServerInfo failed"))
