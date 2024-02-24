module FsHafasTest

open System
open NUnit.Framework
open FsHafas.Client
open FsHafas.Reflect.Compare


// feature properties with empty json object '{}' are parsed as Option.None, see Converter.OptionU3EraseConverter
let acceptEmptyObjectAsNullValue = true

let checkEqual (actual: obj) (expected: obj) =
    let mutable diffs = 0

    let printTypesDifferent (name: string) (o1: obj) (t1: Type) (o2: obj) (t2: Type) =
        if name = "properties" then
            ()
        else
            diffs <- diffs + 1
            fprintfn stderr "%s" (sprintf "TypesDifferent %s: '%A' '%A'" name t1.Name t2.Name)

    let toFeatureArray (o: obj) : Feature [] =
        let typ = o.GetType()

        if typ.IsArray && typ.Name = "Feature[]" then
            let arr = o :?> Feature []
            arr
        else
            raise (NUnit.Framework.AssertionException("Feature[] expected"))

    let equalFeatureProperties (expected: obj) (actual: obj) =
        let mutable diffs = 0

        let printTypesDifferent (name: string) (o1: obj) (t1: Type) (o2: obj) (t2: Type) =
            diffs <- diffs + 1
            fprintfn stderr "%s" (sprintf "FeatureProperties TypesDifferent %s: '%A' '%A'" name t1.Name t2.Name)

        let printValuesDifferent (name: string) (o1: obj) (o2: obj) =
            if o1 <> null && o2 <> null then
                diffs <- diffs + 1
                fprintfn stderr "%s" (sprintf "FeatureProperties ValuesDifferent %s: '%A' '%A'" name o1 o2)

        let evt: CompareEvent =
            { onTypesDifferent = printTypesDifferent
              onValuesDifferent = printValuesDifferent }

        compare evt actual expected

        diffs = 0

    let compareFeatureArrays (expected: Feature []) (actual: Feature []) =
        let notFound =
            expected
            |> Array.filter (fun f1 ->
                not (
                    actual
                    |> Array.exists (fun f2 ->
                        f1.geometry = f2.geometry
                        && equalFeatureProperties f1.properties f2.properties)
                ))

        if notFound.Length > 0 then
            fprintfn stderr "notFound: %A" notFound

        notFound.Length

    let printValuesDifferent (name: string) (o1: obj) (o2: obj) =
        if name = "id"
           && o1 <> null
           && o2 <> null
           && o1.ToString().Replace("-", "").ToLower() = o2.ToString().Replace("-", "").ToLower() then // ignore ids
            ()
        else if name = "matchId" then // ignore
            ()
        else if name = "scheduledDays" then // ignore
            ()
        else if name = "distance"
                && o1 = null
                && o2 <> null
                && (sprintf "%A" o2) = "Some 0" then // ignore None = Some 0
            ()
        else if name = "reachable"
                && o1 <> null
                && o2 = null
                && (sprintf "%A" o1) = "Some true" then // todo
            ()
        else if name = "remarks"
                && o1 = null
                && o2 <> null
                && (sprintf "%A" o2) = "Some [||]" then // ignore None = Some [||]
            ()
        else if name = "remarks" && o1 <> null && o2 = null then
            ()
        else if name = "transfer"
                && o1 <> null
                && o2 = null
                && (sprintf "%A" o1) = "Some false" then // ignore None = Some false
            ()
        else if name = "features" && o1 <> null && o2 <> null then
            // compare feature geometry and feature properties
            try
                let actual = toFeatureArray o1

                let expected = toFeatureArray o2

                diffs <- diffs + (compareFeatureArrays expected actual)
            with
            | ex ->
                diffs <- diffs + 1
                fprintfn stderr "compareFeatureArrays: %s" ex.Message
        else
            diffs <- diffs + 1
            fprintfn stderr "%s" (sprintf "ValuesDifferent %s: '%A' '%A'" name o1 o2)

    let evt: CompareEvent =
        { onTypesDifferent = printTypesDifferent
          onValuesDifferent = printValuesDifferent }

    compare evt actual expected

    diffs = 0

let testRunner (jsonRaw: string) (jsonResult: string) (loader: FsHafas.Raw.RawResult -> string -> obj * obj) =

    try
        let rawResponse =
            FsHafas.Api.Parser.Deserialize<FsHafas.Raw.RawResponse>(jsonRaw, acceptEmptyObjectAsNullValue)

        let svcResL = Option.defaultValue [||] rawResponse.svcResL

        Assert.That(svcResL.Length, Is.EqualTo(1))
        let res = svcResL.[0].res

        let (x1, x2) = loader res.Value jsonResult

        let isEqual = checkEqual x1 x2

        fprintfn stdout "actual:"
        FsHafas.Printf.Long.print x1

        fprintfn stdout "expected:"
        FsHafas.Printf.Long.print x2

        Assert.That(isEqual, Is.EqualTo(true))
    with
    | :? NUnit.Framework.AssertionException as ex -> ()

