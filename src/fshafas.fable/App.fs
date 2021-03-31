module App

open Fable.Core
open FsHafas

let id = ProfileId.Db

let f1 () =
    async {
        let! locs =
            Interactive.AsyncLocations
                id
                "Bielefeld"
                { Default.LocationsOptions with
                      results = Some 1 }

        Printf.Short.Locations locs |> printfn "%s"
    }
    |> Async.StartImmediate

let f2 () =
    let products =
        Interactive.productsOfMode id Client.ProductTypeMode.Train

    Log.Debug <- false

    async {
        let! journeys =
            Interactive.asyncJourneys
                id
                "Hannover"
                "Bielefeld"
                { Default.JourneysOptions with
                      results = Some 1
                      products = Some products
                      stopovers = None
                      polylines = Some true }

        FsHafas.Printf.Short.Journeys journeys
        |> printfn "%s"
    }
    |> Async.StartImmediate

let f3 () =
    let client =
        HafasClient(FsHafas.Profiles.Db.getProfile ()) :> FsHafas.Client.HafasClient

    client.locations "Bielefeld" (Some Default.LocationsOptions)
    |> Promise.iter (Printf.Short.Locations >> printfn "%s")

let f4 () =
    let client =
        HafasClient(FsHafas.Profiles.Db.getProfile ()) :> FsHafas.Client.HafasClient

    client.departures (U2.Case1 "8000036") (Some Default.DeparturesArrivalsOptions)
    |> Promise.iter (Printf.Short.Alternatives >> printfn "%s")

let f5 () =
    let client =
        HafasClient(FsHafas.Profiles.Db.getProfile ()) :> FsHafas.Client.HafasClient

    client.journeys
        (U4.Case3 { Default.Stop with id = Some "8000036" })
        (U4.Case1 "8096003")
        (Some
            { Default.JourneysOptions with
                  scheduledDays = Some true })
    |> Promise.iter (Printf.Short.Journeys >> printfn "%s")

Log.Debug <- false

f5 ()
