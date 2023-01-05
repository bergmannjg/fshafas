module App

open Fable.Core
open HafasClient
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

let DeparturesArrivalsOptions: DeparturesArrivalsOptions =
    { ``when`` = Some System.DateTime.Now
      direction = None
      line = None
      duration = Some 10
      results = None
      subStops = None
      entrances = None
      linesOfStops = Some false
      remarks = None
      stopovers = Some false
      includeRelatedStations = Some false
      products = None
      language = Some "de" }

let ReachableFromOptions: ReachableFromOptions =
    { ``when`` = Some System.DateTime.Now
      maxTransfers = Some 5
      maxDuration = Some 10
      products = None
      subStops = None
      entrances = None
      polylines = None }

let RadarOptions: RadarOptions =
    { results = Some 256
      frames = Some 3
      products = None
      duration = Some 30
      subStops = Some true
      entrances = Some true
      polylines = Some true
      ``when`` = Some System.DateTime.Now }

let Location: Location =
    { ``type`` = LocationType.Location
      id = None
      name = None
      poi = None
      address = Some "unused"
      longitude = None
      latitude = None
      altitude = None
      distance = None }

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
    | "db" -> Db
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
                printfn "%s" (Short.Locations result)
            }
        else if argv.[0] = "--journeys" && argv.Length > 3 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))
                let! result = client.journeys (U4.Case1 argv.[2]) (U4.Case1 argv.[3]) (Some JourneysOptions)
                printfn "%s" (Short.Journeys result)
            }
        else if argv.[0] = "--departures" && argv.Length > 2 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))
                let! result = client.departures (U4.Case1 argv.[2]) (Some DeparturesArrivalsOptions)
                printfn "%s" (Short.Alternatives result.departures)
            }
        else if argv.[0] = "--arrivals" && argv.Length > 2 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))
                let! result = client.arrivals (U4.Case1 argv.[2]) (Some DeparturesArrivalsOptions)
                printfn "%s" (Short.Alternatives result.arrivals)
            }
        else if argv.[0] = "--reachableFrom" && argv.Length > 3 then
            promise {
                let client = createClient (getProfile (toProfileId argv.[1]))

                let location =
                    { Location with
                        longitude = Some(float argv.[2])
                        latitude = Some(float argv.[3]) }

                let! result = client.reachableFrom location (Some ReachableFromOptions)
                printfn "%s" (Short.Durations result.reachable)
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
                printfn "%s" (Short.Movements result.movements.Value)
            }
        else
            promise { usage () }
        |> Promise.catch (fun error -> printfn "%A" error)
        |> Promise.start
        |> ignore
    with
    | e -> printfn "error: %s %s" e.Message e.StackTrace

    0
