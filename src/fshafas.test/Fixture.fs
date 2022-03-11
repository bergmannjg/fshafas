module Fixture

open System.IO

let path = "../../../fixtures/"

let jsonLocationsRawResponse () =
    File.ReadAllText(path + "db-locations-raw-response.json")

let jsonLocationsResponse () =
    File.ReadAllText(path + "db-locations-response.json")

let jsonJouneysOptions () =
    File.ReadAllText(path + "db-journeys-options.json")

let jsonJouneysRawResponse () =
    File.ReadAllText(path + "db-journeys-raw-response.json")

let jsonJouneysResponse () =
    File.ReadAllText(path + "db-journeys-response.json")

let jsonDeparturesRawResponse () =
    File.ReadAllText(path + "db-departures-raw-response.json")

let jsonDeparturesResponse () =
    File.ReadAllText(path + "db-departures-response.json")

let jsonReachableFromRawResponse () =
    File.ReadAllText(path + "db-reachableFrom-raw-response.json")

let jsonReachableFromResponse () =
    File.ReadAllText(path + "db-reachableFrom-response.json")

let jsonRadarRawResponse () =
    File.ReadAllText(path + "db-radar-raw-response.json")

let jsonRadarResponse () =
    File.ReadAllText(path + "db-radar-response.json")

let jsonLinesRawResponse () =
    File.ReadAllText(path + "db-lines-raw-response.json")

let jsonLinesResponse () =
    File.ReadAllText(path + "db-lines-response.json")

let jsonRemarksRawResponse () =
    File.ReadAllText(path + "db-remarks-raw-response.json")

let jsonRemarksResponse () =
    File.ReadAllText(path + "db-remarks-response.json")

let jsonTripRawResponse () =
    File.ReadAllText(path + "db-trip-raw-response.json")

let jsonTripResponse () =
    File.ReadAllText(path + "db-trip-response.json")

let jsonNearbyOptions () =
    File.ReadAllText(path + "db-nearby-options.json")

let jsonNearbyRawResponse () =
    File.ReadAllText(path + "db-nearby-raw-response.json")

let jsonNearbyResponse () =
    File.ReadAllText(path + "db-nearby-response.json")

let jsonJourneysFromTripRawResponse () =
    File.ReadAllText(path + "db-journeysFromTrip-raw-response.json")

let jsonJourneysFromTripResponse () =
    File.ReadAllText(path + "db-journeysFromTrip-response.json")
