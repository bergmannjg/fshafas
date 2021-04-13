// examples using HafasAsyncClient

// use arg '--fsi-server-input-codepage:28591' for unicode characters
#r "nuget:FSharp.SystemTextJson"
#r "nuget:Polyliner.Net"
#r "nuget:FSlugify"
#r "nuget:NodaTime"
#r "../src/fshafas/bin/Debug/net5.0/fshafas.dll"

open FsHafas
open FsHafas.Client

Api.HafasAsyncClient.initSerializer ()

Log.Debug <-
    fsi.CommandLineArgs
    |> Array.exists (fun arg -> arg = "--debug")

let id =
    match fsi.CommandLineArgs
          |> Array.tryFind (fun (arg: string) -> arg.StartsWith("--ProfileId."))
          |> Option.map (fun s -> s.Replace("--ProfileId.", "")) with
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

let locations (name: string) =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! locations = client.AsyncLocations name (Some Default.LocationsOptions)

        Printf.Short.Locations locations |> printfn "%s"
    }
    |> Async.RunSynchronously

let journeys (fromId: string) (toId: string) =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! journeys =
            client.AsyncJourneys
                (U4.Case1 fromId)
                (U4.Case1 toId)
                (Some
                    { Default.JourneysOptions with
                          results = Some 1
                          products = Some products
                          stopovers = None
                          polylines = Some true })

        Printf.Short.Journeys journeys |> printfn "%s"
    }
    |> Async.RunSynchronously

let refreshJourney (refreshToken: string) =
    printfn "refreshJourney: %s" refreshToken

    use client = new Api.HafasAsyncClient(id)

    async {
        let! journey = client.AsyncRefreshJourney refreshToken None

        FsHafas.Printf.Short.Journey 0 journey
        |> printfn "%s"
    }
    |> Async.RunSynchronously

let departures (name: string) =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! departures = client.AsyncDepartures(U2.Case1 name) None

        FsHafas.Printf.Short.Alternatives departures
        |> printfn "%s"
    }
    |> Async.RunSynchronously

let trips (name: string) =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! trips = client.AsyncTripsByName name None

        FsHafas.Printf.Short.Trips trips |> printfn "%s"
    }
    |> Async.RunSynchronously

let nearby () =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! locations =
            client.AsyncNearby
                { Default.Location with
                      latitude = Some 54.308438
                      longitude = Some 13.078028 }
                (Some
                    { Default.NearByOptions with
                          results = Some 10
                          distance = Some 5000
                          products = Some products })

        FsHafas.Printf.Short.Locations locations
        |> printfn "%s"
    }
    |> Async.RunSynchronously

let reachableFrom () =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! durations =
            client.AsyncReachableFrom
                { Default.Location with
                      latitude = Some 54.308438
                      longitude = Some 13.078028 }
                (Some
                    { Default.ReachableFromOptions with
                          maxTransfers = Some 0
                          maxDuration = Some 10 })

        FsHafas.Printf.Short.Durations durations
        |> printfn "%s"
    }
    |> Async.RunSynchronously

let radar () =
    use client = new Api.HafasAsyncClient(id)

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
                          products = Some products })

        FsHafas.Printf.Short.Movements movements
        |> printfn "%s"
    }
    |> Async.RunSynchronously

let stop (name: string) =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! stop = client.AsyncStop(U2.Case1 name) None

        FsHafas.Printf.Short.U3StationStopLocation 0 stop
        |> printfn "%s"
    }
    |> Async.RunSynchronously

let remarks () =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! warnings = client.AsyncRemarks None

        FsHafas.Printf.Short.Warnings warnings
        |> printfn "%s"
    }
    |> Async.RunSynchronously

let lines (name: string) =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! lines = client.AsyncLines name None

        FsHafas.Printf.Short.Lines lines |> printfn "%s"
    }
    |> Async.RunSynchronously

let serverInfo () =
    use client = new Api.HafasAsyncClient(id)

    async {
        let! serverInfo = client.AsyncServerInfo None

        printfn "%A" serverInfo
    }
    |> Async.RunSynchronously

match args with
| [| "locations"; name |] -> locations name
| [| "journeys"; fromId; toId |] -> journeys fromId toId
| [| "journeys" |] -> journeys "8000152" "8011160"
| [| "refreshJourney"; token |] -> refreshJourney token
| [| "departures"; name |] -> departures name
| [| "departures" |] -> departures "8000152"
| [| "trips"; name |] -> trips name
| [| "trips" |] -> trips "ICE 846"
| [| "nearby" |] -> nearby ()
| [| "reachableFrom" |] -> reachableFrom ()
| [| "radar" |] -> radar ()
| [| "stop"; name |] -> stop name
| [| "stop" |] -> stop "8000152"
| [| "remarks" |] -> remarks ()
| [| "lines"; name |] -> lines name
| [| "lines" |] -> lines "S1"
| [| "serverInfo" |] -> serverInfo ()
| _ -> printfn "unkown args %A" args
