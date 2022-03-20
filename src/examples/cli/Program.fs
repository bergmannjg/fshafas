module App

open System.Text.RegularExpressions

#if FABLE_COMPILER
open Fable.Core
open Fable.Core.JsInterop
#endif

open FsHafas
open FsHafas.Client

open Arguments

type CliArguments =
    | Locations of name: string
    | Stop of id: string
    | Journeys of from: string * ``to``: string
    | JourneysFromTrip of fromId: string * toId: string * newToId: string
    | Departures of name: string
    | Trips of name: string
    | Nearby of lon: float * lat: float
    | ReachableFrom of lon: float * lat: float
    | Radar of north: float * west: float * south: float * east: float
    | Lines of name: string
    | ServerInfo
    | Profile of FsHafas.Endpoint.Profile
    | Debug
    | Help

let printHelp () =
    """
USAGE: cli.exe [--help] [--locations <name>] [--stop <id>] [--journeys <from> <to>]
               [--journeysfromtrip <fromId> <toId> <newToId>]
               [--departures <name>] [--trips <name>] [--nearby <lon> <lat>] [--reachablefrom <lon> <lat>]
               [--radar <north> <west> <south> <east>] [--lines <name>] [--serverinfo] [--profile <db|bvg|svv>] [--debug]

OPTIONS:

    --locations <name>    get locations, e.g. Hannover.
    --stop <id>           get stop, e.g. 8000152.
    --journeys <from> <to>
                          get journeys, e.g. Hannover Berlin.
    --journeysfromtrip <fromId> <toId> <newToId>
                          get journeys from current position of trip <fromId> - <toId> to new target <newToId>,
                          e.g. from trip 8002549 to 8000261 to new target 8000207.
    --departures <name>   get Departures, e.g. Hannover.
    --trips <name>        get Trips, e.g. ICE 1001.
    --nearby <lon> <lat>  get Nearby, e.g. 13.078028 54.308438.
    --reachablefrom <lon> <lat>
                          get ReachableFrom, e.g. 13.078028 54.308438.
    --radar <north> <west> <south> <east>
                          get Radar, e.g. 52.039421 8.522777 52.019421 8.542777.
    --lines <name>        get Lines, e.g. S1, profile svv.
    --serverinfo          get ServerInfo.
    --profile <db|bvg|svv>
                          set Profile.
    --debug               show debug msgs.
    --help                display this list of options.
"""

let toProfile s =
    match s with
    | "Db" -> FsHafas.Profiles.Db.profile
    | "Bvg" -> FsHafas.Profiles.Bvg.profile
    | "Svv" -> FsHafas.Profiles.Svv.profile
    | x -> failwithf "%s is out of range" x

let parse (args: string list) =
    [ valueToArg "--profile" (toProfile >> Profile) args
      flagToArg "--debug" Debug args
      valueToArg "--locations" Locations args
      valueToArg "--stop" Stop args
      value2ToArg "--journeys" Journeys args
      value3ToArg "--journeysfromtrip" JourneysFromTrip args
      valueToArg "--departures" Departures args
      valueToArg "--trips" Trips args
      value2ToArg "--nearby" (fun (lon, lat) -> Nearby(float lon, float lat)) args
      value2ToArg "--reachablefrom" (fun (lon, lat) -> ReachableFrom(float lon, float lat)) args
      value4ToArg "--radar" (fun (n, w, s, e) -> Radar(float n, float w, float s, float e)) args
      valueToArg "--lines" Lines args
      flagToArg "--serverinfo" ServerInfo args
      flagToArg "--help" Help args ]
    |> List.choose id

let maybeArray (choose: ('a -> array<'b>)) option =
    match option with
    | Some v -> choose v
    | None -> [||]

let mutable profile = FsHafas.Profiles.Db.profile

printfn "%s" (profile :> FsHafas.Client.Profile).locale

let products () =
    let products =
        Api.HafasAsyncClient.productsOfMode profile Client.ProductTypeMode.Train

    (Api.HafasAsyncClient.productsOfMode profile Client.ProductTypeMode.Bus)
        .Keys
    |> Array.iter (fun p -> products.[p] <- true)

    products |> Some

let trains () =
    let products =
        Api.HafasAsyncClient.productsOfMode profile Client.ProductTypeMode.Train

    products |> Some

