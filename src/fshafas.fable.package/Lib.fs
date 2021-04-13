module App

open Fable.Core
open FsHafas.Client
open FsHafas.Api

let private getFsHafasProfileId (name: string) : FsHafas.Client.ProfileId =
    match name with
    | "db" -> FsHafas.Client.ProfileId.Db
    | "bvg" -> FsHafas.Client.ProfileId.Bvg
    | _ -> raise (System.ArgumentException("profile unkown: " + name))

let private getFsHafasProfile (id:FsHafas.Client.ProfileId) =
    match id with
    | Db -> FsHafas.Profiles.Db.getProfile ()
    | Bvg -> FsHafas.Profiles.Bvg.getProfile ()
    | _ -> raise (System.ArgumentException("profile unkown: " + id.ToString()))

let createClient (profile: string) =
    new HafasClient(getFsHafasProfileId (profile))

let setDebug () = Log.Debug <- true

let getProfile (profile: string) =
    ClientProfile.FromFsHafasProfile(getFsHafasProfile (getFsHafasProfileId profile))

let printLocations = FsHafas.Printf.Short.Locations
let printJourneys = FsHafas.Printf.Short.Journeys

let defaultLocationsOptions = Default.LocationsOptions
let defaultJourneysOptions = Default.JourneysOptions
