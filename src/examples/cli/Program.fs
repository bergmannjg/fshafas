module App

open System.Text.RegularExpressions

#if FABLE_COMPILER
open Fable.Core
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
    | Profile of ProfileId
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
                          get journeys from trip, e.g. 8002549 8000261 8000207.
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

let toProfileId s =
    match s with
    | "Db" -> ProfileId.Db
    | "Bvg" -> ProfileId.Bvg
    | "Svv" -> ProfileId.Svv
    | x -> failwithf "%s is out of range" x

let parse (args: string list) =
    [ valueToArg "--profile" (toProfileId >> Profile) args
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

let mutable profileId = ProfileId.Db

let products () =
    let products =
        Api.HafasAsyncClient.productsOfMode profileId Client.ProductTypeMode.Train

    (Api.HafasAsyncClient.productsOfMode profileId Client.ProductTypeMode.Bus)
        .Keys
    |> Array.iter (fun p -> products.[p] <- true)

    products |> Some

let getLocation (client: Api.HafasAsyncClient) (name: string) =
    async {
        if Regex(@"^\d{6,}$").IsMatch name then
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

let AsyncRun (computation: Async<'T>) =
    computation
#if FABLE_COMPILER
    |> Async.StartAsPromise
    |> ignore
#else
    |> Async.RunSynchronously
#endif

let locations (name: string) =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! locations = client.AsyncLocations name (Some Default.LocationsOptions)

        Printf.Short.Locations locations |> printfn "%s"
    }
    |> AsyncRun

let journeys (from: string, ``to``: string) =
    use client = new Api.HafasAsyncClient(profileId)

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
                      polylines = Some true }

            let! journeys = client.AsyncJourneys fromLoc toLoc (Some options)

            Printf.Short.Journeys journeys |> printfn "%s"
        | _ -> ()
    }
    |> AsyncRun

let journeysFromTrip (fromId: string, toId: string, newToId: string) =
    use client = new Api.HafasAsyncClient(ProfileId.Db)

    let departure = System.DateTime.Now.AddHours(-4.0)

    let options =
        { Default.JourneysOptions with
              results = Some 1
              departure = Some departure
              stopovers = Some true
              transfers = Some 0 }

    async {
        let! journeysResult = client.AsyncJourneys(U4.Case1 fromId) (U4.Case1 toId) (Some options)

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
            |> Option.exists (fun dep -> Api.Parser.ParseIsoString(dep).DateTime < System.DateTime.Now)

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
                            (Some
                                { Default.JourneysFromTripOptions with
                                      stopovers = Some true })
                }

            if journeys.Length > 0 then
                FsHafas.Printf.Short.JourneyLegs 0 journeys.[0]
                |> printfn "%s"
        | _ -> ()
    }
    |> AsyncRun

let refreshJourney (refreshToken: string) =
    printfn "refreshJourney: %s" refreshToken

    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! journey = client.AsyncRefreshJourney refreshToken None

        FsHafas.Printf.Short.Journey 0 journey
        |> printfn "%s"
    }
    |> AsyncRun

let departures (name: string) =
    use client = new Api.HafasAsyncClient(profileId)

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

let trips (name: string) =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! trips = client.AsyncTripsByName name None

        FsHafas.Printf.Short.Trips trips |> printfn "%s"
    }
    |> AsyncRun

let nearby (lon: float, lat: float) =
    use client = new Api.HafasAsyncClient(profileId)

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
    use client = new Api.HafasAsyncClient(profileId)

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
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! movements =
            client.AsyncRadar
                { north = 52.039421
                  west = 8.522777
                  south = 52.019421
                  east = 8.542777 }
                (Some
                    { Default.RadarOptions with
                          results = Some 60
                          duration = Some 1800
                          frames = Some 100
                          products = (products ()) })

        FsHafas.Printf.Short.Movements movements
        |> printfn "%s"
    }
    |> AsyncRun

let stop (name: string) =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! stop =
            client.AsyncStop
                (U2.Case1 name)
                (Some
                    { Default.StopOptions with
                          linesOfStops = Some true })

        FsHafas.Printf.Short.U3StationStopLocation 0 stop
        |> printfn "%s"
    }
    |> AsyncRun

let remarks () =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! warnings = client.AsyncRemarks None

        FsHafas.Printf.Short.Warnings warnings
        |> printfn "%s"
    }
    |> AsyncRun

let lines (name: string) =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! lines = client.AsyncLines name None

        FsHafas.Printf.Short.Lines lines |> printfn "%s"
    }
    |> AsyncRun

let serverInfo () =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! serverInfo = client.AsyncServerInfo None

        printfn "%A" serverInfo
    }
    |> AsyncRun

let run (arg: CliArguments) =
    match arg with
    | Debug -> Log.Debug <- true
    | Profile p -> profileId <- p
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
        Api.HafasAsyncClient.initSerializer ()

        argv |> Array.toList |> parse |> List.iter run
    with
    | e -> printf "error: %s" e.Message

    0
