module App

open Fable.Core
open FsHafas.Client
open FsHafas.Api

let private getFsHafasProfileId (name: string) : FsHafas.Client.ProfileId =
    match name with
    | "db" -> FsHafas.Client.ProfileId.Db
    | "bvg" -> FsHafas.Client.ProfileId.Bvg
    | _ -> raise (System.ArgumentException("profile unkown: " + name))

let createClient (profile: string) =
    new HafasClient(getFsHafasProfileId (profile))

let setDebug () = Log.Debug <- true

let getProfile (profile: string) =
    HafasClient.Profile(getFsHafasProfileId profile)

let printLocations = FsHafas.Printf.Short.Locations
let printJourneys = FsHafas.Printf.Short.Journeys

let defaultLocationsOptions = Default.LocationsOptions
let defaultJourneysOptions = Default.JourneysOptions
