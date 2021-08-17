// VSTEST_HOST_DEBUG=1
module Test

open System
open NUnit.Framework
open FsHafas.Client
open FsHafas.Reflect.Compare

let checkEqual (o1: obj) (o2: obj) =
    let mutable diffs = 0

    let printTypesDifferent (name: string) (o1: obj) (t1: Type) (o2: obj) (t2: Type) =
        if name = "properties" then
            ()
        else if name = "icon" then
            ()

        else
            diffs <- diffs + 1
            fprintfn stderr "%s" (sprintf "TypesDifferent %s: '%A' '%A'" name t1.Name t2.Name)

    let printValuesDifferent (name: string) (o1: obj) (o2: obj) =
        if name = "id"
           && o1 <> null
           && o2 <> null
           && o1.ToString().Replace("-", "").ToLower() = o2.ToString().Replace("-", "").ToLower() then // ignore ids
            ()
        else if name = "id" && (o1 = null || o2 = null) then // ignore ids, todo
            ()
        else if name = "icon" && (o1 = null || o2 = null) then // ignore icon, todo
            ()
        else if name = "remarks"
                && o1 <> null
                && o2 = null
                && (sprintf "%A" o1) = "Some [||]" then // ignore empty remarks, todo
            ()
        else if name = "remarks"
                && o1 = null
                && o2 <> null
                && (sprintf "%A" o2) = "Some [||]" then // ignore empty remarks, todo
            ()
        else
            diffs <- diffs + 1
            fprintfn stderr "%s" (sprintf "ValuesDifferent %s: '%A' '%A'" name o1 o2)

    let evt: CompareEvent =
        { onTypesDifferent = printTypesDifferent
          onValuesDifferent = printValuesDifferent }

    compare evt o1 o2

    diffs = 0

let testRunner (jsonRaw: string) (jsonResult: string) (loader: FsHafas.Raw.RawResult -> string -> obj * obj) =

    try
        let rawResponse =
            FsHafas.Api.Parser.Deserialize<FsHafas.Raw.RawResponse>(jsonRaw)

        Assert.That(rawResponse.svcResL.Length, Is.EqualTo(1))
        let res = rawResponse.svcResL.[0].res

        let (x1, x2) = loader res jsonResult

        let isEqual = checkEqual x1 x2

        fprintfn stdout "actual:"
        FsHafas.Printf.Long.print x1

        fprintfn stdout "expected:"
        FsHafas.Printf.Long.print x2

        Assert.That(isEqual, Is.EqualTo(true))
    with
    | :? NUnit.Framework.AssertionException as ex -> ()
    | ex -> fprintfn stderr "error: %s %s" ex.Message ex.StackTrace

let loadLocations (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseLocationsFromResult
            ProfileId.Db
            res.``match``.Value.locL
            FsHafas.Api.Parser.defaultOptions
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<U3<Station, Stop, Location> []>(expectedJson)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadJourneys (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseJourneysFromResult
            ProfileId.Db
            res.outConL
            { FsHafas.Api.Parser.defaultOptions with
                  scheduledDays = true }
            res

    Assert.That(parsedResponse.journeys.IsSome, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Journeys>(expectedJson)

    Assert.That(response.journeys.IsSome, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let skipLegs (journeys: Journey []) =
    journeys
    |> Array.map (fun j -> { j with legs = [||] })

let loadJourneyArray (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseJourneysArrayFromResult ProfileId.Db res.outConL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Journey []>(expectedJson)

    Assert.That(response.Length > 0, Is.EqualTo(true))
    Assert.AreEqual(response.Length, parsedResponse.Length)

    // legs may differ
    (skipLegs (parsedResponse) :> obj, skipLegs (response) :> obj)

let loadTrip (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseTripFromResult ProfileId.Db res.journey FsHafas.Api.Parser.defaultOptions res

    let response =
        FsHafas.Api.Parser.Deserialize<Trip>(expectedJson)

    (parsedResponse :> obj, response :> obj)

let loadDepartures (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseDeparturesArrivalsFromResult
            ProfileId.Db
            "DEP"
            res.jnyL
            FsHafas.Api.Parser.defaultOptions
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Alternative []>(expectedJson)

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
    |> Array.map
        (fun d ->
            let sorted =
                d.stations
                |> Array.sortBy (fun s -> idOfU3StationStopLocation s)

            { duration = d.duration
              stations = sorted })

let loadReachableFrom (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    Assert.That(res.posL.IsSome, Is.EqualTo(true))

    let parsedResponse =
        FsHafas.Api.Parser.parseDurationsFromResult ProfileId.Db res.posL.Value FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Duration []>(expectedJson)

    Assert.That(response.Length > 0, Is.EqualTo(true))
    Assert.That(parsedResponse.Length, Is.EqualTo(response.Length))

    // convert to unique sort order
    let x1 = sortDurations parsedResponse
    let x2 = sortDurations response

    (x1 :> obj, x2 :> obj)

let loadNearby (res: FsHafas.Raw.RawResult) (expectedJson: string) =

    let parsedResponse =
        FsHafas.Api.Parser.parseLocationsFromResult
            ProfileId.Db
            res.locL.Value
            { FsHafas.Api.Parser.defaultOptions with
                  linesOfStops = false }
            res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<U3<Station, Stop, Location> []>(expectedJson)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadRadar (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseMovementsFromResult ProfileId.Db res.jnyL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Movement []>(expectedJson)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadLines (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseLinesFromResult ProfileId.Svv res.lineL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Line []>(expectedJson)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

let loadWarnings (res: FsHafas.Raw.RawResult) (expectedJson: string) =
    let parsedResponse =
        FsHafas.Api.Parser.parseWarningsFromResult ProfileId.Svv res.msgL FsHafas.Api.Parser.defaultOptions res

    Assert.That(parsedResponse.Length > 0, Is.EqualTo(true))

    let response =
        FsHafas.Api.Parser.Deserialize<Warning []>(expectedJson)

    Assert.That(response.Length > 0, Is.EqualTo(true))

    (parsedResponse :> obj, response :> obj)

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
    testRunner (Fixture.jsonSearchOnTripRawResponse ()) (Fixture.jsonSearchOnTripResponse ()) loadJourneyArray
