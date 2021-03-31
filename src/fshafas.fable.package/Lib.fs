module App

open Fable.Core
open FsHafas

let private getFsHafasProfile (name: string) : FsHafas.Profile =
    match name with
    | "db" -> FsHafas.Profiles.Db.getProfile ()
    | _ -> raise (System.ArgumentException("profile unkown: " + name))

let createClient (profile: string) =
    new HafasClient(getFsHafasProfile (profile))

let setDebug () = Log.Debug <- true

let getProfile (name: string) =
    ClientProfile.FromFsHafasProfile(getFsHafasProfile name)

let printLocations = FsHafas.Printf.Short.Locations
let printJourneys = FsHafas.Printf.Short.Journeys

let defaultLocationsOptions = Default.LocationsOptions
let defaultJourneysOptions = Default.JourneysOptions