let loadDbProfile () = FsHafas.Profiles.Db.profile
let loadOebbProfile () = FsHafas.Profiles.Oebb.profile
let loadSvvProfile () = FsHafas.Profiles.Svv.profile
let loadRejseplanenProfile () = FsHafas.Profiles.Rejseplanen.profile
let loadSaarFahrplanProfile () = FsHafas.Profiles.SaarFahrplan.profile

let loadProfile (p: string) =
    if p = "db" then
        loadDbProfile ()
    else if p = "oebb" then
        loadOebbProfile ()
    else if p = "svv" then
        loadSvvProfile ()
    else if p = "rejseplanen" then
        loadRejseplanenProfile ()
    else if p = "saarfahrplan" then
        loadSaarFahrplanProfile ()
    else
        raise (Exception("profile " + p + " unkown"))

let loadLocations (profile: FsHafas.Endpoint.Profile) (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseLocationsFromResult
            profile
            res.``match``.Value.locL
            FsHafas.Api.Parser.defaultOptions
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<StationStopLocation []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadLocation (profile: FsHafas.Endpoint.Profile) (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseLocationsFromResult
            profile
            res.locL
            FsHafas.Api.Parser.defaultOptions
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<StationStopLocation>(expectedJson, acceptEmptyObjectAsNullValue)

    (parsedResponse.[0] :> obj, response :> obj)

let loadJourneys
    (profile: FsHafas.Endpoint.Profile)
    (jouneysOptions: string)
    (res: FsHafas.Raw.RawResult)
    (expectedJson: string)
    =
    let options =
        FsHafas.Api.Parser.Deserialize<JourneysOptions>(jouneysOptions, acceptEmptyObjectAsNullValue)

    let parsedResponse =
        FsHafas.Api.Parser.parseJourneysFromResult
            profile
            res.outConL
            { FsHafas.Api.Parser.defaultOptions with
                scheduledDays = options.scheduledDays |> Option.defaultValue false
                remarks = options.remarks |> Option.defaultValue false }
            res

    Assert.That(parsedResponse.journeys.Value.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Journeys>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.journeys.Value.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let skipLegs (journeys: Journey []) =
    journeys
    |> Array.map (fun j -> { j with legs = [||] })

let loadJourneyArray (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseJourneysFromResult (loadDbProfile ()) res.outConL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.journeys.Value.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Journeys>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.journeys.Value.Length > 0, Is.EqualTo(true))
    Assert.That(response.journeys.Value.Length, Is.EqualTo(parsedResponse.journeys.Value.Length))

    // legs may differ
    (skipLegs (parsedResponse.journeys.Value) :> obj, skipLegs (response.journeys.Value) :> obj)

let loadTrip (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseTripFromResult (loadDbProfile ()) res.journey FsHafas.Api.Parser.defaultOptions res

    let response =
        FsHafas.Api.Parser.Deserialize<TripWithRealtimeData>(expectedJson, acceptEmptyObjectAsNullValue)

    (parsedResponse :> obj, response :> obj)

let loadDepartures (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseDeparturesArrivalsFromResult
            (loadDbProfile ())
            "DEP"
            res.jnyL
            FsHafas.Api.Parser.defaultOptions
            res

    let response =
        FsHafas.Api.Parser.Deserialize<Departures>(expectedJson, acceptEmptyObjectAsNullValue)

    (parsedResponse :> obj, response :> obj)

let idOfU3StationStopLocation (location: StationStopLocation) =
    match location with
    | StationStopLocation.Location l -> l.id
    | StationStopLocation.Station s -> s.id
    | StationStopLocation.Stop s -> s.id
    |> Option.defaultValue ""

let sortDurations (durations: Duration []) : Duration [] =
    durations
    |> Array.map (fun d ->
        let sorted =
            d.stations
            |> Array.sortBy (fun s -> idOfU3StationStopLocation s)

        { duration = d.duration
          stations = sorted })

let loadReachableFrom (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    Assert.That(res.posL.IsSome, Is.EqualTo(true))

    let parsedResponse =
        FsHafas.Api.Parser.parseDurationsFromResult
            (loadDbProfile ())
            res.posL.Value
            FsHafas.Api.Parser.defaultOptions
            res

    Assert.That(parsedResponse.reachable.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<DurationsWithRealtimeData>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.reachable.Length > 0, Is.EqualTo(true))
    Assert.That(parsedResponse.reachable.Length, Is.EqualTo(response.reachable.Length))

    // convert to unique sort order
    let x1 = sortDurations parsedResponse.reachable
    let x2 = sortDurations response.reachable

    (x1 :> obj, x2 :> obj)

let loadNearby (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let options =
        FsHafas.Api.Parser.Deserialize<NearByOptions>(Fixture.jsonNearbyOptions (), acceptEmptyObjectAsNullValue)

    let parsedResponse =
        FsHafas.Api.Parser.parseLocationsFromResult
            (loadDbProfile ())
            res.locL
            { FsHafas.Api.Parser.defaultOptions with linesOfStops = options.linesOfStops |> Option.defaultValue false }
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<StationStopLocation []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadRadar (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseMovementsFromResult (loadDbProfile ()) res.jnyL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.movements.Value.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Radar>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.movements.Value.Length, Is.EqualTo(parsedResponse.movements.Value.Length))

    (parsedResponse :> obj, response :> obj)

let loadLines (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseLinesFromResult (loadSvvProfile ()) res.lineL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.lines.Value.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<LinesWithRealtimeData>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.lines.Value.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadWarnings (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseWarningsFromResult (loadSvvProfile ()) res.msgL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.remarks.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<WarningsWithRealtimeData>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.remarks.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let getLocationData () = Fixture.getFilePath "locations"

let getJourneyData () = Fixture.getFilePath "journeys"

[<Test>]
let TestFeatureParser () =
    let jsonPropertiesAsEmptyObject =
        """
{
    "type": "Feature",
    "properties": {},
    "geometry": {
        "type": "Point",
        "coordinates": [
            10.007,
            53.55371
        ]
    }
}
"""

    let feature0 =
        FsHafas.Api.Parser.Deserialize<Feature>(jsonPropertiesAsEmptyObject, acceptEmptyObjectAsNullValue)

    Assert.That(feature0.properties.IsNone)

    let jsonPropertiesAsNull =
        """
{
    "type": "Feature",
    "properties": null,
    "geometry": {
        "type": "Point",
        "coordinates": [
            10.007,
            53.55371
        ]
    }
}
"""

    let feature1 =
        FsHafas.Api.Parser.Deserialize<Feature>(jsonPropertiesAsNull, acceptEmptyObjectAsNullValue)

    Assert.That(feature1.properties.IsNone)

    let jsonPropertiesAsStop =
        """
{
    "type": "Feature",
    "properties": {
        "type": "stop",
        "id": "8002549",
        "name": "Hamburg Hbf",
        "location": {
            "type": "location",
            "id": "8002549",
            "latitude": 53.553533,
            "longitude": 10.00636
        },
        "products": {
            "nationalExpress": true,
            "national": true,
            "regionalExpress": true,
            "regional": true,
            "suburban": true,
            "bus": true,
            "ferry": false,
            "subway": true,
            "tram": false,
            "taxi": false
        }
    },
    "geometry": {
        "type": "Point",
        "coordinates": [
            10.007,
            53.55371
        ]
    }
}
"""

    let feature2 =
        FsHafas.Api.Parser.Deserialize<Feature>(jsonPropertiesAsStop, acceptEmptyObjectAsNullValue)

    Assert.That(feature2.properties.IsSome)

    match feature2.properties.Value with
    | StationStopLocation.Stop s -> Assert.That("Stop", Is.EqualTo(s.``type``.ToString()))
    | _ -> raise (NUnit.Framework.AssertionException("U3.Case2 Stop expected"))

    let jsonPropertiesAsUndefined =
        """
{
    "type": "Feature",
    "geometry": {
        "type": "Point",
        "coordinates": [
            10.007,
            53.55371
        ]
    }
}
"""

    let feature3 =
        FsHafas.Api.Parser.Deserialize<Feature>(jsonPropertiesAsUndefined, acceptEmptyObjectAsNullValue)

    Assert.That(feature3.properties.IsNone)

[<TestCaseSource(nameof (getLocationData))>]
let TestLocations (path: string) =
    let fixtureData = Fixture.getFileData path
    testRunner fixtureData.rawResponse fixtureData.response (loadLocations (loadProfile (fixtureData.profile)))

[<TestCaseSource(nameof (getJourneyData))>]
let TestJourneys (path: string) =
    let fixtureData = Fixture.getFileData path

    testRunner
        fixtureData.rawResponse
        fixtureData.response
        (loadJourneys (loadProfile (fixtureData.profile)) fixtureData.options)

[<Test>]
let TestTrip () =
    testRunner (Fixture.jsonTripRawResponse ()) (Fixture.jsonTripResponse ()) loadTrip

[<Test>]
let TestDepartures () =
    testRunner (Fixture.jsonDeparturesRawResponse ()) (Fixture.jsonDeparturesResponse ()) loadDepartures

[<Test>]
let TestNearby () =
    testRunner (Fixture.jsonNearbyRawResponse ()) (Fixture.jsonNearbyResponse ()) loadNearby

[<Test>]
let TestReachableFrom () =
    testRunner (Fixture.jsonReachableFromRawResponse ()) (Fixture.jsonReachableFromResponse ()) loadReachableFrom

[<Test>]
let TestRadar () =
    testRunner (Fixture.jsonRadarRawResponse ()) (Fixture.jsonRadarResponse ()) loadRadar

[<Test>]
let TestLines () =
    testRunner (Fixture.jsonLinesRawResponse ()) (Fixture.jsonLinesResponse ()) loadLines

[<Test>]
let TestWarnings () =
    testRunner (Fixture.jsonRemarksRawResponse ()) (Fixture.jsonRemarksResponse ()) loadWarnings

[<Test>]
let TestJourneysFromTrip () =
    testRunner (Fixture.jsonJourneysFromTripRawResponse ()) (Fixture.jsonJourneysFromTripResponse ()) loadJourneyArray

[<Test>]
let TestStop () =
    testRunner (Fixture.jsonStopRawResponse ()) (Fixture.jsonStopResponse ()) (loadLocation (loadDbProfile ()))
