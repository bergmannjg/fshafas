module Fixture

open System.IO

type FixturyData =
    { profile: string
      rawResponse: string
      response: string
      options: string }

let private path = "../../../fixtures/"

let getFileData (method: string) : FixturyData [] =
    Directory.GetFiles(path, "*-" + method + "-*response.json")
    |> Array.map (fun f -> (FileInfo(f).Name.Split('-')[0], FileInfo(f).Name))
    |> Array.groupBy (fun (p, _) -> p)
    |> Array.choose (fun (k, l) ->
        let rawResponse =
            l
            |> Array.tryFind (fun (_, f) -> f.Contains("-raw-response"))

        let response =
            l
            |> Array.tryFind (fun (_, f) -> f.Contains(method + "-response"))

        let optionsFile = path + k + "-" + method + "-options.json"

        let options =
            if File.Exists(optionsFile) then
                File.ReadAllText(optionsFile)
            else
                raise (System.Exception(optionsFile + " not found"))

        match rawResponse, response with
        | Some (_, rawResponse), Some (_, response) ->
            Some(
                { profile = k
                  rawResponse = File.ReadAllText(path + rawResponse)
                  response = File.ReadAllText(path + response)
                  options = options }
            )
        | _ -> None)

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
    File.ReadAllText(path + "svv-lines-raw-response.json")

let jsonLinesResponse () =
    File.ReadAllText(path + "svv-lines-response.json")

let jsonRemarksRawResponse () =
    File.ReadAllText(path + "svv-remarks-raw-response.json")

let jsonRemarksResponse () =
    File.ReadAllText(path + "svv-remarks-response.json")

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
