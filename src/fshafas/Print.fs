namespace FsHafas.Printf

/// <namespacedoc>
///   <summary>Print client types</summary>
/// </namespacedoc>

module Short =

    open FsHafas.Client

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

    let private printfnArrL<'a> (ident: int) (prefix: string) (arr: 'a [] option) =
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
        | _, Some _ ->
            printNameId ident location.address location.id
            + " "
        | _ -> ""
        + printLonLat ident location.longitude location.latitude
        + nl
        + printDistance (ident + 2) location.distance

    let private Stop (ident: int) (stop: Stop) =
        printNameId ident stop.name stop.id
        + match stop.location with
          | Some (location) ->
              " "
              + printLonLat ident location.longitude location.latitude
          | None -> ""
        + nl
        + printDistance (ident + 2) stop.distance

    let private Station (ident: int) (station: Station) =
        printNameId ident station.name station.id
        + nl
        + printDistance (ident + 2) station.distance

    let private Alternative (ident: int) (alternative: Alternative) =
        printfnS ident "direction: " alternative.direction
        + printfnS ident "when: " alternative.``when``
        + match alternative.stop with
          | Some stop ->
              match stop with
              | U2.Case2 s -> printfS ident "stop: " (Some "") + Stop 0 s
              | _ -> ""
          | _ -> ""

    let private Comment (ident: int) (s: string) =
        if s.Length > 0 then
            printfnS ident "---" (Some "")
        else
            ""

    let private Remark (ident: int) (remark: U3<Hint, Status, Warning>) =
        match remark with
        | U3.Case1 hint -> printfnS ident "hint: " (Some hint.text)
        | U3.Case2 status -> printfnS ident "status: " (Some status.text)
        | _ -> ""

    let private Warning (ident: int) (w: Warning) =
        printfnS ident "warning: " w.summary
        + printfnS (ident + 2) "validFrom: " w.validFrom
        + printfnS (ident + 2) "validUntil: " w.validUntil

    let private Remarks (ident: int) (remarks: array<U3<Hint, Status, Warning>> option) =
        match remarks with
        | Some remarks ->
            remarks
            |> Array.fold (fun s r -> s + Remark (ident + 2) r) ""
        | None -> ""

    let private ProductOfLeg (ident: int) (leg: Leg) =
        match leg.line with
        | Some line ->
            match line.product with
            | Some product -> printfnS ident "product: " (Some product)
            | None -> ""
        | None -> ""

    let StopOverStop (ident: int) (so: StopOver) =
        match so.stop with
        | Some (U2.Case2 s) -> printfS ident "" (Some "") + Stop 0 s
        | Some (U2.Case1 s) -> printfS ident "" (Some "") + Station 0 s
        | _ -> ""

    let StopOver (ident: int) (so: StopOver) =
        match so.stop with
        | Some (U2.Case2 s) -> printfS ident "origin: " (Some "") + Stop 0 s
        | Some (U2.Case1 s) -> printfS ident "origin: " (Some "") + Station 0 s
        | _ -> ""
        + match so.departure with
          | Some departure -> printfnS ident "departure: " so.departure
          | None -> printfnS ident "arrival: " so.arrival

    let private StopOvers (ident: int) (stopOvers: StopOver [] option) =
        match stopOvers with
        | Some stopOvers ->
            printfnS ident "stopOvers: " (Some "")
            + (stopOvers
               |> Array.fold
                   (fun s l ->
                       s
                       + (Comment (ident + 2) s)
                       + (StopOver (ident + 2) l))
                   "")
        | None -> ""

    let private Leg (ident: int) (leg: Leg) (short: bool) =
        printfnS ident "tripId: " leg.tripId
        + match leg.origin with
          | Some (U3.Case3 l) -> printfS ident "origin: " (Some "") + Location 0 l
          | Some (U3.Case2 s) -> printfS ident "origin: " (Some "") + Stop 0 s
          | Some (U3.Case1 s) -> printfS ident "origin: " (Some "") + Station 0 s
          | _ -> ""
        + match leg.destination with
          | Some (U3.Case2 s) -> printfS ident "destination: " (Some "") + Stop 0 s
          | _ -> ""
        + printfnS ident "departure: " leg.departure
        + printfnS ident "arrival: " leg.arrival
        + if short then
              ""
          else
              StopOvers ident leg.stopovers
        + printfnB ident "cancelled: " leg.cancelled
        + match leg.currentLocation with
          | Some (location) ->
              (String.replicate ident " ")
              + "currentLocation: "
              + printLonLat 0 location.longitude location.latitude
              + nl
          | None -> ""

        + if short then
              ProductOfLeg ident leg
              + printfnS ident "loadFactor: " leg.loadFactor
          else
              match leg.line with
              | Some (line) when line.name.IsSome -> printfnS ident "Line: " line.name
              | _ -> ""
              + printfnB ident "walking: " leg.walking
              + printfnB ident "transfer: " leg.transfer
              + ProductOfLeg ident leg
              + printfnS ident "loadFactor: " leg.loadFactor
              + Remarks ident leg.remarks

    let private Trip (ident: int) (trip: Trip) =
        match trip.origin with
        | Some (U3.Case2 s) -> printfS ident "origin: " (Some "") + Stop 0 s
        | Some (U3.Case1 s) -> printfS ident "origin: " (Some "") + Station 0 s
        | _ -> ""
        + match trip.destination with
          | Some (U3.Case2 s) -> printfS ident "destination: " (Some "") + Stop 0 s
          | _ -> ""
        + printfnS ident "departure: " trip.departure
        + printfnS ident "arrival: " trip.arrival
        + printfnArrL ident "stopovers: " trip.stopovers
        + match trip.line with
          | Some (line) when line.name.IsSome -> printfnS ident "Line: " line.name
          | _ -> ""
        + printfnB ident "cancelled: " trip.cancelled
        + printfnB ident "walking: " trip.walking
        + printfnB ident "transfer: " trip.transfer
        + Remarks ident trip.remarks

    let private Legs (ident: int) (legs: Leg []) (short: bool) =
        legs
        |> Array.fold
            (fun s l ->
                s
                + (Comment (ident + 2) s)
                + (Leg (ident + 2) l short))
            ""

    let JourneyLegs (ident: int) (journey: Journey) =
        printfnS ident "jouney:" (Some "")
        + Legs ident journey.legs true

    let Journey (ident: int) (journey: Journey) =
        let short = true

        let distS () =
            let distance = FsHafas.Parser.Journey.distanceOfJourney journey

            if distance > 0.0 then
                let identS = String.replicate (ident + 2) " "

                sprintf "%sdistance: %.2f" identS distance + nl
            else
                ""

        let price =
            match journey.price with
            | Some price ->
                let identS = String.replicate (ident + 2) " "

                sprintf "%sprice: %.2f %s" identS price.amount price.currency
                + nl
            | None -> ""

        printfnS ident "jouney:" (Some "")
        + Legs ident journey.legs short
        + price
        + distS ()
        + match short, journey.refreshToken with
          | false, Some refreshToken -> printfnS (ident + 2) "refreshToken: '" (Some(refreshToken + "'"))
          | _ -> ""

    let private JourneyItems (ident: int) (journeys: Journey []) =
        journeys
        |> Array.fold (fun s j -> s + Journey ident j) ""

    let Journeys (journeys: Journeys) =
        journeys.journeys
        |> Option.fold (fun s value -> s + JourneyItems 0 value) ""

    let private U2StationStop (ident: int) (location: U2<Station, Stop> option) =
        match location with
        | Some (U2.Case1 s) -> Station (ident + 2) s
        | Some (U2.Case2 s) -> Stop (ident + 2) s
        | _ -> ""

    let U3StationStopLocation (ident: int) (location: U3<Station, Stop, Location>) =
        match location with
        | U3.Case3 l -> Location (ident + 2) l
        | U3.Case2 s -> Stop (ident + 2) s
        | _ -> ""

    let Duration (ident: int) (duration: Duration) =
        printfnS
            ident
            "duration: "
            (duration.duration
             |> Option.map (fun d -> d.ToString()))
        + (duration.stations
           |> Array.fold (fun s j -> s + U3StationStopLocation (ident + 2) j) "")

    let private Directions (ident: int) (directions: string [] option) =
        match directions with
        | Some (directions) ->
            directions
            |> Array.fold (fun s j -> s + printfnS (ident + 2) "" (Some j)) ""
        | None -> ""

    let private Line (ident: int) (l: Line) =
        printfnS ident "name: " l.name
        + printfnArrL ident "directions: " l.directions
        + Directions (ident + 2) l.directions

    let private Movement (ident: int) (m: Movement) (withStopovers: bool) =
        printfnS ident "tripId: " m.tripId
        + printfnS ident "direction: " m.direction
        + match m.line with
          | Some (line) when line.name.IsSome -> printfnS ident "Line: " line.name
          | _ -> ""
        + (match m.location with
           | Some location -> printLonLat ident location.longitude location.latitude
           | None -> "")
        + (if withStopovers then
               printfnArrL ident "stopovers: " m.nextStopovers
               + StopOvers ident m.nextStopovers
           else
               "")

    let Locations (locations: U3<Station, Stop, Location> []) =
        locations
        |> Array.fold (fun s j -> s + U3StationStopLocation 0 j) ""

    let Durations (durations: Duration []) =
        durations
        |> Array.fold (fun s j -> s + Duration 0 j) ""

    let MovementsWithStopovers (durations: Movement []) =
        durations
        |> Array.fold (fun s j -> s + Movement 0 j true) ""

    let Movements (durations: Movement []) =
        durations
        |> Array.fold (fun s j -> s + Movement 0 j false) ""

    let Alternatives (alternatives: Alternative []) =
        alternatives
        |> Array.fold (fun s a -> s + Alternative 0 a) ""

    let Trips (trips: Trip []) =
        trips |> Array.fold (fun s t -> s + Trip 0 t) ""

    let Warnings (warnings: Warning []) =
        warnings
        |> Array.fold (fun s t -> s + Warning 0 t) ""

    let Lines (lines: Line []) =
        lines |> Array.fold (fun s t -> s + Line 0 t) ""

