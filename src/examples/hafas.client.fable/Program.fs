module App

open Fable.Core
open HafasClient
open HafasClient.Api
open HafasClient.Defaults

let usage () =
    printfn
        """
USAGE:
    --locations <profile> <name>                    get locations, e.g. Hannover (8000152).
    --journeys <profile> <from> <to>                get journeys, e.g. 8000152 8011160.
    --departures <profile> <name>                   get Departures, e.g. 8000152.
    --arrivals <profile> <name>                     get Arrivals, e.g. 8000152.
    --reachablefrom <profile> <lon> <lat>           get ReachableFrom, e.g. 13.078028 54.308438.
    --radar <profile> <north> <west> <south> <east> get Radar, e.g. 52.039421 8.522777 52.019421 8.542777.
"""

let toProfileId (p: string) : ProfileId =
    match p.ToLower() with
    | "bvg" -> Bvg
    | "oebb" -> Oebb
    | "rejseplanen" -> Rejseplanen
    | "irishrail" -> Irishrail
    | _ -> raise (System.Exception("profile expected: bvg|db|irishrail|oebb|rejseplanen"))

[<EntryPoint>]
let main argv =
    try
        if argv.Length = 0 then
            promise { usage () }
        else if argv.[0] = "--locations" && argv.Length > 2 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))
                let! result = client.locations argv.[2] (Some LocationsOptions)
                printfn "%s" (Printf.Locations result)
            }
        else if argv.[0] = "--journeys" && argv.Length > 3 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))
                let! result = client.journeys (U4.Case1 argv.[2]) (U4.Case1 argv.[3]) (Some JourneysOptions)
                printfn "%s" (Printf.Journeys result)
            }
        else if argv.[0] = "--departures" && argv.Length > 2 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))
                let! result = client.departures (U4.Case1 argv.[2]) (Some DeparturesArrivalsOptions)
                printfn "%s" (Printf.Alternatives result.departures)
            }
        else if argv.[0] = "--arrivals" && argv.Length > 2 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))
                let! result = client.arrivals (U4.Case1 argv.[2]) (Some DeparturesArrivalsOptions)
                printfn "%s" (Printf.Alternatives result.arrivals)
            }
        else if argv.[0] = "--reachableFrom" && argv.Length > 3 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))

                let location =
                    { Location with
                        longitude = Some(float argv.[2])
                        latitude = Some(float argv.[3]) }

                let! result = client.reachableFrom location (Some ReachableFromOptions)
                printfn "%s" (Printf.Durations result.reachable)
            }
        else if argv.[0] = "--radar" && argv.Length > 5 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))

                let box: BoundingBox =
                    { north = float argv.[2]
                      west = float argv.[3]
                      south = float argv.[4]
                      east = float argv.[5] }

                let! result = client.radar box (Some RadarOptions)
                printfn "%s" (Printf.Movements result.movements.Value)
            }
        else
            promise { usage () }
        |> Promise.catch (fun error -> printfn "%A" error)
        |> Promise.start
        |> ignore
    with e ->
        printfn "error: %s %s" e.Message e.StackTrace

    0