let getLocation (client: Api.HafasAsyncClient) (name: string) =
    async {
        if Regex.IsMatch(name, @"^\d+$") then
            return Some(U4.Case1 name)
        else
            let! locations = client.AsyncLocations name (Some Default.LocationsOptions)

            let toU4 (u3: U3<Station, Stop, Location>) =
                match u3 with
                | U3.Case1 station -> U4.Case2 station
                | U3.Case2 stop -> U4.Case3 stop
                | U3.Case3 location -> U4.Case4 location

            return (locations |> Array.tryPick (toU4 >> Some))
    }

let AsyncRunCatched (computation: Async<unit>) =
    async {
        match! computation |> Async.Catch with
        | Choice1Of2 r -> return r
        | Choice2Of2 ext ->
            match ext with
            | :? (HafasError) as ex -> printfn "hafas error: %s %s" ex.code ex.Message
            | e -> printfn "error: %s" e.Message

            return ()
    }

#if FABLE_JS
let AsyncRun (computation: Async<unit>) =
    computation
    |> Async.StartAsPromise
    |> Promise.catch (fun error ->
        if error?isHafasError then
            printfn "hafas error, code: %s msg: %s" error?code error.Message
        else
            printfn "error: %s" error.Message)
    |> ignore
#else
#if FABLE_PY
let AsyncRun (computation: Async<unit>) =
    computation
    |> AsyncRunCatched
    |> Async.StartImmediate
#else
let AsyncRun (computation: Async<unit>) =
    computation
    |> AsyncRunCatched
    |> Async.RunSynchronously
#endif
#endif

let locations (name: string) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! locations = client.AsyncLocations name (Some Default.LocationsOptions)

        Printf.Short.Locations locations |> printfn "%s"
    }
    |> AsyncRun

let journeys (from: string, ``to``: string) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! fromLoc = getLocation client from
        let! toLoc = getLocation client ``to``

        match fromLoc, toLoc with
        | Some fromLoc, Some toLoc ->
            let options =
                { Default.JourneysOptions with
                    results = Some 1
                    products = (products ())
                    stopovers = None
                    polylines = Some true
                    scheduledDays = Some false }

            let! journeys = client.AsyncJourneys fromLoc toLoc (Some options)

            Printf.Short.Journeys journeys |> printfn "%s"
        | _ -> ()
    }
    |> AsyncRun

let journeysFromTrip (fromId: string, toId: string, newToId: string) =
    use client = new Api.HafasAsyncClient(FsHafas.Profiles.Db.profile)

    let departure =
#if FABLE_PY
        FsHafas.Extensions.DateTimeEx.addHours (System.DateTime.Now, -3)
#else
        System.DateTime.Now.AddHours(-3.0)
#endif

    printfn "departure: %s" (departure.ToString("HHmm"))

    let options =
        { Default.JourneysOptions with
            results = Some 1
            departure = Some departure
            stopovers = Some true
            transfers = Some 0 }

    async {
        let! journeysResult = client.AsyncJourneys (U4.Case1 fromId) (U4.Case1 toId) (Some options)

        let journey =
            journeysResult.journeys
            |> maybeArray id
            |> Array.tryFind (fun j -> not (j.legs.[0].cancelled |> Option.contains true))

        journey
        |> Option.iter (FsHafas.Printf.Short.JourneyLegs 0 >> printfn "%s")

        let hasProduct (product: string) (l: Leg) =
            match l.line with
            | Some line -> line.product |> Option.contains product
            | None -> false

        let (tripId, stopovers) =
            journey
            |> maybeArray (fun x -> x.legs)
            |> Array.tryFind (hasProduct "nationalExpress")
            |> Option.fold (fun _ leg -> (leg.tripId, leg.stopovers)) (None, None)

        let hasLeft (s: StopOver) =
            s.departure
            |> Option.exists (fun dep -> Api.Parser.ParseIsoString(dep) < System.DateTime.Now)

        let previousStopover =
            stopovers
            |> maybeArray id
            |> Array.tryFindBack hasLeft

        match tripId, previousStopover with
        | Some tripId, Some previousStopover ->
            printfn "previousStopover:"

            FsHafas.Printf.Short.StopOver 2 previousStopover
            |> printfn "%s"

            let! journeys =
                async {
                    return!
                        client.AsyncJourneysFromTrip
                            tripId
                            previousStopover
                            (U4.Case1 newToId)
                            (Some { Default.JourneysFromTripOptions with stopovers = Some true })
                }

            if journeys.Length > 0 then
                FsHafas.Printf.Short.JourneyLegs 0 journeys.[0]
                |> printfn "%s"

                printfn
                    "journey to %s found, switching at previousStopover: %s"
                    newToId
                    (FsHafas.Printf.Short.StopOverStop 0 previousStopover)
        | _, None -> printfn "previousStopover not found in %d stopovers" ((maybeArray id stopovers).Length)
        | _ -> printfn "journey to %s not found" newToId
    }
    |> AsyncRun

