/// analyze journeydata from the calendar csv export of DB Navigator app
module App

open FsHafas
open FsHafas.Client

open Argu

type Bahncard =
    | No = 0
    | BC25 = 25
    | BC50 = 50

type CliArguments =
    | File of path: string
    | DateStart of date: string
    | DateEnd of date: string
    | PriceInNDays of offset: int
    | Take of count: int
    | Discount of Bahncard
    | Debug

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | File _ -> "path to csv file."
            | DateStart _ -> "start date, default 01/01/20"
            | DateEnd _ -> "end date, default 08/28/21"
            | PriceInNDays _ -> "get price in n days, deafult 1."
            | Take _ -> "take only count elems, default 1000."
            | Discount _ -> "Discount of Bahncard, default BC25."
            | Debug _ -> "show debug msgs."

let csvjourneydata (path: string) = FSharp.Data.CsvFile.Load(path).Cache()

type JourneyData =
    { ifFrom: string
      idTo: string
      dtStart: System.DateTime
      dtEnd: System.DateTime }

type JourneyInfo =
    { idFrom: string
      idTo: string
      dtStart: System.DateTime
      dtEnd: System.DateTime
      km: float
      price: float }

let subjectRegex =
    System.Text.RegularExpressions.Regex @"(?i)^Fahrt von (.+) nach ([^;]+)"

let rowToJourneydata (dtFrom: System.DateTime) (dtTo: System.DateTime) (row: FSharp.Data.CsvRow) =
    let subject = row.GetColumn "Subject"
    let strStartDate = row.GetColumn "Start Date"
    let strStartTime = row.GetColumn "Start Time"
    let strEndDate = row.GetColumn "End Date"
    let strEndTime = row.GetColumn "End Time"

    let startdate =
        System.DateTime.ParseExact(strStartDate + " " + strStartTime, "MM/dd/yy h:mm:ss tt", null)

    let enddate =
        System.DateTime.ParseExact(strEndDate + " " + strEndTime, "MM/dd/yy h:mm:ss tt", null)

    let m = subjectRegex.Match subject

    if m.Groups.Count = 3
       && startdate >= dtFrom
       && startdate <= dtTo then
        Some
            { ifFrom = m.Groups.[1].Value
              idTo = m.Groups.[2].Value
              dtStart = startdate
              dtEnd = enddate }
    else
        None

let journeydata (path: string) (dtFrom: System.DateTime) (dtTo: System.DateTime) =
    (csvjourneydata path).Rows
    |> Seq.map (rowToJourneydata dtFrom dtTo)
    |> Seq.choose id
    |> Seq.toArray
    |> Array.sortBy (fun x -> x.dtStart)

let getIdOfFirstStop (arr: StationStopLocation array) =
    let test (u3: StationStopLocation) =
        match u3 with
        | StationStopLocation.Station station -> station.id
        | StationStopLocation.Stop stop -> stop.id
        | StationStopLocation.Location _ -> None

    arr |> Array.tryPick test

let getLocationId (client: Api.HafasAsyncClient) (name: string) =
    let options = { Default.LocationsOptions with results = Some 3 }

    async {
        let! locations = client.AsyncLocations name (Some options)

        return (getIdOfFirstStop locations)
    }

let getJourney
    (client: Api.HafasAsyncClient)
    (from: string)
    (``to``: string)
    (incrDays: int)
    (start: System.DateTime)
    (discount: int)
    =
    async {
        try
            let! fromId = getLocationId client from
            let! toId = getLocationId client ``to``

            let departure =
                System
                    .DateTime
                    .Today
                    .AddDays(float incrDays)
                    .AddHours(float start.Hour)

            let options =
                { Default.JourneysOptions with
                    departure = Some departure
                    loyaltyCard =
                        if discount <> 25 && discount <> 50 then
                            None
                        else
                            Some
                                { ``type`` = "Bahncard"
                                  discount = Some discount
                                  ``class`` = Some 2 }
                    polylines = Some true }

            return!
                async {
                    match fromId, toId with
                    | Some (f), Some (t) ->
                        let! result = client.AsyncJourneys (U4.Case1 f) (U4.Case1 t) (Some options)

                        match result.journeys with
                        | Some j when j.Length > 0 -> return Some j.[0]
                        | _ -> return None
                    | _ -> return None
                }
        with
        | ex ->
            fprintfn stderr "error:%s" ex.Message
            return None
    }

let getJourneyInfo (client: Api.HafasAsyncClient) (data: JourneyData) (incrDays: int) (discount: int) =
    async {
        match! getJourney client data.ifFrom data.idTo incrDays data.dtStart discount with
        | Some j ->
            return
                Some
                    { idFrom = data.ifFrom
                      idTo = data.idTo
                      dtStart = data.dtStart
                      dtEnd = data.dtEnd
                      km = client.distanceOfJourney j
                      price =
                        match j.price with
                        | Some p -> p.amount
                        | None -> 0.0 }
        | None -> return None
    }

let takeChecked count (array: 'T []) =
    if count >= array.Length then
        array
    else
        Array.take count array

[<EntryPoint>]
let main argv =
    try

        let parser =
            ArgumentParser.Create<CliArguments>(programName = "analyzejourneys.exe")

        let options = parser.Parse(argv)

        let file = options.GetResult(File, defaultValue = "../../../calendar.csv")

        let priceInNDays = options.GetResult(PriceInNDays, defaultValue = 1)

        let dtStart =
            System.DateTime.ParseExact(options.GetResult(DateStart, defaultValue = "01/01/20"), "MM/dd/yy", null)

        let dtEnd =
            System.DateTime.ParseExact(options.GetResult(DateEnd, defaultValue = "08/28/21"), "MM/dd/yy", null)

        let discount = options.GetResult(Discount, defaultValue = Bahncard.BC25)

        let count = options.GetResult(Take, defaultValue = System.Int32.MaxValue)

        Log.Debug <- options.Contains Debug
        Api.HafasAsyncClient.initSerializer ()

        use client = new Api.HafasAsyncClient(FsHafas.Profiles.Db.profile)

        let journeyInfo =
            journeydata file dtStart dtEnd
            |> takeChecked count
            |> Array.map (fun d ->
                async {
                    match! getJourneyInfo client d priceInNDays (LanguagePrimitives.EnumToValue discount) with
                    | Some j ->
                        printfn "journey: %s %s %A %.0f km, %.2f €" j.idFrom j.idTo j.dtStart j.km j.price
                        return Some j
                    | None ->
                        printfn "!!journey: %s %s %A" d.ifFrom d.idTo d.dtStart
                        return None
                }
                |> Async.RunSynchronously)

        let km =
            journeyInfo
            |> Array.choose id
            |> Array.sumBy (fun j -> j.km)

        let preis =
            journeyInfo
            |> Array.choose id
            |> Array.sumBy (fun j -> j.price)

        printfn
            "options: dtStart %A, dtEnd %A, priceInNDays %i, discount %A, count %i"
            dtStart
            dtEnd
            priceInNDays
            discount
            count

        printfn "journeys: %i, %.0f km, %.0f €" journeyInfo.Length km preis
    with
    | e -> printfn "%s" e.Message

    0
