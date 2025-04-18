namespace FsHafas.Api

open System
open FsHafas.Client

/// <summary>F# async based interface corresponding to hafas-client</summary>
type HafasAsyncClient(profile: FsHafas.Endpoint.Profile) =

    let log msg o = FsHafas.Client.Log.Print msg o

    let cfg =
        match profile.cfg with
        | Some cfg -> cfg
        | None -> raise (System.ArgumentException("profile.cfg"))

    let baseRequest =
        match profile.baseRequest with
        | Some baseRequest -> baseRequest
        | None -> raise (System.ArgumentException("profile.baseRequest"))

    let httpClient =
        FsHafas.Api.HafasRawClient(
            (profile :> FsHafas.Client.Profile).endpoint,
            profile.addChecksum,
            profile.addMicMac,
            profile.salt,
            cfg,
            profile.transformReq,
            baseRequest
        )

    let enabled (value: bool option) =
        match value with
        | Some value -> value
        | None -> false

    let runAsyncAllowNoMatch (computation: Async<'T>) : Async<'T option> =
        async {
            match! computation |> Async.Catch with
            | Choice1Of2 r -> return Some r
            | Choice2Of2 ext ->
                match ext with
                | :? (HafasError) as ex ->
                    if ex.code = HafasError.CodeNoMatch then
                        return None
                    else
                        return raise ex
                | e -> return raise e
        }

    interface IDisposable with
        member __.Dispose() = httpClient.Dispose()

    static member initSerializer() =
#if FABLE_COMPILER
        ()
#else
        Serializer.addConverters ([| Converter.UnionConverter<FsHafas.Client.ProductTypeMode>() |])
#endif

    static member productsOfFilter (profile: FsHafas.Endpoint.Profile) (filter: ProductType -> Boolean) : Products =
        (profile :> FsHafas.Client.Profile).products
        |> Array.filter filter
        |> Array.fold
            (fun m p ->
                m.[p.id] <- true
                m)
            (Products(false))

    static member productsOfMode (profile: FsHafas.Endpoint.Profile) (mode: ProductTypeMode) : Products =
        HafasAsyncClient.productsOfFilter profile (fun p -> p.mode = mode && p.name <> "Tram")

#if !FABLE_COMPILER
    static member toTask<'a>(async: Async<'a>) : Threading.Tasks.Task<'a> = Async.StartAsTask(async)
