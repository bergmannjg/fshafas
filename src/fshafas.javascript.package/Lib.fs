module App

open Fable.Core
open Fable.Core.JsInterop

open FsHafas.Client
open FsHafas.Api

let createClient (profile: FsHafas.Client.Profile) = HafasClient(profile)

let setDebug () = Log.Debug <- true

let printLocations = FsHafas.Printf.Short.Locations
let printJourneys = FsHafas.Printf.Short.Journeys

let defaultLocationsOptions = Default.LocationsOptions
let defaultJourneysOptions = Default.JourneysOptions

let bestprices
    (profile: FsHafas.Client.Profile)
    (from: string)
    (``to``: string)
    (opt: JourneysOptions option)
    =
    let client = new FsHafas.Api.HafasAsyncClient(profile :?> FsHafas.Endpoint.Profile)
    client.AsyncBestPrices (U4.Case1 from) (U4.Case1 ``to``) opt
    |> Async.StartAsPromise