let refreshJourney (refreshToken: string) =
    printfn "refreshJourney: %s" refreshToken

    use client = new Api.HafasAsyncClient(profile)

    async {
        let! journey = client.AsyncRefreshJourney refreshToken None

        FsHafas.Printf.Short.Journey 0 journey
        |> printfn "%s"
    }
    |> AsyncRun

let departures (name: string) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! loc = getLocation client name

        match loc with
        | Some loc ->
            let! departures = client.AsyncDepartures loc None

            FsHafas.Printf.Short.Alternatives departures
            |> printfn "%s"
        | _ -> ()
    }
    |> AsyncRun

#if !FABLE_PY
let dateOfCurrentHour () =
    let dt = System.DateTime.Now
    System.DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0)
#endif

let trips (name: string) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! trips = client.AsyncTripsByName name None

        FsHafas.Printf.Short.Trips trips |> printfn "%s"
    }
    |> AsyncRun

let nearby (lon: float, lat: float) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! locations =
            client.AsyncNearby
                { Default.Location with
                    latitude = Some lat
                    longitude = Some lon }
                (Some
                    { Default.NearByOptions with
                        results = Some 10
                        distance = Some 5000
                        products = (products ()) })

        FsHafas.Printf.Short.Locations locations
        |> printfn "%s"
    }
    |> AsyncRun

let reachableFrom (lon: float, lat: float) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! durations =
            client.AsyncReachableFrom
                { Default.Location with
                    address = Some "unused"
                    latitude = Some lat
                    longitude = Some lon }
                (Some
                    { Default.ReachableFromOptions with
                        maxTransfers = Some 0
                        maxDuration = Some 10 })

        FsHafas.Printf.Short.Durations durations
        |> printfn "%s"
    }
    |> AsyncRun

let radar (n: float, w: float, s: float, e: float) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! movements =
            client.AsyncRadar
                { north = n
                  west = w
                  south = s
                  east = e }
                (Some
                    { Default.RadarOptions with
                        results = Some 60
                        duration = Some 2400
                        frames = Some 10
                        products = (trains ()) })

        FsHafas.Printf.Short.Movements movements
        |> printfn "%s"
    }
    |> AsyncRun

let stop (name: string) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! stop = client.AsyncStop (U2.Case1 name) (Some { Default.StopOptions with linesOfStops = Some true })

        FsHafas.Printf.Short.U3StationStopLocation 0 stop
        |> printfn "%s"
    }
    |> AsyncRun

let remarks () =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! warnings = client.AsyncRemarks None

        FsHafas.Printf.Short.Warnings warnings
        |> printfn "%s"
    }
    |> AsyncRun

let lines (name: string) =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! lines = client.AsyncLines name None

        FsHafas.Printf.Short.Lines lines |> printfn "%s"
    }
    |> AsyncRun

let serverInfo () =
    use client = new Api.HafasAsyncClient(profile)

    async {
        let! serverInfo = client.AsyncServerInfo None

        printfn
            "timetableStart: %A, timetableEnd: %A, serverTime: %A"
            serverInfo.timetableStart
            serverInfo.timetableEnd
            serverInfo.serverTime
    }
    |> AsyncRun

let run (arg: CliArguments) =
    match arg with
    | Debug -> Log.Debug <- true
    | Profile p -> profile <- p
    | Locations v -> locations v
    | Journeys (f, t) -> journeys (f, t)
    | Stop v -> stop v
    | JourneysFromTrip (f, t, n) -> journeysFromTrip (f, t, n)
    | Departures v -> departures v
    | Trips v -> trips v
    | Nearby (lon, lat) -> nearby (lon, lat)
    | ReachableFrom (lon, lat) -> reachableFrom (lon, lat)
    | Radar (n, w, s, e) -> radar (n, w, s, e)
    | Lines v -> lines v
    | ServerInfo -> serverInfo ()
    | Help -> printfn "%s" (printHelp ())

[<EntryPoint>]
let main argv =
    try
#if !FABLE_PY
        Api.HafasAsyncClient.initSerializer ()
#endif
        argv |> Array.toList |> parse |> List.iter run
    with
    | e -> printfn "main error: %s" e.Message

    0
