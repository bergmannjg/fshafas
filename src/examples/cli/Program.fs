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
    | JourneysWithOptions of from: string * ``to``: string * options: string
    | JourneysFromTrip of tripId: string * stopover: string * departure: string * newToId: string
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
    --journeys <from> <to> <options>
                          get journeys, e.g. Hannover "Berlin-Spandau" "ProductId:national;Transfers:0".
    --journeysfromtrip <tripId> <prevStopId> <prevStopDepature> <newToId>
                          get journeys from  <prevStopId> of trip <tripId> to new target <newToId>.
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
    | "db" -> FsHafas.Profiles.Db.profile
    | "bvg" -> FsHafas.Profiles.Bvg.profile
    | "svv" -> FsHafas.Profiles.Svv.profile
    | x -> failwithf "%s is out of range" x

let parse (args: string list) =
    [ valueToArg "--profile" (toProfile >> Profile) args
      flagToArg "--debug" Debug args
      valueToArg "--locations" Locations args
      valueToArg "--stop" Stop args
      (value3ToArg "--journeys" JourneysWithOptions args
       |> Option.orElseWith (fun () -> value2ToArg "--journeys" Journeys args))
      value4ToArg "--journeysfromtrip" JourneysFromTrip args
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

let productsOfId (id: string) =
    let products = Products(false)

    (Api.HafasAsyncClient.productsOfFilter profile (fun p -> p.id = id))
        .Keys
    |> Array.iter (fun p -> products.[p] <- true)

    products |> Some

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

type JourneyOption =
    | Id of id: string
    | Transfers of nr: int
    | Bahncard of discount: int * ``class``: int
    | Age of age: int

let parseJourneyOptions (args: string list) =
    [ valueToArg "ProductId" JourneyOption.Id args
      valueToArg "Transfers" (fun (nr) -> JourneyOption.Transfers(int nr)) args
      value2ToArg "Bahncard" (fun (nr, cls) -> JourneyOption.Bahncard(int nr, int cls)) args
      valueToArg "Age" (fun (age) -> JourneyOption.Age(int age)) args ]
    |> List.choose id

let applyJourneyOption (option: JourneyOption) (journeysOptions: JourneysOptions) =
    match option with
    | Id id -> { journeysOptions with products = productsOfId id }
    | Transfers nr -> { journeysOptions with transfers = Some nr }
    | Bahncard (discount, cls) ->
        { journeysOptions with
            loyaltyCard =
                Some(
                    { ``type`` = "Bahncard"
                      discount = Some discount
                      ``class`` = Some cls }
                ) }
    | Age age -> { journeysOptions with age = Some age }

let applyJourneyOptions (options: JourneyOption list) =
    options
    |> List.fold (fun s o -> applyJourneyOption o s) Default.JourneysOptions

let getJourneyOptions (options: string) =
    let l = options.Split([| ':'; ';' |])

    if l.Length > 0 then
        l
        |> Array.toList
        |> parseJourneyOptions
        |> applyJourneyOptions
    else
        Default.JourneysOptions

let journeys (from: string, ``to``: string, someOptions: string option) =
    use client = new Api.HafasAsyncClient(profile)

    let options =
        match someOptions with
        | Some options -> getJourneyOptions options
        | None ->
            { Default.JourneysOptions with
                results = Some 1
                products = (products ())
                stopovers = None
                polylines = Some true
                scheduledDays = Some false }

    async {
        let! fromLoc = getLocation client from
        let! toLoc = getLocation client ``to``

        match fromLoc, toLoc with
        | Some fromLoc, Some toLoc ->
            let! journeys = client.AsyncJourneys fromLoc toLoc (Some options)

            Printf.Short.Journeys journeys |> printfn "%s"
        | _ -> ()
    }
    |> AsyncRun

let journeysFromTrip (tripId: string, stopover: string, departure: string, newTo: string) =
    use client = new Api.HafasAsyncClient(FsHafas.Profiles.Db.profile)

    async {
        let! journeys =
            async {
                let! stopoverLoc = getLocation client stopover
                let! newToLoc = getLocation client newTo

                let stopoverId =
                    match stopoverLoc with
                    | Some (U4.Case1 id) -> Some id
                    | Some (U4.Case3 stop) -> stop.id
                    | _ -> None

                match stopoverId, newToLoc with
                | Some stopoverId, Some newToLoc ->
                    let stop: Stop = { Default.Stop with id = Some stopoverId }

                    let previousStopover: StopOver =
                        { Default.StopOver with
                            stop = Some(U2.Case2 stop)
                            departure = Some departure }

                    return!
                        client.AsyncJourneysFromTrip
                            tripId
                            previousStopover
                            newToLoc
                            (Some { Default.JourneysFromTripOptions with stopovers = Some true })
                | _ -> return [||]
            }

        Printf.Short.Journeys
            { earlierRef = None
              laterRef = None
              journeys = Some journeys
              realtimeDataFrom = None }
        |> printfn "%s"
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

#if FABLE_PY

module internal DateTimeEx =
    open Fable.Core

    // workaround: missing code year
    [<Emit("$0.year")>]
    let year (dt: obj) : int = jsNative

    // workaround: missing code month
    [<Emit("$0.month")>]
    let month (dt: obj) : int = jsNative

    // workaround: missing code day
    [<Emit("$0.day")>]
    let day (dt: obj) : int = jsNative

    // workaround: missing code hour
    [<Emit("$0.hour")>]
    let hour (dt: obj) : int = jsNative

    // workaround: missing code minute
    [<Emit("$0.minute")>]
    let minute (dt: obj) : int = jsNative

    // workaround: missing code second
    [<Emit("$0.second")>]
    let second (dt: obj) : int = jsNative

let dateOfCurrentHour () =
    let dt = System.DateTime.Now
    System.DateTime(DateTimeEx.year (dt), DateTimeEx.month (dt), DateTimeEx.day (dt), DateTimeEx.hour (dt), 0, 0)

#else

let dateOfCurrentHour () =
    let dt = System.DateTime.Now
    System.DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0)

#endif

let departures (name: string) =
    use client = new Api.HafasAsyncClient(profile)

    let options =
        { Default.DeparturesArrivalsOptions with ``when`` = Some(dateOfCurrentHour ()) }

    async {
        let! loc = getLocation client name

        match loc with
        | Some loc ->
            let! departures = client.AsyncDepartures loc (Some options)

            FsHafas.Printf.Short.Alternatives departures
            |> printfn "%s"
        | _ -> ()
    }
    |> AsyncRun

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
            "hciVersion: %A, timetableStart: %A, timetableEnd: %A, serverTime: %A"
            serverInfo.hciVersion
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
    | Journeys (f, t) -> journeys (f, t, None)
    | JourneysWithOptions (f, t, o) -> journeys (f, t, Some o)
    | Stop v -> stop v
    | JourneysFromTrip (f, t, d, n) -> journeysFromTrip (f, t, d, n)
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
