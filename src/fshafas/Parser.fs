namespace FsHafas.Api

open FsHafas.Parser
open FsHafas.Client

#if FABLE_COMPILER
open Fable.Core
#endif

/// <exclude>Parser</exclude>
module Parser =

    open FsHafas.Endpoint

    let internal defaultCommonData: CommonData =
        { operators = Array.empty
          locations = Array.empty
          lines = Array.empty
          hints = Array.empty }

    let defaultProfile =
        { locale = "de-DE"
          timezone = "Europe/Berlin"
          endpoint = ""
          salt = ""
          cfg = None
          baseRequest = None
          products = Array.empty
          trip = None
          radar = None
          refreshJourney = None
          journeysFromTrip = None
          reachableFrom = None
          journeysWalkingSpeed = None
          tripsByName = None
          remarks = None
          lines = None
          journeysOutFrwd = false
          departuresGetPasslist = true
          departuresStbFltrEquiv = true
          formatStation = id
          transformJourneysQuery = (fun _ q -> q)
          parseCommon = Common.parseCommon
          parseArrival = ArrivalOrDeparture.parseArrival
          parseDeparture = ArrivalOrDeparture.parseDeparture
          parseHint = Hint.parseHint
          parsePolyline = Polyline.parsePolyline
          parseLocations = Location.parseLocations
          parseLine = Line.parseLine
          parseJourney = Journey.parseJourney
          parseJourneyLeg = JourneyLeg.parseJourneyLeg
          parseMovement = Movement.parseMovement
          parseOperator = Operator.parseOperator
          parsePlatform = JourneyLeg.parsePlatform
          parseStopover = Stopover.parseStopover
          parseStopovers = Stopover.parseStopovers
          parseTrip = Trip.parseTrip
          parseWhen = When.parseWhen
          parseDateTime = DateTime.parseDateTime
          parseBitmask = ProductsBitmask.parseBitmask
          parseWarning = Warning.parseWarning }

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
    let Deserialize<'a> (response: string) =
        Serializer.addConverters (
            [| FsHafas.Api.Converter.UnionConverter<ProductTypeMode>()
               FsHafas.Api.Converter.U2EraseConverter<Station, Stop>(
                   FsHafas.Api.Converter.UnionCaseSelection.ByTagName "type"
               )
               FsHafas.Api.Converter.U3EraseConverter<Hint, Status, Warning>(
                   FsHafas.Api.Converter.UnionCaseSelection.ByTagName "type"
               )
               FsHafas.Api.Converter.IndexMapConverter<string, bool>(false) |]
        )

        Serializer.Deserialize<'a>(response)
