namespace FsHafas.Printf

/// <namespacedoc>
///   <summary>Print client types</summary>
/// </namespacedoc>

module Short =

    open System
    open FsHafas.Client

    let private ``calculate distance`` (p1Latitude, p1Longitude) (p2Latitude, p2Longitude) =
        let r = 6371.0 // km

        let dLat = (p2Latitude - p1Latitude) * Math.PI / 180.0

        let dLon = (p2Longitude - p1Longitude) * Math.PI / 180.0

        let lat1 = p1Latitude * Math.PI / 180.0
        let lat2 = p2Latitude * Math.PI / 180.0

        let a =
            Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0)
            + Math.Sin(dLon / 2.0) * Math.Sin(dLon / 2.0) * Math.Cos(lat1) * Math.Cos(lat2)

        let c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a))

        r * c

    let private distanceOfFeatureCollection (fc: FsHafas.Client.FeatureCollection) =
        let latLonPoints =
            fc.features
            |> Array.map (fun f -> (f.geometry.coordinates.[1], f.geometry.coordinates.[0]))

        latLonPoints
        |> Array.mapi (fun i _ ->
            if i > 0 then
                let prev = latLonPoints.[i - 1]
                let curr = latLonPoints.[i]
                ``calculate distance`` prev curr
            else
                0.0)
        |> Array.sum

    let distanceOfJourney (j: Journey) =
        j.legs
        |> Array.choose (fun l -> l.polyline)
        |> Array.sumBy distanceOfFeatureCollection

    let private nl = "\n"

    let private printfS (ident: int) (prefix: string) (s: string option) =
        let identS = String.replicate ident " "

        s
        |> Option.fold
            (fun s value ->
                s
                + if prefix = "(" then
                      sprintf "%s%s%s%s" identS prefix value ")"
                  else
                      sprintf "%s%s%s" identS prefix value)
            ""

    let private printfnS (ident: int) (prefix: string) (s: string option) = printfS ident prefix s + nl

    let private printfnArrL<'a> (ident: int) (prefix: string) (arr: 'a[] option) =
        let identS = String.replicate ident " "

        match arr with
        | Some value when value.Length > 0 -> sprintf "%s%s%d" identS prefix value.Length + nl
        | _ -> ""

    let private printfnB (ident: int) (prefix: string) (b: bool option) =
        let identS = String.replicate ident " "

        match b with
        | Some value when value -> sprintf "%s%s%b" identS prefix value + nl
        | _ -> ""

    let private printDistance (ident: int) (d: int option) =
        let identS = String.replicate ident " "

        match d with
        | Some d when d > 0 ->
            let km = (d |> float) / 1000.0
            sprintf "%s%s%0.3f" identS "distance: " km + nl
        | _ -> ""

    let private printLonLat (ident: int) (lon: float option) (lat: float option) =
        let identS = String.replicate ident " "

        match lon, lat with
        | Some lon, Some lat -> sprintf "%s%s%f,%f" identS "lonlat: " lon lat
        | _ -> ""

    let private printNameId (ident: int) (name: string option) (id: string option) =
        printfS ident "" name + printfS ident "(" id

    let private Location (ident: int) (location: Location) =
        match location.name, location.address with
        | Some _, _ -> printNameId ident location.name location.id + " "
        | _, Some _ -> printNameId ident location.address location.id + " "
        | _ -> ""
        + printLonLat ident location.longitude location.latitude
        + nl
        + printDistance (ident + 2) location.distance

    let private Stop (ident: int) (stop: Stop) =
        printNameId ident stop.name stop.id
        + match stop.location with
          | Some(location) -> " " + printLonLat ident location.longitude location.latitude
          | None -> ""
        + nl
        + printDistance (ident + 2) stop.distance

    let private Station (ident: int) (station: Station) =
        printNameId ident station.name station.id
        + nl
        + printDistance (ident + 2) station.distance

    let private ProductOfLine (ident: int) (line: Line option) =
        match line with
        | Some line ->
            match line.product, line.name, line.matchId with
            | Some product, Some name, Some matchId ->
                printfnS ident "product: " (Some(product + ", '" + name + "', linenumber " + matchId))
            | _ -> ""
        | None -> ""

    let private Alternative (ident: int) (alternative: Alternative) =
        printfnS ident "direction: " alternative.direction
        + match alternative.origin with
          | Some stop ->
              match stop with
              | StationStopLocation.Stop s -> printfS ident "origin: " (Some "") + Stop 0 s
              | StationStopLocation.Location l -> printfS ident "origin: " (Some "") + Location 0 l
              | _ -> ""
          | _ -> ""
        + printfnS ident "when: " alternative.``when``
        + ProductOfLine ident alternative.line
        + match alternative.stop with
          | Some stop ->
              match stop with
              | StationStop.Stop s -> printfS ident "stop: " (Some "") + Stop 0 s
              | _ -> ""
          | _ -> ""

    let private Comment (ident: int) (s: string) =
        if s.Length > 0 then printfnS ident "---" (Some "") else ""

    let private Remark (ident: int) (remark: HintStatusWarning) =
        match remark with
        | HintStatusWarning.Hint hint -> printfnS ident "hint: " (Some hint.text)
        | HintStatusWarning.Status status -> printfnS ident "status: " (Some status.text)
        | _ -> ""

    let private Warning (ident: int) (w: Warning) =
        printfnS ident "warning: " w.summary
        + printfnS (ident + 2) "validFrom: " w.validFrom
        + printfnS (ident + 2) "validUntil: " w.validUntil

    let private Remarks (ident: int) (remarks: array<HintStatusWarning> option) =
        match remarks with
        | Some remarks -> remarks |> Array.fold (fun s r -> s + Remark (ident + 2) r) ""
        | None -> ""

    let StopOverStop (ident: int) (so: StopOver) =
        match so.stop with
        | Some(StationStop.Stop s) -> printfS ident "" (Some "") + Stop 0 s
        | Some(StationStop.Station s) -> printfS ident "" (Some "") + Station 0 s
        | _ -> ""

    let StopOver (ident: int) (so: StopOver) =
        match so.stop with
        | Some(StationStop.Stop s) -> printfS ident "origin: " (Some "") + Stop 0 s
        | Some(StationStop.Station s) -> printfS ident "origin: " (Some "") + Station 0 s
        | _ -> ""
        + match so.departure with
          | Some _ -> printfnS ident "departure: " so.departure
          | None ->
              match so.arrival with
              | Some _ -> printfnS ident "arrival: " so.arrival
              | None -> ""
        + match so.additional with
          | Some additional when additional -> printfnS ident "additional: " (Some "true")
          | _ -> ""
        + match so.cancelled with
          | Some cancelled when cancelled -> printfnS ident "cancelled: " (Some "true")
          | _ -> ""

    let private StopOvers (ident: int) (stopOvers: StopOver[] option) =
        match stopOvers with
        | Some stopOvers ->
            printfnS ident "stopOvers: " (Some "")
            + (stopOvers
               |> Array.fold (fun s l -> s + (Comment (ident + 2) s) + (StopOver (ident + 2) l)) "")
        | None -> ""

    let private Leg (ident: int) (leg: Leg) (short: bool) =
        printfnS ident "tripId: " leg.tripId
        + match leg.origin with
          | Some(StationStopLocation.Location l) -> printfS ident "origin: " (Some "") + Location 0 l
          | Some(StationStopLocation.Stop s) -> printfS ident "origin: " (Some "") + Stop 0 s
          | Some(StationStopLocation.Station s) -> printfS ident "origin: " (Some "") + Station 0 s
          | _ -> ""
        + match leg.destination with
          | Some(StationStopLocation.Stop s) -> printfS ident "destination: " (Some "") + Stop 0 s
          | _ -> ""
        + printfnS ident "departure: " leg.departure
        + printfnS ident "plannedDeparture: " leg.plannedDeparture
        + printfnS ident "arrival: " leg.arrival
        + printfnS ident "plannedArrival: " leg.plannedArrival
        + if not short then "" else StopOvers ident leg.stopovers
        + printfnB ident "cancelled: " leg.cancelled
        + match leg.currentLocation with
          | Some(location) ->
              (String.replicate ident " ")
              + "currentLocation: "
              + printLonLat 0 location.longitude location.latitude
              + nl
          | None -> ""

        + printfnB ident "walking: " leg.walking
        + if short then
              ProductOfLine ident leg.line + printfnS ident "loadFactor: " leg.loadFactor
          else
              match leg.line with
              | Some(line) when line.name.IsSome -> printfnS ident "Line: " line.name
              | _ -> ""
              + printfnB ident "walking: " leg.walking
              + printfnB ident "transfer: " leg.transfer
              + ProductOfLine ident leg.line
              + printfnS ident "loadFactor: " leg.loadFactor
              + Remarks ident leg.remarks

    let private scheduledDaysIdent
        (ident: int)
        (scheduledDaysSummary: string option)
        (scheduledDays: ScheduledDays option)
        =
        match scheduledDaysSummary, scheduledDays with
        | Some scheduledDaysSummary, _ -> printfnS ident "scheduledDays: " (Some scheduledDaysSummary)
        | _, Some scheduledDays when scheduledDays.Keys.Length > 0 ->
            let keys = scheduledDays.Keys

            match
                keys |> Array.tryFind (fun k -> scheduledDays.[k]),
                keys |> Array.tryFindBack (fun k -> scheduledDays.[k])
            with
            | Some first, Some last -> printfnS ident "scheduledDays: " (Some(first + " " + last))
            | _ -> ""
        | _ -> ""

    let Trip (trip: Trip) =
        let ident = 2

        printfnS 0 "trip:" (Some "")
        + printfnS ident "id: " (Some trip.id)
        + match trip.origin with
          | Some(StationStopLocation.Stop s) -> printfS ident "origin: " (Some "") + Stop 0 s
          | Some(StationStopLocation.Station s) -> printfS ident "origin: " (Some "") + Station 0 s
          | _ -> ""
        + match trip.destination with
          | Some(StationStopLocation.Stop s) -> printfS ident "destination: " (Some "") + Stop 0 s
          | _ -> ""
        + printfnS ident "departure: " trip.departure
        + printfnS ident "arrival: " trip.arrival
        + printfnArrL ident "stopovers: " trip.stopovers
        + StopOvers ident trip.stopovers
        + scheduledDaysIdent ident None trip.scheduledDays
        + match trip.line with
          | Some(line) -> printfS ident "Line: " line.name + printfnS 0 ", linenumber: " line.matchId
          | _ -> ""
        + match trip.currentLocation with
          | Some(location) ->
              (String.replicate ident " ")
              + "currentLocation: "
              + printLonLat 0 location.longitude location.latitude
              + nl
          | None -> ""

        + printfnB ident "cancelled: " trip.cancelled
        + printfnB ident "walking: " trip.walking
        + printfnB ident "transfer: " trip.transfer
        + Remarks ident trip.remarks

    let private Legs (ident: int) (legs: Leg[]) (short: bool) =
        legs
        |> Array.fold (fun s l -> s + (Comment (ident + 2) s) + (Leg (ident + 2) l short)) ""

    let JourneyLegs (ident: int) (journey: Journey) =
        printfnS ident "jouney:" (Some "") + Legs ident journey.legs true

    let ScheduledDays (scheduledDays: ScheduledDays option) = scheduledDaysIdent 0 None scheduledDays

    let Journey (ident: int) (journey: Journey) =
        let short = true

        let distS () =
            let distance = distanceOfJourney journey

            if distance > 0.0 then
                let identS = String.replicate (ident + 2) " "

                sprintf "%sdistance: %.2f" identS distance + nl
            else
                ""

        let price =
            match journey.tickets with
            | Some tickets ->
                tickets
                |> Array.fold
                    (fun s t ->
                        let ticket =
                            match t.priceObj with
                            | Some price ->
                                let identS = String.replicate (ident + 2) " "

                                sprintf
                                    "%sprice: %.2f %s %s"
                                    identS
                                    (System.Math.Round(price.amount / 100.0, 2))
                                    "EUR"
                                    t.name
                                + nl
                            | _ -> ""

                        s + ticket)
                    ""
            | _ ->
                match journey.price with
                | Some price ->
                    let identS = String.replicate (ident + 2) " "

                    sprintf "%sprice: %.2f %s" identS price.amount (price.currency |> Option.defaultValue "")
                    + nl
                | None -> ""

        printfnS ident "jouney:" (Some "")
        + Legs ident journey.legs short
        + price
        + distS ()
        + scheduledDaysIdent (ident + 2) None journey.scheduledDays
        + match journey.refreshToken with
          | Some refreshToken -> printfnS (ident + 2) "refreshToken: '" (Some(refreshToken + "'"))
          | None -> ""

    let Journeys (journeys: Journeys) =
        match journeys.journeys with
        | Some journeys -> journeys |> Array.fold (fun s j -> s + Journey 0 j) ""
        | None -> ""

    let private U2StationStop (ident: int) (location: U2<Station, Stop> option) =
        match location with
        | Some(U2.Case1 s) -> Station (ident + 2) s
        | Some(U2.Case2 s) -> Stop (ident + 2) s
        | _ -> ""

    let U3StationStopLocation (ident: int) (location: StationStopLocation) =
        match location with
        | StationStopLocation.Location l -> Location (ident + 2) l
        | StationStopLocation.Stop s -> Stop (ident + 2) s
        | StationStopLocation.Station s -> Station (ident + 2) s

    let Duration (ident: int) (duration: Duration) =
        printfnS ident "duration: " (Some(duration.duration.ToString()))
        + (duration.stations
           |> Array.fold (fun s j -> s + U3StationStopLocation (ident + 2) j) "")

    let private Directions (ident: int) (directions: string[] option) =
        match directions with
        | Some(directions) -> directions |> Array.fold (fun s j -> s + printfnS (ident + 2) "" (Some j)) ""
        | None -> ""

    let private Line (ident: int) (l: Line) =
        printfnS ident "name: " l.name
        + printfnArrL ident "directions: " l.directions
        + Directions (ident + 2) l.directions

    let private Movement (ident: int) (m: Movement) (withStopovers: bool) =
        printfnS ident "tripId: " m.tripId
        + printfnS ident "direction: " m.direction
        + match m.line with
          | Some(line) when line.name.IsSome -> printfnS ident "Line: " line.name
          | _ -> ""
        + (match m.location with
           | Some location -> printLonLat ident location.longitude location.latitude
           | None -> "")
        + (if withStopovers then
               printfnArrL ident "stopovers: " m.nextStopovers
               + StopOvers ident m.nextStopovers
           else
               "")

    let Locations (locations: StationStopLocation[]) =
        locations |> Array.fold (fun s j -> s + U3StationStopLocation 0 j) ""

    let Durations (durations: Duration[]) =
        durations |> Array.fold (fun s j -> s + Duration 0 j) ""

    let MovementsWithStopovers (durations: Movement[]) =
        durations |> Array.fold (fun s j -> s + Movement 0 j true) ""

    let Movements (durations: Movement[]) =
        durations |> Array.fold (fun s j -> s + Movement 0 j false) ""

    let Alternatives (alternatives: Alternative[]) =
        alternatives |> Array.fold (fun s a -> s + Alternative 0 a) ""

    let Trips (trips: Trip[]) =
        trips |> Array.fold (fun s t -> s + Trip t) ""

    let Warnings (warnings: Warning[]) =
        warnings |> Array.fold (fun s t -> s + Warning 0 t) ""

    let Lines (lines: Line[]) =
        lines |> Array.fold (fun s t -> s + Line 0 t) ""
