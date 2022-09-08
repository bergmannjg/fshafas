module App

open FsHafas.Client
open FsHafas.Api

let createClient (profile: FsHafas.Client.Profile) = HafasClient(profile)

let setDebug () = Log.Debug <- true

let printLocations = FsHafas.Printf.Short.Locations
let printJourneys = FsHafas.Printf.Short.Journeys

let defaultLocationsOptions = Default.LocationsOptions
let defaultJourneysOptions = Default.JourneysOptions
