/// examples using HafasClient, compiles to JavaScript
module App

open Fable.Core

open FsHafas.Client
open FsHafas.Api

let profile = FsHafas.Profiles.Db.profile

let f1 () =
    let client =
        HafasClient(profile) :> FsHafas.Client.HafasClient

    client.locations
        "Hannover"
        (Some
            { Default.LocationsOptions with
                  results = Some 1 })
    |> Promise.iter (FsHafas.Printf.Short.Locations >> printfn "%s")

let f3 () =
    let client =
        HafasClient(profile) :> FsHafas.Client.HafasClient

    client.locations "Hannover" (Some Default.LocationsOptions)
    |> Promise.iter (FsHafas.Printf.Short.Locations >> printfn "%s")

let f4 () =
    let client =
        HafasClient(profile) :> FsHafas.Client.HafasClient

    client.departures (U4.Case1 "8000036") (Some Default.DeparturesArrivalsOptions)
    |> Promise.iter (FsHafas.Printf.Short.Alternatives >> printfn "%s")

let f5 () =
    let client =
        HafasClient(profile) :> FsHafas.Client.HafasClient

    printfn "%A" profile.products

    client.journeys
        (U4.Case3
            { Default.Stop with
                  id = Some "8000036" })
        (U4.Case1 "8096003")
        (Some
            { Default.JourneysOptions with
                  scheduledDays = Some true })
    |> Promise.iter (FsHafas.Printf.Short.Journeys >> printfn "%s")

Log.Debug <- false

f5 ()
