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
        else if name = "distance"
                && o1 = null
                && o2 <> null
                && (sprintf "%A" o2) = "Some 0" then // ignore None = Some 0
            ()
        else if name = "remarks"
                && o1 = null
                && o2 <> null
                && (sprintf "%A" o2) = "Some [||]" then // ignore None = Some [||]
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
let loadSvvProfile () = FsHafas.Profiles.Svv.profile

let loadLocations (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseLocationsFromResult
            (loadDbProfile ())
            res.``match``.Value.locL
            FsHafas.Api.Parser.defaultOptions
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<U3<Station, Stop, Location> []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadJourneys (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let options =
        FsHafas.Api.Parser.Deserialize<JourneysOptions>(Fixture.jsonJouneysResponse (), acceptEmptyObjectAsNullValue)

    let parsedResponse =
        FsHafas.Api.Parser.parseJourneysFromResult
            (loadDbProfile ())
            res.outConL
            { FsHafas.Api.Parser.defaultOptions with
                scheduledDays = options.scheduledDays |> Option.defaultValue false
                remarks = options.remarks |> Option.defaultValue false }
            res

    Assert.That(parsedResponse.journeys.IsSome, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Journeys>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.journeys.IsSome, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let skipLegs (journeys: Journey []) =
    journeys
    |> Array.map (fun j -> { j with legs = [||] })

let loadJourneyArray (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseJourneysArrayFromResult
            (loadDbProfile ())
            res.outConL
            FsHafas.Api.Parser.defaultOptions
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Journey []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))
    Assert.AreEqual(response.Length, parsedResponse.Length)

    // legs may differ
    (skipLegs (parsedResponse) :> obj, skipLegs (response) :> obj)

let loadTrip (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseTripFromResult (loadDbProfile ()) res.journey FsHafas.Api.Parser.defaultOptions res

    let response =
        FsHafas.Api.Parser.Deserialize<Trip>(expectedJson, acceptEmptyObjectAsNullValue)

    (parsedResponse :> obj, response :> obj)

let loadDepartures (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let xloadDbProfile = FsHafas.Profiles.Db.profile

    let parsedResponse =
        FsHafas.Api.Parser.parseDeparturesArrivalsFromResult
            (loadDbProfile ())
            "DEP"
            res.jnyL
            FsHafas.Api.Parser.defaultOptions
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Alternative []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let idOfU3StationStopLocation (location: U3<Station, Stop, Location>) =
    match location with
    | U3.Case3 l -> l.id
    | U3.Case2 s -> s.id
    | U3.Case1 s -> s.id
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

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Duration []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))
    Assert.That(parsedResponse.Length, Is.EqualTo(response.Length))

    // convert to unique sort order
    let x1 = sortDurations parsedResponse
    let x2 = sortDurations response

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
        FsHafas.Api.Parser.Deserialize<U3<Station, Stop, Location> []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadRadar (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseMovementsFromResult (loadDbProfile ()) res.jnyL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Movement []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadLines (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseLinesFromResult (loadSvvProfile ()) res.lineL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Line []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadWarnings (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseWarningsFromResult (loadSvvProfile ()) res.msgL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Warning []>(expectedJson, acceptEmptyObjectAsNullValue)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

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

    Assert.IsTrue(feature0.properties.IsNone)

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

    Assert.IsTrue(feature1.properties.IsNone)

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
            "regionalExp": true,
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

    Assert.IsTrue(feature2.properties.IsSome)

    match feature2.properties.Value with
    | U3.Case2 s -> Assert.AreEqual("Stop", s.``type``.ToString())
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

    Assert.IsTrue(feature3.properties.IsNone)

[<Test>]
let TestLocations () =
    testRunner (Fixture.jsonLocationsRawResponse ()) (Fixture.jsonLocationsResponse ()) loadLocations

[<Test>]
let TestJourneys () =
    testRunner (Fixture.jsonJouneysRawResponse ()) (Fixture.jsonJouneysResponse ()) loadJourneys

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
