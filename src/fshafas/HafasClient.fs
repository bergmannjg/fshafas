namespace FsHafas

#if FABLE_COMPILER

open Fable.Core
open Fable.Core.JsInterop
open FsHafas
open Client

type HafasClient(profile: FsHafas.Profile) =
    let client = new Api.HafasAsyncClient(profile)

    [<Emit("typeof $1")>]
    let jsTypeof (_: obj) : string = jsNative

    [<Emit("$1.type")>]
    let jsTypeField (o: obj) : string = jsNative

    // get cases at runtime
    let makeCaseOfU4
        (v: U4<string, Client.Station, Client.Stop, Client.Location>)
        : U4<string, Client.Station, Client.Stop, Client.Location> =
        if jsTypeof v = "string" then
            U4.Case1(unbox<string> v)
        else if (jsTypeField v) = "station" then
            let s = unbox<Client.Station> v
            U4.Case2 { Default.Station with id = s.id }
        else if (jsTypeField v) = "stop" then
            let s = unbox<Client.Stop> v
            U4.Case3 { Default.Stop with id = s.id }
        else if (jsTypeField v) = "location" then
            let l = unbox<Client.Location> v

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
    let makeCaseOfU2StringStation (v: U2<string, Client.Station>) : U2<string, Client.Station> =
        if jsTypeof v = "string" then
            U2.Case1(unbox<string> v)
        else if (jsTypeField v) = "station" then
            let s = unbox<Client.Station> v
            U2.Case2 { Default.Station with id = s.id }
        else
            raise (System.ArgumentException("string|Station expected"))

    // get cases at runtime
    let makeCaseOfU2StringStop (v: U2<string, Client.Stop>) : U2<string, Client.Stop> =
        if jsTypeof v = "string" then
            U2.Case1(unbox<string> v)
        else if (jsTypeField v) = "stop" then
            let s = unbox<Client.Stop> v
            U2.Case2 { Default.Stop with id = s.id }
        else
            raise (System.ArgumentException("string|Stop expected"))

    let validateString (v: string) =
        if jsTypeof v <> "string"
           || (unbox<string> v).Length = 0 then
            raise (System.ArgumentException("string expected"))

    // make Location at runtime
    let makeLocation (v: Client.Location) : Client.Location =
        if (jsTypeField v) = "location" then
            let l = unbox<Client.Location> v

            { Default.Location with
                  id = l.id
                  latitude = l.latitude
                  longitude = l.longitude
                  address = l.address }
        else
            raise (System.ArgumentException("Location expected"))

    let validateBoundingBox (v: Client.BoundingBox) =
        if jsTypeof v.north <> "number"
           || jsTypeof v.west <> "number"
           || jsTypeof v.south <> "number"
           || jsTypeof v.east <> "number" then
            raise (System.ArgumentException("BoundingBox expected"))

    interface FsHafas.Client.HafasClient with

        member __.journeys
            (from: U4<string, Client.Station, Client.Stop, Client.Location>)
            (``to``: U4<string, Client.Station, Client.Stop, Client.Location>)
            (opt: Client.JourneysOptions option)
            =
            client.AsyncJourneys (makeCaseOfU4 from) (makeCaseOfU4 ``to``) opt
            |> Async.StartAsPromise

        member __.refreshJourney
            (refreshToken: string)
            (opt: Client.RefreshJourneyOptions option)
            : JS.Promise<Client.Journey> =
            validateString refreshToken

            client.AsyncRefreshJourney refreshToken opt
            |> Async.StartAsPromise

        member __.trip (id: string) (name: string) (opt: Client.TripOptions option) : JS.Promise<Client.Trip> =
            validateString id

            client.AsyncTrip id name opt
            |> Async.StartAsPromise

        member __.departures
            (id: U2<string, Client.Station>)
            (opt: Client.DeparturesArrivalsOptions option)
            : JS.Promise<array<Client.Alternative>> =
            client.AsyncDepartures (makeCaseOfU2StringStation id) opt
            |> Async.StartAsPromise

        member __.arrivals
            (id: U2<string, Client.Station>)
            (opt: Client.DeparturesArrivalsOptions option)
            : JS.Promise<array<Client.Alternative>> =
            client.AsyncArrivals (makeCaseOfU2StringStation id) opt
            |> Async.StartAsPromise

        member __.locations
            (name: string)
            (opt: Client.LocationsOptions option)
            : JS.Promise<array<U3<Client.Station, Client.Stop, Client.Location>>> =
            validateString name

            client.AsyncLocations name opt
            |> Async.StartAsPromise

        member __.stop
            (stop: U2<string, Stop>)
            (opt: Client.StopOptions option)
            : JS.Promise<U3<Client.Station, Client.Stop, Client.Location>> =
            client.AsyncStop (makeCaseOfU2StringStop stop) opt
            |> Async.StartAsPromise

        member __.nearby
            (location: Client.Location)
            (opt: Client.NearByOptions option)
            : JS.Promise<array<U3<Client.Station, Client.Stop, Client.Location>>> =
            client.AsyncNearby (makeLocation location) opt
            |> Async.StartAsPromise

        member __.reachableFrom
            (location: Client.Location)
            (opt: Client.ReachableFromOptions option)
            : JS.Promise<array<Client.Duration>> =
            client.AsyncReachableFrom (makeLocation location) opt
            |> Async.StartAsPromise

        member __.radar (x: Client.BoundingBox) (opt: Client.RadarOptions option) : JS.Promise<array<Client.Movement>> =
            validateBoundingBox x
            client.AsyncRadar x opt |> Async.StartAsPromise

        member __.tripsByName (name: string) (opt: Client.TripsByNameOptions option) : JS.Promise<array<Client.Trip>> =
            validateString name

            client.AsyncTripsByName name opt
            |> Async.StartAsPromise

        member __.remarks(opt: Client.RemarksOptions option) : JS.Promise<array<Client.Warning>> =
            client.AsyncRemarks opt |> Async.StartAsPromise

        member __.lines (query: string) (opt: Client.LinesOptions option) : JS.Promise<array<Client.Line>> =
            validateString query

            client.AsyncLines query opt
            |> Async.StartAsPromise

        member __.serverInfo(opt: Client.ServerOptions option) : JS.Promise<Client.ServerInfo> =
            client.AsyncServerInfo opt |> Async.StartAsPromise

#endif