#if !FABLE_COMPILER

module Long =

    open System
    open FSharp.Reflection
    open FsHafas.Reflect

    let mutable private printSome = false
    let mutable private printNone = false
    let private ident (depth: int) = 2 * depth

    let private printRecordFieldname (depth: int) (name: string) (typ: Type) =
        fprintfn stdout "%s" (sprintf "%*s%s: (%s)" (ident depth) "" name (typ.ToString()))

    let private printCaseInfo (depth: int) (case: UnionCaseInfo) =
        if printSome || case.Name <> "Some" then
            fprintfn stdout "%s" (sprintf "%*s%s: (%s)" (ident depth) "" case.Name (case.DeclaringType.ToString()))

    let private printField (depth: int) (name: string) (o: obj) =
        if not (isNull o) then
            let typ = o.GetType()
            fprintfn stdout "%s" (sprintf "%*s%s: (%s) %A" (ident depth) "" name typ.Name o)
        else if printNone then
            fprintfn stdout "%s" (sprintf "%*s%s: %s" (ident depth) "" name "null")

    let private printMapField (depth: int) (name: string) (o: Map<string, bool>) =
        let v =
            o
            |> Seq.filter (fun kv -> kv.Value)
            |> Seq.map (fun kv -> kv.Key)
            |> String.concat ","

        fprintfn stdout "%s" (sprintf "%*s%s: (%s) %s" (ident depth) "" name (o.GetType().Name) v)

    let private printEmptyArray (depth: int) (name: string) =
        fprintfn stdout "%s" (sprintf "%*s%s: %s" (ident depth) "" name "[]")

    let private printUnkownType (depth: int) (t: Type) (name: string) (o: obj) =
        if name = "properties" then // todo
            ()
        else if name = "icon" then // todo
            ()
        else
            fprintfn stderr "%s" (sprintf "common: unkown type %s, field '%s', value '%A'" t.FullName name o)

    let private evt: Traverse.TraverseEvent =
        { onRecordFieldname = printRecordFieldname
          onCaseInfo = printCaseInfo
          onField = printField
          onUnkownType = printUnkownType
          onEmptyArray = printEmptyArray
          onMapField = printMapField }

    let print (o: obj) = Traverse.traverse evt o

#endif
