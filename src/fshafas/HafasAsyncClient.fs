namespace FsHafas.Api
/// <namespacedoc>
///   <summary>FsHafas client apis</summary>
/// </namespacedoc>

open System
open FsHafas.Client
open FsHafas.Client

#if FABLE_COMPILER
open Fable.Core
#endif

/// <summary>F# async based interface corresponding to hafas-client</summary>
type HafasAsyncClient(id: FsHafas.Client.ProfileId) =

    let log msg o = FsHafas.Client.Log.Print msg o

    let profile =
        match id with
        | Db -> FsHafas.Profiles.Db.getProfile ()
        | Bvg -> FsHafas.Profiles.Bvg.getProfile ()
        | Svv -> FsHafas.Profiles.Svv.getProfile ()

    let cfg =
        match profile.cfg with
        | Some cfg -> cfg
        | None -> raise (System.ArgumentException("profile.cfg"))

    let baseRequest =
        match profile.baseRequest with
        | Some baseRequest -> baseRequest
        | None -> raise (System.ArgumentException("profile.baseRequest"))

    let httpClient =
        FsHafas.Api.HafasRawClient(profile.endpoint, profile.salt, cfg, baseRequest)

    let enabled (value: bool option) =
        match value with
        | Some value -> value
        | None -> false

    let getIdU2 (s: U2<string, Station>) =
        match s with
        | U2.Case1 v -> v
        | U2.Case2 v when v.id.IsSome -> v.id.Value
        | _ -> raise (System.ArgumentException(""))

    interface IDisposable with
        member __.Dispose() = httpClient.Dispose()

    static member initSerializer() =
#if FABLE_COMPILER
        ()
#else
        Serializer.addConverters ([| Serializer.UnionConverter<FsHafas.Client.ProductTypeMode>() |])
#endif

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
            if enabled profile.refreshJourney then
                let! (common, res, outConl) =
                    httpClient.AsyncReconstruction(Format.reconstructionRequest profile refreshToken opt)

                return Parser.parseJourney outConl (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Default.Journey
        }

    member __.AsyncTrip (id: string) (name: string) (opt: TripOptions option) : Async<Trip> =

        async {
            if enabled profile.trip then
                let! (common, res, journey) = httpClient.AsyncJourneyDetails(Format.tripRequest profile id name opt)

                return Parser.parseTrip journey (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Default.Trip
        }

    member __.AsyncDepartures
        (name: U2<string, Station>)
        (opt: DeparturesArrivalsOptions option)
        : Async<array<Alternative>> =
        async {
            let ``type`` = FsHafas.Parser.ArrivalOrDeparture.DEP

            let! (common, res, journey) =
                httpClient.AsyncStationBoard(Format.stationBoardRequest profile ``type`` (getIdU2 name) opt)

            return
                Parser.parseDeparturesArrivals
                    ``type``
                    journey
                    (Parser.parseCommon profile Parser.defaultOptions common res)
        }

    member __.AsyncArrivals
        (name: U2<string, Station>)
        (opt: DeparturesArrivalsOptions option)
        : Async<array<Alternative>> =
        async {
            let ``type`` = FsHafas.Parser.ArrivalOrDeparture.ARR

            let! (common, res, journey) =
                httpClient.AsyncStationBoard(Format.stationBoardRequest profile ``type`` (getIdU2 name) opt)

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
            if enabled profile.reachableFrom then
                let! (common, res, locL) = httpClient.AsyncLocGeoReach(Format.locGeoReachRequest profile l opt)

                return Parser.parseDurations locL (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncRadar (rect: BoundingBox) (opt: RadarOptions option) : Async<array<Movement>> =

        async {
            if enabled profile.radar then
                let! (common, res, jnyL) = httpClient.AsyncJourneyGeoPos(Format.journeyGeoPosRequest profile rect opt)

                return Parser.parseMovements jnyL (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncTripsByName (lineName: string) (opt: TripsByNameOptions option) : Async<array<Trip>> =
        async {
            if enabled profile.tripsByName then
                let! (common, res, journey) =
                    httpClient.AsyncJourneyMatch(Format.journeyMatchRequest profile lineName opt)

                return Parser.parseTrips journey (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncRemarks(opt: RemarksOptions option) : Async<array<Warning>> =
        async {
            if enabled profile.remarks then
                let! (common, res, msgL) = httpClient.AsyncHimSearch(Format.himSearchRequest profile opt)

                return Parser.parseWarnings msgL (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncLines (query: string) (opt: LinesOptions option) : Async<array<Line>> =
        async {
            if enabled profile.lines then
                let! (common, res, lineL) = httpClient.AsyncLineMatch(Format.lineMatchRequest profile query opt)

                return Parser.parseLines lineL (Parser.parseCommon profile Parser.defaultOptions common res)
            else
                return Array.empty
        }

    member __.AsyncServerInfo(opt: ServerOptions option) : Async<ServerInfo> =
        async {
            let! (common, res) = httpClient.AsyncServerInfo(new Object())

            return Parser.parseServerInfo res (Parser.parseCommon profile Parser.defaultOptions common res)
        }

    member __.distanceOfJourney(j: Journey) =
        FsHafas.Parser.Journey.distanceOfJourney j
