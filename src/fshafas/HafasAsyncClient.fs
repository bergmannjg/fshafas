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
        FsHafas.Api.HafasRawClient((profile :> FsHafas.Client.Profile).endpoint, profile.salt, cfg, baseRequest)

    let enabled (value: bool option) =
        match value with
        | Some value -> value
        | None -> false

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

    member __.AsyncJourneys
        (from: U4<string, Station, Stop, Location>)
        (``to``: U4<string, Station, Stop, Location>)
        (opt: JourneysOptions option)
        : Async<Journeys> =

        async {
            let! (common, res, outConl) = httpClient.AsyncTripSearch(Format.journeyRequest profile from ``to`` opt)

            return
                Parser.parseJourneys
                    outConl
                    (Parser.parseCommon profile (MergeOptions.JourneysOptions Parser.defaultOptions opt) common res)
        }

    member __.AsyncRefreshJourney (refreshToken: string) (opt: RefreshJourneyOptions option) : Async<Journey> =

        async {
            if enabled (profile :> FsHafas.Client.Profile).refreshJourney then
                let! (common, res, outConl) =
                    httpClient.AsyncReconstruction(Format.reconstructionRequest profile refreshToken opt)

                return Parser.parseJourney outConl (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Default.Journey
        }

    member __.AsyncJourneysFromTrip
        (fromTripId: string)
        (previousStopOver: StopOver)
        (``to``: U4<string, Station, Stop, Location>)
        (opt: JourneysFromTripOptions option)
        : Async<array<Journey>> =

        async {
            if
                enabled
                    (profile :> FsHafas.Client.Profile)
                        .journeysFromTrip then
                let! (common, res, outConl) =
                    httpClient.AsyncSearchOnTrip(
                        Format.searchOnTripRequest profile fromTripId previousStopOver ``to`` opt
                    )

                return
                    Parser.parseJourneysArray
                        outConl
                        (Parser.parseCommon
                            profile
                            (MergeOptions.JourneysFromTripOptions Parser.defaultOptions opt)
                            common
                            res)
            else
                return [||]
        }

    member __.AsyncTrip (id: string) (name: string) (opt: TripOptions option) : Async<Trip> =

        async {
            if enabled (profile :> FsHafas.Client.Profile).trip then
                let! (common, res, journey) = httpClient.AsyncJourneyDetails(Format.tripRequest profile id name opt)

                return Parser.parseTrip journey (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Default.Trip
        }

    member __.AsyncDepartures
        (name: U4<string, Station, Stop, Location>)
        (opt: DeparturesArrivalsOptions option)
        : Async<array<Alternative>> =
        async {
            let ``type`` = FsHafas.Parser.ArrivalOrDeparture.DEP

            let! (common, res, journey) =
                httpClient.AsyncStationBoard(Format.stationBoardRequest profile ``type`` name opt)

            return
                Parser.parseDeparturesArrivals
                    ``type``
                    journey
                    (Parser.parseCommon profile Parser.defaultOptions common res)
        }

    member __.AsyncArrivals
        (name: U4<string, Station, Stop, Location>)
        (opt: DeparturesArrivalsOptions option)
        : Async<array<Alternative>> =
        async {
            let ``type`` = FsHafas.Parser.ArrivalOrDeparture.ARR

            let! (common, res, journey) =
                httpClient.AsyncStationBoard(Format.stationBoardRequest profile ``type`` name opt)

            return
                Parser.parseDeparturesArrivals
                    ``type``
                    journey
                    (Parser.parseCommon profile Parser.defaultOptions common res)
        }

    member __.AsyncLocations (name: string) (opt: LocationsOptions option) : Async<array<U3<Station, Stop, Location>>> =

        async {
            let! (common, res, locL) = httpClient.AsyncLocMatch(Format.locationRequest profile name opt)

            return
                Parser.parseLocations
                    locL
                    (Parser.parseCommon profile (MergeOptions.LocationsOptions Parser.defaultOptions opt) common res)
        }

    member __.AsyncStop (stop: U2<string, Stop>) (opt: StopOptions option) : Async<U3<Station, Stop, Location>> =

        async {
            let! (common, res, locL) = httpClient.AsyncLocDetails(Format.locDetailsRequest profile stop opt)

            return Parser.parseLocation locL (Parser.parseCommon profile Parser.defaultOptions common res)
        }

    member __.AsyncNearby (l: Location) (opt: NearByOptions option) : Async<array<U3<Station, Stop, Location>>> =

        async {
            let! (common, res, locL) = httpClient.AsyncLocGeoPos(Format.locGeoPosRequest profile l opt)

            return
                Parser.parseLocations
                    locL
                    (Parser.parseCommon profile (MergeOptions.NearByOptions Parser.defaultOptions opt) common res)
        }

    member __.AsyncReachableFrom (l: Location) (opt: ReachableFromOptions option) : Async<array<Duration>> =

        async {
            if enabled (profile :> FsHafas.Client.Profile).reachableFrom then
                let! (common, res, locL) = httpClient.AsyncLocGeoReach(Format.locGeoReachRequest profile l opt)

                return Parser.parseDurations locL (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncRadar (rect: BoundingBox) (opt: RadarOptions option) : Async<array<Movement>> =

        async {
            if enabled (profile :> FsHafas.Client.Profile).radar then
                let! (common, res, jnyL) = httpClient.AsyncJourneyGeoPos(Format.journeyGeoPosRequest profile rect opt)

                return Parser.parseMovements jnyL (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncTripsByName (lineName: string) (opt: TripsByNameOptions option) : Async<array<Trip>> =
        async {
            if enabled (profile :> FsHafas.Client.Profile).tripsByName then
                let! (common, res, journey) =
                    httpClient.AsyncJourneyMatch(Format.journeyMatchRequest profile lineName opt)

                return Parser.parseTrips journey (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncRemarks(opt: RemarksOptions option) : Async<array<Warning>> =
        async {
            if enabled (profile :> FsHafas.Client.Profile).remarks then
                let! (common, res, msgL) = httpClient.AsyncHimSearch(Format.himSearchRequest profile opt)

                return Parser.parseWarnings msgL (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncLines (query: string) (opt: LinesOptions option) : Async<array<Line>> =
        async {
            if enabled (profile :> FsHafas.Client.Profile).lines then
                let! (common, res, lineL) = httpClient.AsyncLineMatch(Format.lineMatchRequest profile query opt)

                return Parser.parseLines lineL (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncServerInfo(opt: ServerOptions option) : Async<ServerInfo> =
        async {
            let! (common, res) = httpClient.AsyncServerInfo(Format.serverInfoRequest profile opt)

            return Parser.parseServerInfo res (Parser.parseCommon profile Parser.defaultOptions common res)
        }

    member __.distanceOfJourney(j: Journey) =
        FsHafas.Parser.Journey.distanceOfJourney j
