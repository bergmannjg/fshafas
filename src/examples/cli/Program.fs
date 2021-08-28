module App

open System.Text.RegularExpressions

open Argu

open FsHafas
open FsHafas.Client

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
    | [<Unique>] ServerInfo
    | [<Unique>] Profile of ProfileId
    | [<Unique>] Debug

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Locations _ -> "get locations, ex. Hannover."
            | Stop _ -> "get stop, ex. 8000152."
            | Journeys _ -> "get journeys, ex. Hannover Berlin."
            | JourneysFromTrip _ -> "get journeysfromtrip, ex. 8002549 8000261 8000207."
            | Departures _ -> "get departures, ex. Hannover."
            | Trips _ -> "get trips, ex. ICE 1001."
            | Nearby _ -> "get nearby, ex. 13.078028 54.308438."
            | ReachableFrom _ -> "get reachablefrom, ex. 13.078028 54.308438."
            | Radar _ -> "get radar, ex. 52.039421 8.522777 52.019421 8.542777."
            | Lines _ -> "get lines, ex. S1 --profile svv."
            | ServerInfo _ -> "get serverinfo."
            | Profile _ -> "set profile."
            | Debug _ -> "show debug msgs."

let private maybeArray (choose: ('a -> array<'b>)) option =
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

let locations (name: string) =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! locations = client.AsyncLocations name (Some Default.LocationsOptions)

        Printf.Short.Locations locations |> printfn "%s"
    }
    |> Async.RunSynchronously

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
    |> Async.RunSynchronously

let journeysFromTrip (fromId: string, toId: string, newToId: string) =
    use client = new Api.HafasAsyncClient(ProfileId.Db)

    let departure = System.DateTime.Now.AddHours(-4.0)

    let options =
        { Default.JourneysOptions with
              results = Some 1
              departure = Some departure
              stopovers = Some true
              transfers = Some 0 }

    let journeysResult =
        async { return! client.AsyncJourneys(U4.Case1 fromId) (U4.Case1 toId) (Some options) }
        |> Async.RunSynchronously

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

        let journeys =
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
            |> Async.RunSynchronously

        if journeys.Length > 0 then
            FsHafas.Printf.Short.JourneyLegs 0 journeys.[0]
            |> printfn "%s"
    | _ -> ()

let refreshJourney (refreshToken: string) =
    printfn "refreshJourney: %s" refreshToken

    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! journey = client.AsyncRefreshJourney refreshToken None

        FsHafas.Printf.Short.Journey 0 journey
        |> printfn "%s"
    }
    |> Async.RunSynchronously

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
    |> Async.RunSynchronously

let trips (name: string) =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! trips = client.AsyncTripsByName name None

        FsHafas.Printf.Short.Trips trips |> printfn "%s"
    }
    |> Async.RunSynchronously

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
    |> Async.RunSynchronously

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
    |> Async.RunSynchronously

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
    |> Async.RunSynchronously

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
    |> Async.RunSynchronously

let remarks () =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! warnings = client.AsyncRemarks None

        FsHafas.Printf.Short.Warnings warnings
        |> printfn "%s"
    }
    |> Async.RunSynchronously

let lines (name: string) =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! lines = client.AsyncLines name None

        FsHafas.Printf.Short.Lines lines |> printfn "%s"
    }
    |> Async.RunSynchronously

let serverInfo () =
    use client = new Api.HafasAsyncClient(profileId)

    async {
        let! serverInfo = client.AsyncServerInfo None

        printfn "%A" serverInfo
    }
    |> Async.RunSynchronously

[<EntryPoint>]
let main argv =
    try
        Api.HafasAsyncClient.initSerializer ()

        let parser =
            ArgumentParser.Create<CliArguments>(programName = "cli.exe")

        let options = parser.Parse(argv)

        Log.Debug <- options.Contains Debug

        profileId <- options.GetResult(Profile, defaultValue = ProfileId.Db)

        options.GetResults Locations
        |> List.iter locations

        options.GetResults Stop |> List.iter stop

        options.GetResults Journeys |> List.iter journeys

        options.GetResults JourneysFromTrip
        |> List.iter journeysFromTrip

        options.GetResults Departures
        |> List.iter departures

        options.GetResults Trips |> List.iter trips

        options.GetResults Nearby |> List.iter nearby

        options.GetResults ReachableFrom
        |> List.iter reachableFrom

        options.GetResults Radar |> List.iter radar

        options.GetResults Lines |> List.iter lines

        options.GetResults ServerInfo
        |> List.iter (fun _ -> serverInfo ())

    with
    | :? ArguException as e -> printfn "%s" e.Message
    | e -> fprintfn stderr "error: %s" e.Message

    0
