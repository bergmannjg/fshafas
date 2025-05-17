namespace DbVendo.Parser

module internal StationBoard =

    open FsHafas.Client
    open DbVendo
    open DbVendo.Client

    let private findProduct (response: Raw.BahnhofstafelPosition) : Products.ProductTypeEx option =
        match response.produktGattung with
        | Some produktGattung -> Products.products |> Array.tryFind (fun p -> p.dbnav_short = produktGattung)
        | None -> None

    let private parseLine (response: Raw.BahnhofstafelPosition) : Line option =
        let product = findProduct response

        { Default.Line with
            matchId = response.mitteltext
            name = response.kurztext
            productName = response.kurztext
            mode = product |> Option.map (fun p -> p.mode)
            product = product |> Option.map (fun p -> p.id) }
        |> Some

    let private FromSomeU3StationStopLocation (u3: StationStopLocation option) =
        match u3 with
        | Some(StationStopLocation.Station s) -> StationStop.Station s |> Some
        | Some(StationStopLocation.Stop s) -> StationStop.Stop s |> Some
        | _ -> None

    let private parseBahnhofstafelPosition
        (isDeparture: bool)
        (response: Raw.BahnhofstafelPosition)
        : FsHafas.Client.Alternative =
        { tripId = Option.getD response.zuglaufId ""
          direction = response.richtung
          location = None
          line = parseLine response
          stop =
            response.abfrageOrt
            |> Option.map Location.parseLocation
            |> FromSomeU3StationStopLocation
          ``when`` =
            if isDeparture then
                response.abgangsDatum
            else
                response.ankunftsDatum
          plannedWhen =
            if isDeparture then
                Option.orElse response.abgangsDatum response.ezAbgangsDatum
            else
                Option.orElse response.ankunftsDatum response.ezAnkunftsDatum
          prognosedWhen = None
          delay = None
          platform = response.gleis
          plannedPlatform = response.ezGleis
          prognosedPlatform = None
          remarks = None
          cancelled = None
          loadFactor = None
          provenance = None
          previousStopovers = None
          nextStopovers = None
          currentTripPosition = None
          origin = (response.abgangsOrt |> Option.map Location.parseLocation)
          destination = None
          prognosisType = None }

    let parseStationBoard (response: Raw.StationBoardResponse) : Alternative array =
        match response.bahnhofstafelAbfahrtPositionen, response.bahnhofstafelAnkunftPositionen with
        | Some bahnhofstafelAbfahrtPositionen, _ ->
            bahnhofstafelAbfahrtPositionen
            |> Array.map (fun v -> parseBahnhofstafelPosition true v)
        | _, Some bahnhofstafelAnkunftPositionen ->
            bahnhofstafelAnkunftPositionen
            |> Array.map (fun v -> parseBahnhofstafelPosition false v)
        | _ -> [||]