#endif

    let internal createContext (profile: FsHafas.Endpoint.Profile) (opt: Options) (res: FsHafas.Raw.RawResult) : Context =
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
        | Some (res), Some (common) ->
            let ctx = createContext profile opt res

            { ctx with
                  common = ctx.profile.parseCommon ctx common }
            |> Some
        | _ -> None

    let internal parseLocation (locL: FsHafas.Raw.RawLoc option) (ctx: Context option) =
        match ctx, locL with
        | Some (ctx), Some (locL) ->
            let locs =
                [| locL |] |> ctx.profile.parseLocations ctx

            locs.[0]
        | _ -> U3.Case3 Default.Location

    let internal parseLocations (locL: FsHafas.Raw.RawLoc []) (ctx: Context option) =
        match ctx with
        | Some (ctx) -> locL |> ctx.profile.parseLocations ctx
        | _ -> Array.empty

    let parseLocationsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (locL: FsHafas.Raw.RawLoc [])
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseLocations locL (parseCommon profile options res.common (Some res))

    let internal parseMovements (jnyL: FsHafas.Raw.RawJny [] option) (ctx: Context option) =
        match ctx, jnyL with
        | Some ctx, Some jnyL -> jnyL |> Array.map (ctx.profile.parseMovement ctx)
        | _ -> Array.empty

    let parseMovementsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (jnyL: FsHafas.Raw.RawJny [] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseMovements jnyL (parseCommon profile options res.common (Some res))

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

    let internal parseDurations (posL: FsHafas.Raw.RawPos []) (ctx: Context option) =
        match ctx with
        | Some (ctx) ->
            posL
            |> Array.fold addToMap Map.empty
            |> Map.toArray
            |> Array.map
                (fun (d, locXs) ->
                    { duration = Some d
                      stations = getLocations ctx locXs })
        | _ -> Array.empty

    let parseDurationsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (posL: FsHafas.Raw.RawPos [])
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseDurations posL (parseCommon profile options res.common (Some res))

    let internal parseJourney (outConL: FsHafas.Raw.RawOutCon [] option) (ctx: Context option) =
        match ctx, outConL with
        | Some (ctx), Some (outConL) when outConL.Length > 0 -> ctx.profile.parseJourney ctx outConL.[0]
        | _ -> Default.Journey

    let internal parseJourneysArray (outConL: FsHafas.Raw.RawOutCon [] option) (ctx: Context option) =
        match ctx, outConL with
        | Some (ctx), Some (outConL) ->
            let journeys =
                outConL
                |> Array.map (fun o -> ctx.profile.parseJourney ctx o)

            journeys
        | _ -> [||]

    let parseJourneysArrayFromResult
        (profile: FsHafas.Endpoint.Profile)
        (outConL: FsHafas.Raw.RawOutCon [] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseJourneysArray outConL (parseCommon profile options res.common (Some res))

    let internal parseJourneys (outConL: FsHafas.Raw.RawOutCon [] option) (ctx: Context option) =
        match ctx, outConL with
        | Some (ctx), Some (outConL) ->
            let journeys =
                outConL
                |> Array.map (fun o -> ctx.profile.parseJourney ctx o)
                |> Some

            { Default.Journeys with
                  earlierRef = ctx.res.outCtxScrB
                  laterRef = ctx.res.outCtxScrF
                  realtimeDataFrom = ctx.res.planrtTS |> Option.map (fun p -> p |> int)
                  journeys = journeys }
        | _ -> Default.Journeys

    let parseJourneysFromResult
        (profile: FsHafas.Endpoint.Profile)
        (outConL: FsHafas.Raw.RawOutCon [] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseJourneys outConL (parseCommon profile options res.common (Some res))

    let internal parseTrip (journey: FsHafas.Raw.RawJny option) (ctx: Context option) =
        match ctx, journey with
        | Some (ctx), Some (journey) -> journey |> ctx.profile.parseTrip ctx
        | _ -> raise (System.ArgumentException("parseTrip failed"))

    let parseTripFromResult
        (profile: FsHafas.Endpoint.Profile)
        (journey: FsHafas.Raw.RawJny option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseTrip journey (parseCommon profile options res.common (Some res))

    let internal parseTrips (journeys: FsHafas.Raw.RawJny [] option) (ctx: Context option) =
        match ctx, journeys with
        | Some (ctx), Some (journeys) ->
            journeys
            |> Array.map (fun j -> ctx.profile.parseTrip ctx j)
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

    let internal parseLines (lines: FsHafas.Raw.RawLine [] option) (ctx: Context option) =
        match ctx, lines with
        | Some (ctx), Some (lines) ->
            lines
            |> Array.map (fun l -> parseLine l ctx)
            |> Array.choose id
        | Some (ctx), None -> Array.empty
        | _ -> raise (System.ArgumentException("parseLines failed"))

    let parseLinesFromResult
        (profile: FsHafas.Endpoint.Profile)
        (lines: FsHafas.Raw.RawLine [] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseLines lines (parseCommon profile options res.common (Some res))

    let internal parseWarnings (msgL: FsHafas.Raw.RawHim [] option) (ctx: Context option) =
        match ctx, msgL with
        | Some (ctx), Some (msgL) ->
            msgL
            |> Array.map (fun j -> ctx.profile.parseWarning ctx j)
        | _ -> raise (System.ArgumentException("parseWarnings failed"))

    let parseWarningsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (msgL: FsHafas.Raw.RawHim [] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseWarnings msgL (parseCommon profile options res.common (Some res))

    let ParseIsoString (datetime: string) =
        let year = datetime.Substring(0, 4) |> int
        let month = datetime.Substring(5, 2) |> int
        let day = datetime.Substring(8, 2) |> int
        let hour = datetime.Substring(11, 2) |> int
        let minute = datetime.Substring(14, 2) |> int

        let tzOffset =
            datetime.Substring(20, 2) |> int |> (*) 60

        System.DateTimeOffset(year, month, day, hour, minute, 0, System.TimeSpan(tzOffset / 60, 0, 0))

    let internal parseDeparturesArrivals (``type``: string) (jnyL: FsHafas.Raw.RawJny [] option) (ctx: Context option) =
        match ctx, jnyL with
        | Some ctx, Some jnyL ->
            let parse =
                if ``type`` = ArrivalOrDeparture.DEP then
                    ctx.profile.parseDeparture
                else
                    ctx.profile.parseArrival

            jnyL
            |> Array.map (fun jny -> parse ctx jny)
            |> Seq.sortBy
                (fun d ->
                    try
                        ParseIsoString d.``when``.Value
                    with
                    | ex ->
                        printfn "%s" ex.Message
                        System.DateTimeOffset.Now)
            |> Seq.toArray

        | _ -> Array.empty

    let parseDeparturesArrivalsFromResult
        (profile: FsHafas.Endpoint.Profile)
        (``type``: string)
        (jnyL: FsHafas.Raw.RawJny [] option)
        (options: Options)
        (res: FsHafas.Raw.RawResult)
        =
        parseDeparturesArrivals ``type`` jnyL (parseCommon profile options res.common (Some res))

    let internal parseServerInfo (res: FsHafas.Raw.RawResult option) (ctx: Context option) =
        match ctx, res with
        | Some (ctx), Some (res) ->

            let serverTime =
                match res.sD with
                | Some sD -> ctx.profile.parseDateTime ctx sD res.sT None
                | None -> None

            { Default.ServerInfo with
                  timetableStart = res.fpB
                  timetableEnd = res.fpE
                  serverTime = serverTime
                  realtimeDataUpdatedAt = res.planrtTS |> Option.map (fun p -> p |> int) }
        | _ -> raise (System.ArgumentException("ServerInfo failed"))
