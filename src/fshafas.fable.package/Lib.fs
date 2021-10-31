module App

open Fable.Core
open FsHafas.Client
open FsHafas.Api

let private getFsHafasProfile (name: string) : FsHafas.Endpoint.Profile =
    match name with
    | "db" -> FsHafas.Profiles.Db.getProfile (FsHafas.Api.Parser.defaultProfile)
    | "bvg" -> FsHafas.Profiles.Bvg.getProfile (FsHafas.Api.Parser.defaultProfile)
    | _ -> raise (System.ArgumentException("profile unkown: " + name))

let createClient (profile: string) =
    HafasClient(getFsHafasProfile (profile))

let setDebug () = Log.Debug <- true

let getProfile (profile: string) =
    HafasClient.Profile(getFsHafasProfile profile)

let printLocations = FsHafas.Printf.Short.Locations
let printJourneys = FsHafas.Printf.Short.Journeys

let defaultLocationsOptions = Default.LocationsOptions
let defaultJourneysOptions = Default.JourneysOptions
