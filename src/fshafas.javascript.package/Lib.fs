module App

open FsHafas.Client
open FsHafas.Api

let dbProfile = FsHafas.Profiles.Db.profile :> FsHafas.Client.Profile

let bvgProfile = FsHafas.Profiles.Bvg.profile :> FsHafas.Client.Profile

let createClient (profile: FsHafas.Client.Profile) = HafasClient(profile)

let setDebug () = Log.Debug <- true

let getProfile (profile: string) =
    match profile with
    | "db" -> FsHafas.Profiles.Db.profile :> FsHafas.Client.Profile
    | "bvg" -> FsHafas.Profiles.Bvg.profile :> FsHafas.Client.Profile
    | _ -> raise (System.ArgumentException("profile unkown: " + profile))

let printLocations = FsHafas.Printf.Short.Locations
let printJourneys = FsHafas.Printf.Short.Journeys

let defaultLocationsOptions = Default.LocationsOptions
let defaultJourneysOptions = Default.JourneysOptions
