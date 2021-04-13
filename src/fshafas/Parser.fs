namespace FsHafas.Api

open FsHafas.Parser
open FsHafas.Client

#if FABLE_COMPILER
open Fable.Core
#endif

/// <exclude>Parser</exclude>
module Parser =

    let defaultCommonData : CommonData =
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

    let defaultOptions : Options =
        { remarks = true
          stopovers = true
          polylines = true
          scheduledDays = true
          subStops = true
          entrances = true
          linesOfStops = true
          firstClass = false }

    let createContext (profile: FsHafas.Parser.Profile) (opt: Options) (res: FsHafas.Raw.RawResult) : Context =
        { profile = profile
          opt = opt
          common = defaultCommonData
          res = res }

    let parseCommon
        (profile: FsHafas.Parser.Profile)
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

    let parseLocation (locL: FsHafas.Raw.RawLoc option) (ctx: Context option) =
        match ctx, locL with
        | Some (ctx), Some (locL) ->
            let locs =
                [| locL |] |> ctx.profile.parseLocations ctx

            locs.[0]
        | _ -> U3.Case3 Default.Location

    let parseLocations (locL: FsHafas.Raw.RawLoc []) (ctx: Context option) =
        match ctx with
        | Some (ctx) -> locL |> ctx.profile.parseLocations ctx
        | _ -> Array.empty

    let parseMovements (jnyL: FsHafas.Raw.RawJny [] option) (ctx: Context option) =
        match ctx, jnyL with
        | Some ctx, Some jnyL -> jnyL |> Array.map (ctx.profile.parseMovement ctx)
        | _ -> Array.empty

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

    let parseDurations (posL: FsHafas.Raw.RawPos []) (ctx: Context option) =
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

    let parseJourney (outConL: FsHafas.Raw.RawOutCon [] option) (ctx: Context option) =
        match ctx, outConL with
        | Some (ctx), Some (outConL) when outConL.Length > 0 -> ctx.profile.parseJourney ctx outConL.[0]
        | _ -> Default.Journey

    let parseJourneys (outConL: FsHafas.Raw.RawOutCon [] option) (ctx: Context option) =
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

    let parseTrip (journey: FsHafas.Raw.RawJny option) (ctx: Context option) =
        match ctx, journey with
        | Some (ctx), Some (journey) -> journey |> ctx.profile.parseTrip ctx
        | _ -> raise (System.ArgumentException("parseTrip failed"))

    let parseTrips (journeys: FsHafas.Raw.RawJny [] option) (ctx: Context option) =
        match ctx, journeys with
        | Some (ctx), Some (journeys) ->
            journeys
            |> Array.map (fun j -> ctx.profile.parseTrip ctx j)
        | _ -> raise (System.ArgumentException("parseTrip failed"))

    let parseLine (l: FsHafas.Raw.RawLine) (ctx: Context) =
        let dirl =
            match ctx.res.common with
            | Some common ->
                match common.dirL with
                | Some dirL -> dirL
                | None -> Array.empty
            | None -> Array.empty

        let directions =
            l.dirRefL
            |> Array.map (fun d -> Common.getElementAt d dirl)
            |> Array.choose id
            |> Array.map (fun d -> d.txt)

        match Common.getElementAt l.prodX ctx.common.lines with
        | Some line ->
            Some
                { line with
                      directions = Some directions }
        | None -> None

    let parseLines (lines: FsHafas.Raw.RawLine [] option) (ctx: Context option) =
        match ctx, lines with
        | Some (ctx), Some (lines) ->
            lines
            |> Array.map (fun l -> parseLine l ctx)
            |> Array.choose id
        | Some (ctx), None -> Array.empty
        | _ -> raise (System.ArgumentException("parseLines failed"))

    let parseWarnings (msgL: FsHafas.Raw.RawHim [] option) (ctx: Context option) =
        match ctx, msgL with
        | Some (ctx), Some (msgL) ->
            msgL
            |> Array.map (fun j -> ctx.profile.parseWarning ctx j)
        | _ -> raise (System.ArgumentException("parseWarnings failed"))

    let parseDeparturesArrivals (``type``: string) (jnyL: FsHafas.Raw.RawJny [] option) (ctx: Context option) =
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
                        DateTime.ParseIsoString d.``when``.Value
                    with ex ->
                        printfn "%s" ex.Message
                        System.DateTimeOffset.Now)
            |> Seq.toArray

        | _ -> Array.empty

    let parseServerInfo (res: FsHafas.Raw.RawResult option) (ctx: Context option) =
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
