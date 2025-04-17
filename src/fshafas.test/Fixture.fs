module Fixture

open System.IO

type FixturyData =
    { profile: string
      rawResponse: string
      response: string
      options: string }

let private path = "../../../fixtures/"

let getFilePath (method: string) : string[] =
    Directory.GetFiles(path, "*-" + method + "-response.json")

let getFileData (path: string) : FixturyData =
    let profile = FileInfo(path).Name.Split('-')[0]
    let response = path
    let rawResponse = response.Replace("-response", "-raw-response")
    let options = response.Replace("-response", "-options")

    { profile = profile
      rawResponse = File.ReadAllText(rawResponse)
      response = File.ReadAllText(response)
      options = File.ReadAllText(options) }

let jsonDeparturesRawResponse () =
    File.ReadAllText(path + "oebb-departures-raw-response.json")

let jsonDeparturesResponse () =
    File.ReadAllText(path + "oebb-departures-response.json")

let jsonReachableFromRawResponse () =
    File.ReadAllText(path + "oebb-reachableFrom-raw-response.json")

let jsonReachableFromResponse () =
    File.ReadAllText(path + "oebb-reachableFrom-response.json")

let jsonRadarRawResponse () =
    File.ReadAllText(path + "oebb-radar-raw-response.json")

let jsonRadarResponse () =
    File.ReadAllText(path + "oebb-radar-response.json")

let jsonLinesRawResponse () =
    File.ReadAllText(path + "svv-lines-raw-response.json")

let jsonLinesResponse () =
    File.ReadAllText(path + "svv-lines-response.json")

let jsonRemarksRawResponse () =
    File.ReadAllText(path + "svv-remarks-raw-response.json")

let jsonRemarksResponse () =
    File.ReadAllText(path + "svv-remarks-response.json")

let jsonTripRawResponse () =
    File.ReadAllText(path + "oebb-trip-raw-response.json")

let jsonTripResponse () =
    File.ReadAllText(path + "oebb-trip-response.json")

let jsonNearbyOptions () =
    File.ReadAllText(path + "oebb-nearby-options.json")

let jsonNearbyRawResponse () =
    File.ReadAllText(path + "oebb-nearby-raw-response.json")

let jsonNearbyResponse () =
    File.ReadAllText(path + "oebb-nearby-response.json")

let jsonJourneysFromTripRawResponse () =
    File.ReadAllText(path + "oebb-journeysFromTrip-raw-response.json")

let jsonJourneysFromTripResponse () =
    File.ReadAllText(path + "oebb-journeysFromTrip-response.json")

let jsonStopRawResponse () =
    File.ReadAllText(path + "oebb-stop-raw-response.json")

let jsonStopResponse () =
    File.ReadAllText(path + "oebb-stop-response.json")