#endif

    interface IAsyncClient with
        member __.AsyncJourneys
            (from: U4<string, Station, Stop, Location>)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysOptions option)
            : Async<Journeys> =

            async {
                let! (common, res, outConl) =
                    httpClient.AsyncTripSearch (Format.journeyRequest profile from ``to`` opt) (fun cfg ->
                        profile.transformCfg (opt |> Option.bind (fun v -> v.routingMode)) cfg)

                return
                    Parser.parseJourneys
                        outConl
                        (Parser.parseCommon profile (MergeOptions.JourneysOptions Parser.defaultOptions opt) common res)
            }

        // minimal support for BestPriceSearch, see https://github.com/public-transport/hafas-client/issues/291
        // todo: parse outDaySegL in FsHafas.Raw.RawResult
        member __.AsyncBestPrices
            (from: U4<string, Station, Stop, Location>)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysOptions option)
            : Async<Journeys> =

            async {
                if profile._endpoint.Contains "reiseauskunft.bahn.de" then // guess db profile
                    let! (common, res, outConl) =
                        httpClient.AsyncBestPriceSearch(Format.journeyRequest profile from ``to`` opt)

                    return
                        Parser.parseJourneys
                            outConl
                            (Parser.parseCommon
                                profile
                                (MergeOptions.JourneysOptions Parser.defaultOptions opt)
                                common
                                res)
                else
                    return Default.Journeys
            }


        member __.AsyncRefreshJourney
            (refreshToken: string)
            (opt: RefreshJourneyOptions option)
            : Async<JourneyWithRealtimeData> =

            async {
                if enabled (profile :> FsHafas.Client.Profile).refreshJourney then
                    let! (common, res, outConl) =
                        httpClient.AsyncReconstruction(Format.reconstructionRequest profile refreshToken opt)

                    return Parser.parseJourney outConl (Parser.parseCommon profile Parser.defaultOptions common res)
                else
                    return Default.JourneyWithRealtimeData
            }


        member __.AsyncJourneysFromTrip
            (fromTripId: string)
            (previousStopOver: StopOver)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysFromTripOptions option)
            : Async<Journeys> =

            async {
                if enabled (profile :> FsHafas.Client.Profile).journeysFromTrip then
                    let! (common, res, outConl) =
                        httpClient.AsyncSearchOnTrip(
                            Format.searchOnTripRequest profile fromTripId previousStopOver ``to`` opt
                        )

                    return
                        { Default.Journeys with
                            journeys =
                                Parser.parseJourneysArray
                                    outConl
                                    (Parser.parseCommon
                                        profile
                                        (MergeOptions.JourneysFromTripOptions Parser.defaultOptions opt)
                                        common
                                        res)
                                |> Some
                            realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
                else
                    return Default.Journeys
            }


        member __.AsyncTrip (id: string) (opt: TripOptions option) : Async<TripWithRealtimeData> =

            async {
                if enabled (profile :> FsHafas.Client.Profile).trip then
                    let! (common, res, journey) = httpClient.AsyncJourneyDetails(Format.tripRequest profile id opt)

                    return
                        { Default.TripWithRealtimeData with
                            trip =
                                Parser.parseTrip journey (Parser.parseCommon profile Parser.defaultOptions common res)
                            realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
                else
                    return Default.TripWithRealtimeData
            }


        member __.AsyncDepartures
            (name: U4<string, Station, Stop, Location>)
            (opt: DeparturesArrivalsOptions option)
            : Async<Departures> =
            async {
                let ``type`` = FsHafas.Parser.ArrivalOrDeparture.DEP

                let! (common, res, journey) =
                    httpClient.AsyncStationBoard(Format.stationBoardRequest profile ``type`` name opt)

                return
                    { departures =
                        Parser.parseDeparturesArrivals
                            ``type``
                            journey
                            (Parser.parseCommon profile Parser.defaultOptions common res)
                      realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
            }


        member __.AsyncArrivals
            (name: U4<string, Station, Stop, Location>)
            (opt: DeparturesArrivalsOptions option)
            : Async<Arrivals> =
            async {
                let ``type`` = FsHafas.Parser.ArrivalOrDeparture.ARR

                let! (common, res, journey) =
                    httpClient.AsyncStationBoard(Format.stationBoardRequest profile ``type`` name opt)

                return
                    { arrivals =
                        Parser.parseDeparturesArrivals
                            ``type``
                            journey
                            (Parser.parseCommon profile Parser.defaultOptions common res)
                      realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
            }


        member _.AsyncLocations (name: string) (opt: LocationsOptions option) : Async<array<StationStopLocation>> =

            async {
                let! (common, res, locL) = httpClient.AsyncLocMatch(Format.locationRequest profile name opt)

                return
                    Parser.parseLocations
                        locL
                        (Parser.parseCommon profile (MergeOptions.LocationsOptions Parser.defaultOptions opt) common res)
            }


        member __.AsyncStop (stop: U2<string, Stop>) (opt: StopOptions option) : Async<StationStopLocation> =

            async {
                let! (common, res, locL) = httpClient.AsyncLocDetails(Format.locDetailsRequest profile stop opt)

                return Parser.parseLocation locL (Parser.parseCommon profile Parser.defaultOptions common res)
            }


        member __.AsyncNearby (l: Location) (opt: NearByOptions option) : Async<array<StationStopLocation>> =

            async {
                let! (common, res, locL) = httpClient.AsyncLocGeoPos(Format.locGeoPosRequest profile l opt)

                return
                    Parser.parseLocations
                        locL
                        (Parser.parseCommon profile (MergeOptions.NearByOptions Parser.defaultOptions opt) common res)
            }


        member __.AsyncReachableFrom
            (l: Location)
            (opt: ReachableFromOptions option)
            : Async<DurationsWithRealtimeData> =

            async {
                if enabled (profile :> FsHafas.Client.Profile).reachableFrom then
                    let! (common, res, locL) = httpClient.AsyncLocGeoReach(Format.locGeoReachRequest profile l opt)

                    return
                        { Default.DurationsWithRealtimeData with
                            reachable =
                                Parser.parseDurations locL (Parser.parseCommon profile Parser.defaultOptions common res)
                            realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
                else
                    return Default.DurationsWithRealtimeData
            }


        member __.AsyncRadar (rect: BoundingBox) (opt: RadarOptions option) : Async<Radar> =

            async {
                if enabled (profile :> FsHafas.Client.Profile).radar then
                    let! (common, res, jnyL) =
                        httpClient.AsyncJourneyGeoPos(Format.journeyGeoPosRequest profile rect opt)

                    return
                        { Default.Radar with
                            movements =
                                Parser.parseMovements jnyL (Parser.parseCommon profile Parser.defaultOptions common res)
                                |> Some
                            realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
                else
                    return Default.Radar
            }


        member __.AsyncTripsByName (lineName: string) (opt: TripsByNameOptions option) : Async<TripsWithRealtimeData> =
            async {
                if enabled (profile :> FsHafas.Client.Profile).tripsByName then
                    match!
                        runAsyncAllowNoMatch (
                            httpClient.AsyncJourneyMatch(Format.journeyMatchRequest profile lineName opt)
                        )
                    with
                    | Some(common, res, journey) ->
                        return
                            { Default.TripsWithRealtimeData with
                                trips =
                                    Parser.parseTrips
                                        journey
                                        (Parser.parseCommon profile Parser.defaultOptions common res)
                                realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
                    | None -> return Default.TripsWithRealtimeData
                else
                    return Default.TripsWithRealtimeData
            }


        member __.AsyncRemarks(opt: RemarksOptions option) : Async<WarningsWithRealtimeData> =
            async {
                if enabled (profile :> FsHafas.Client.Profile).remarks then
                    let! (common, res, msgL) = httpClient.AsyncHimSearch(Format.himSearchRequest profile opt)

                    return
                        { Default.WarningsWithRealtimeData with
                            remarks =
                                Parser.parseWarnings msgL (Parser.parseCommon profile Parser.defaultOptions common res)
                            realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
                else
                    return Default.WarningsWithRealtimeData
            }


        member __.AsyncLines (query: string) (opt: LinesOptions option) : Async<LinesWithRealtimeData> =
            async {
                if enabled (profile :> FsHafas.Client.Profile).lines then
                    let! (common, res, lineL) = httpClient.AsyncLineMatch(Format.lineMatchRequest profile query opt)

                    return
                        { Default.LinesWithRealtimeData with
                            lines =
                                Parser.parseLines lineL (Parser.parseCommon profile Parser.defaultOptions common res)
                                |> Some
                            realtimeDataUpdatedAt = Parser.parseRealtimeDataUpdatedAt res }
                else
                    return Default.LinesWithRealtimeData
            }


        member __.AsyncServerInfo(opt: ServerOptions option) : Async<ServerInfo> =
            async {
                let! (common, res) = httpClient.AsyncServerInfo(Format.serverInfoRequest profile opt)

                return Parser.parseServerInfo res (Parser.parseCommon profile Parser.defaultOptions common res)
            }

    member __.distanceOfJourney(j: Journey) =
        FsHafas.Parser.Journey.distanceOfJourney j
