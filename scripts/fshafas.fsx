// use arg '--fsi-server-input-codepage:28591' for unicode characters
#r "nuget:FSharp.SystemTextJson"
#r "nuget:Polyliner.Net"
#r "nuget:FSlugify"
#r "nuget:NodaTime"
#r "../src/fshafas/bin/Debug/net5.0/fshafas.dll"

open FsHafas
open FsHafas.Default

Serializer.addConverters ([| Serializer.UnionConverter<Client.ProductTypeMode>() |])

Log.Debug <-
    fsi.CommandLineArgs
    |> Array.exists (fun arg -> arg = "--debug")

let id =
    match fsi.CommandLineArgs
          |> Array.tryFind (fun (arg: string) -> arg.StartsWith("--Profile."))
          |> Option.map (fun s -> s.Replace("--Profile.", "")) with
    | Some "Db" -> ProfileId.Db
    | Some "Bvg" -> ProfileId.Bvg
    | Some "Svv" -> ProfileId.Svv
    | _ -> ProfileId.Db

let args =
    fsi.CommandLineArgs
    |> Array.skip 1
    |> Array.filter (fun arg -> not (arg.StartsWith("--")))

let products =
    Interactive.productsOfMode id Client.ProductTypeMode.Train

let locations () =
    let name =
        if id = ProfileId.Bvg then
            "Alexanderplatz"
        else
            "Bielefeld"

    let locations =
        Interactive.locations
            id
            name
            { Default.LocationsOptions with
                  results = Some 3
                  linesOfStops = Some true }

    FsHafas.Printf.Short.Locations locations
    |> printfn "%s"

let journeys () =
    let from =
        if id = ProfileId.Bvg then
            "Alexanderplatz"
        else
            "Bielefeld"

    let ``to`` =
        if id = ProfileId.Bvg then
            "Nollendorfplatz"
        else
            "Berlin"

    let journeys =
        Interactive.journeys
            id
            from
            ``to``
            { Default.JourneysOptions with
                  results = Some 1
                  products = Some products
                  stopovers = None
                  polylines = Some true }

    FsHafas.Printf.Short.Journeys journeys
    |> printfn "%s"

let refreshJourney (refreshToken: string) =
    printfn "refreshJourney:"

    let journey =
        Interactive.refreshJourney id refreshToken Default.RefreshJourneyOptions

    FsHafas.Printf.Short.Journey 0 journey
    |> printfn "%s"

let departures () =
    let departures =
        Interactive.departures
            id
            "Bielefeld"
            { Default.DeparturesArrivalsOptions with
                  products = Some products }

    FsHafas.Printf.Short.Alternatives departures
    |> printfn "%s"

let trips () =
    printfn "tripsByName:"

    let trips =
        Interactive.tripsByName
            id
            "ICE 846"
            { Default.TripsByNameOptions with
                  ``when`` = Some System.DateTime.Now }

    FsHafas.Printf.Short.Trips trips |> printfn "%s"

let nearby () =
    printfn "nearby:"

    let locations =
        Interactive.nearby
            id
            { Default.Location with
                  latitude = Some 54.308438
                  longitude = Some 13.078028 }
            { Default.NearByOptions with
                  results = Some 10
                  distance = Some 5000
                  products = Some products }

    FsHafas.Printf.Short.Locations locations
    |> printfn "%s"

let reachableFrom () =
    printfn "reachableFrom:"

    let durations =
        Interactive.reachableFrom
            id
            { Default.Location with
                  latitude = Some 54.308438
                  longitude = Some 13.078028 }
            { Default.ReachableFromOptions with
                  maxTransfers = Some 0
                  maxDuration = Some 10 }

    FsHafas.Printf.Short.Durations durations
    |> printfn "%s"

let radar () =
    printfn "radar:"

    let movements =
        Interactive.radar
            id
            { north = 52.039421
              west = 8.522777
              south = 52.019421
              east = 8.542777 }
            { Default.RadarOptions with
                  results = Some 60
                  duration = Some 1800
                  frames = Some 100
                  products = Some products }

    FsHafas.Printf.Short.Movements movements
    |> printfn "%s"

let stop () =
    printfn "stop:"

    let stop =
        Interactive.stop id "8010338" Default.StopOptions

    FsHafas.Printf.Short.U3StationStopLocation 0 stop
    |> printfn "%s"

let remarks () =
    printfn "remarks:"

    let warnings = Interactive.remarks id

    FsHafas.Printf.Short.Warnings warnings
    |> printfn "%s"

let lines () =
    printfn "lines:"

    let lines = Interactive.lines id "S1"

    FsHafas.Printf.Short.Lines lines |> printfn "%s"

let serverInfo () =
    printfn "serverInfo:"

    let serverInfo = Interactive.serverInfo id

    printfn "%A" serverInfo

match args with
| [| "locations" |] -> locations ()
| [| "journeys" |] -> journeys ()
| [| "refreshJourney"; token |] -> refreshJourney (token)
| [| "departures" |] -> departures ()
| [| "trips" |] -> trips ()
| [| "nearby" |] -> nearby ()
| [| "reachableFrom" |] -> reachableFrom ()
| [| "radar" |] -> radar ()
| [| "stop" |] -> stop ()
| [| "remarks" |] -> remarks ()
| [| "lines" |] -> lines ()
| [| "serverInfo" |] -> serverInfo ()
| _ -> printfn "unkown args %A" args
