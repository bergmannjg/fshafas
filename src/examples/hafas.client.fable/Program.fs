module App

open Fable.Core
open HafasClient.Types
open HafasClient.Api
open Printf

let LocationsOptions: LocationsOptions =
    { fuzzy = Some true
      results = Some 5
      stops = Some true
      addresses = Some true
      poi = Some true
      subStops = Some true
      entrances = Some true
      linesOfStops = Some false
      language = Some "de" }

let JourneysOptions: JourneysOptions =
    { departure = Some System.DateTime.Now
      arrival = None
      earlierThan = None
      laterThan = None
      results = Some 3
      via = None
      stopovers = Some true
      transfers = Some -1
      transferTime = Some 0
      accessibility = Some "none"
      bike = Some false
      products = None
      tickets = Some false
      polylines = Some false
      subStops = Some true
      entrances = Some true
      remarks = Some true
      walkingSpeed = Some "normal"
      startWithWalking = Some true
      language = Some "de"
      scheduledDays = Some false
      firstClass = Some false
      age = None
      loyaltyCard = None
      ``when`` = None }

let usage () =
    printfn "usage: --locations <db|bvg> <name>"

[<EntryPoint>]
let main argv =
    try
        if argv.Length = 0 then
            promise { usage () }
        else if argv.[0] = "--locations" && argv.Length > 2 then
            promise {
                let client = createClient (getProfile argv.[1])
                let! result = client.locations argv.[2] (Some LocationsOptions)
                printfn "%s" (Short.Locations result)
            }
        else if argv.[0] = "--journeys" && argv.Length > 3 then
            promise {
                let client = createClient (getProfile argv.[1])
                let! result = client.journeys (U4.Case1 argv.[2]) (U4.Case1 argv.[3]) (Some JourneysOptions)
                printfn "%s" (Short.Journeys result)
            }
        else
            promise { usage () }
        |> Promise.catch (fun error -> printfn "%A" error)
        |> Promise.start
        |> ignore
    with
    | e -> printfn "error: %s %s" e.Message e.StackTrace

    0
