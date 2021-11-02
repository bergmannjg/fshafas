namespace FsHafas.Api

#if FABLE_COMPILER
open Fable.Core
open Fable.Core.JsInterop
#endif

open FsHafas.Client

/// <summary>JS promise based interface corresponding to hafas-client</summary>
type HafasClient(profile: FsHafas.Client.Profile) =
    let client =
        new FsHafas.Api.HafasAsyncClient(profile :?> FsHafas.Endpoint.Profile)

#if FABLE_COMPILER
    [<Emit("typeof $1")>]
    let jsTypeof (_: obj) : string = jsNative

    [<Emit("$1.type")>]
    let jsTypeField (o: obj) : string = jsNative

    // get cases at runtime
    let makeCaseOfU4 (v: U4<string, Station, Stop, Location>) : U4<string, Station, Stop, Location> =
        if jsTypeof v = "string" then
            U4.Case1(unbox<string> v)
        else if (jsTypeField v) = "station" then
            let s = unbox<Station> v
            U4.Case2 { Default.Station with id = s.id }
        else if (jsTypeField v) = "stop" then
            let s = unbox<Stop> v
            U4.Case3 { Default.Stop with id = s.id }
        else if (jsTypeField v) = "location" then
            let l = unbox<Location> v

            U4.Case4
                { Default.Location with
                    id = l.id
                    poi = l.poi
                    address = l.address
                    latitude = l.latitude
                    longitude = l.longitude }
        else
            raise (System.ArgumentException("string|Station|Stop|Location expected"))

    // get cases at runtime
    let makeCaseOfU2StringStop (v: U2<string, Stop>) : U2<string, Stop> =
        if jsTypeof v = "string" then
            U2.Case1(unbox<string> v)
        else if (jsTypeField v) = "stop" then
            let s = unbox<Stop> v
            U2.Case2 { Default.Stop with id = s.id }
        else
            raise (System.ArgumentException("string|Stop expected"))

    let validateString (v: string) =
        if jsTypeof v <> "string"
           || (unbox<string> v).Length = 0 then
            raise (System.ArgumentException("string expected"))

    // make Location at runtime
    let makeLocation (v: Location) : Location =
        if (jsTypeField v) = "location" then
            let l = unbox<Location> v

            { Default.Location with
                id = l.id
                latitude = l.latitude
                longitude = l.longitude
                address = l.address }
        else
            raise (System.ArgumentException("Location expected"))

    let validateBoundingBox (v: BoundingBox) =
        if jsTypeof v.north <> "number"
           || jsTypeof v.west <> "number"
           || jsTypeof v.south <> "number"
           || jsTypeof v.east <> "number" then
            raise (System.ArgumentException("BoundingBox expected"))
#endif

    interface FsHafas.Client.HafasClient with

        member __.journeys
            (from: U4<string, Station, Stop, Location>)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysOptions option)
            =
#if FABLE_COMPILER
            client.AsyncJourneys(makeCaseOfU4 from) (makeCaseOfU4 ``to``) opt
            |> Async.StartAsPromise
#else
            client.AsyncJourneys from ``to`` opt
#endif

        member __.refreshJourney (refreshToken: string) (opt: RefreshJourneyOptions option) =
#if FABLE_COMPILER
            validateString refreshToken

            client.AsyncRefreshJourney refreshToken opt
            |> Async.StartAsPromise
#else
            client.AsyncRefreshJourney refreshToken opt
#endif

        member __.trip (id: string) (name: string) (opt: TripOptions option) =
#if FABLE_COMPILER
            validateString id

            client.AsyncTrip id name opt
            |> Async.StartAsPromise
#else
            client.AsyncTrip id name opt
#endif

        member __.departures (id: U4<string, Station, Stop, Location>) (opt: DeparturesArrivalsOptions option) =
#if FABLE_COMPILER
            client.AsyncDepartures(makeCaseOfU4 id) opt
            |> Async.StartAsPromise
#else
            client.AsyncDepartures id opt
#endif

        member __.arrivals (id: U4<string, Station, Stop, Location>) (opt: DeparturesArrivalsOptions option) =
#if FABLE_COMPILER
            client.AsyncArrivals(makeCaseOfU4 id) opt
            |> Async.StartAsPromise
#else
            client.AsyncArrivals id opt
#endif

#if FABLE_COMPILER
        member __.journeysFromTrip
            (tripId: string)
            (prevStopOver: StopOver)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysFromTripOptions option)
            : JS.Promise<array<Journey>> =
            client.AsyncJourneysFromTrip tripId prevStopOver (makeCaseOfU4 ``to``) opt
            |> Async.StartAsPromise
#else
        member __.journeysFromTrip
            (tripId: string)
            (prevStopOver: StopOver)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysFromTripOptions option)
            : Async<array<Journey>> =
            client.AsyncJourneysFromTrip tripId prevStopOver ``to`` opt
#endif

        member __.locations (name: string) (opt: LocationsOptions option) =
#if FABLE_COMPILER
            validateString name

            client.AsyncLocations name opt
            |> Async.StartAsPromise
#else
            client.AsyncLocations name opt
#endif

        member __.stop (stop: U2<string, Stop>) (opt: StopOptions option) =
#if FABLE_COMPILER
            client.AsyncStop(makeCaseOfU2StringStop stop) opt
            |> Async.StartAsPromise
#else
            client.AsyncStop stop opt
#endif

        member __.nearby (location: Location) (opt: NearByOptions option) =
#if FABLE_COMPILER
            client.AsyncNearby(makeLocation location) opt
            |> Async.StartAsPromise
#else
            client.AsyncNearby location opt
#endif

        member __.reachableFrom (location: Location) (opt: ReachableFromOptions option) =
#if FABLE_COMPILER
            client.AsyncReachableFrom(makeLocation location) opt
            |> Async.StartAsPromise
#else
            client.AsyncReachableFrom location opt
#endif

        member __.radar (x: BoundingBox) (opt: RadarOptions option) =
#if FABLE_COMPILER
            validateBoundingBox x
            client.AsyncRadar x opt |> Async.StartAsPromise
#else
            client.AsyncRadar x opt
#endif

        member __.tripsByName (name: string) (opt: TripsByNameOptions option) =
#if FABLE_COMPILER
            validateString name

            client.AsyncTripsByName name opt
            |> Async.StartAsPromise
#else
            client.AsyncTripsByName name opt
#endif

        member __.remarks(opt: RemarksOptions option) =
#if FABLE_COMPILER
            client.AsyncRemarks opt |> Async.StartAsPromise
#else
            client.AsyncRemarks opt
#endif

        member __.lines (query: string) (opt: LinesOptions option) =
#if FABLE_COMPILER
            validateString query

            client.AsyncLines query opt
            |> Async.StartAsPromise
#else
            client.AsyncLines query opt
#endif

        member __.serverInfo(opt: ServerOptions option) =
#if FABLE_COMPILER
            client.AsyncServerInfo opt |> Async.StartAsPromise
#else
            client.AsyncServerInfo opt
#endif
