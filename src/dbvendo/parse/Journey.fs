namespace DbVendo.Parser

module internal Journey =

    open FsHafas.Client
    open DbVendo
    open DbVendo.Client

    let private findProduct (response: Raw.VerbindungsAbschnitt) : Products.ProductTypeEx option =
        match response.produktGattung with
        | Some produktGattung -> Products.products |> Array.tryFind (fun p -> p.dbnav_short = produktGattung)
        | None -> None

    let private parseLine (response: Raw.VerbindungsAbschnitt) : Line option =
        let product = findProduct response

        { Default.Line with
            matchId = response.risZuglaufId
            name = response.langtext
            fahrtNr = response.verkehrsmittelNummer
            productName = response.kurztext
            mode = product |> Option.map (fun p -> p.mode)
            product = product |> Option.map (fun p -> p.id) }
        |> Some

    let private parseOrt (response: Raw.Location) : StationStop =
        match Location.parseLocation response with
        | StationStopLocation.Stop s -> StationStop.Stop s
        | _ -> failwith "parseOrt: stop expected"

    let private parseHalt (response: Raw.Halt) : StopOver =
        { Default.StopOver with
            stop = response.ort |> Option.map parseOrt
            departure = response.abgangsDatum
            plannedDeparture = Option.orElse response.abgangsDatum response.ezAbgangsDatum
            departurePlatform = response.plattform
            arrival = response.ankunftsDatum
            plannedArrival = Option.orElse response.ankunftsDatum response.ezAnkunftsDatum }

    let private echtzeitNotizenContains (response: Raw.Halt array) (text: string) : bool =
        response
        |> Array.exists (fun halt -> halt.echtzeitNotizen |> Array.exists (fun m -> Option.contains text m.text))

    let private isCancelled (response: Raw.Halt array option) : bool option =
        match response with
        | Some response ->
            if (echtzeitNotizenContains response "Stop cancelled") then
                Some true
            else
                None
        | None -> None

    let private parsePolylineDesc (response: Raw.PolylineDesc) : Feature array =
        response.coordinates
        |> Array.map (fun c ->
            { ``type`` = FeatureType.Feature
              properties = None
              geometry =
                { ``type`` = GeometryType.Point
                  coordinates = [| c.latitude; c.longitude |] } })

    let private parsePolyline (response: Raw.PolylineGroup) : FeatureCollection option =
        match response.polylineDesc with
        | [| polylineDesc |] ->
            { ``type`` = FeatureCollection
              features = parsePolylineDesc polylineDesc }
            |> Some
        | _ -> None

    let firstOf (arr: ('a array) option) : 'a option =
        match arr with
        | Some arr when arr.Length > 0 -> Some arr.[0]
        | _ -> None

    let laststOf (arr: ('a array) option) : 'a option =
        match arr with
        | Some arr when arr.Length > 0 -> Some arr.[arr.Length - 1]
        | _ -> None

    let locationOfStopover (s: StopOver option) : StationStopLocation option =
        match s with
        | Some s ->
            match s.stop with
            | Some(StationStop.Stop s) -> Some(StationStopLocation.Stop s)
            | _ -> None
        | None -> None

    let departureOfStopover (s: StopOver option) : string option =
        match s with
        | Some s -> s.departure
        | None -> None

    let arrivalOfStopover (s: StopOver option) : string option =
        match s with
        | Some s -> s.arrival
        | None -> None

    let parseVerbindungsAbschnitt (response: Raw.VerbindungsAbschnitt) : Leg =
        let stopovers =
            Option.getD response.halte [||] |> Array.map (fun h -> parseHalt h) |> Some

        { Default.Leg with
            tripId = response.zuglaufId
            line = parseLine response
            origin =
                Option.orElseWith
                    (fun () -> locationOfStopover (firstOf stopovers))
                    (response.abgangsOrt |> Option.map Location.parseLocation)
            departure = Option.orElseWith (fun () -> departureOfStopover (firstOf stopovers)) response.abgangsDatum
            plannedDeparture = Option.orElse response.abgangsDatum response.ezAbgangsDatum
            destination =
                Option.orElseWith
                    (fun () -> locationOfStopover (laststOf stopovers))
                    (response.ankunftsOrt |> Option.map Location.parseLocation)
            arrival = Option.orElseWith (fun () -> arrivalOfStopover (laststOf stopovers)) response.ankunftsDatum
            plannedArrival = Option.orElse response.ankunftsDatum response.ezAnkunftsDatum
            stopovers = stopovers
            direction = response.richtung
            walking = Option.equals response.typ "FUSSWEG"
            cancelled = isCancelled response.halte
            polyline = response.polylineGroup |> Option.bind parsePolyline }

    let private trimJourneyId (kontext: string option) : string option =
        match kontext with
        | Some kontext when kontext.StartsWith "¶HKI¶" ->
            match kontext.IndexOf('¶', 5) with
            | -1 -> Some kontext
            | index -> Some(kontext.Substring(0, index - 1))
        | _ -> None

    let private emptyJourneysOptions: JourneysOptions =
        { departure = None
          arrival = None
          earlierThan = None
          laterThan = None
          results = None
          via = None
          stopovers = None
          transfers = None
          transferTime = None
          accessibility = None
          bike = None
          products = None
          tickets = None
          polylines = None
          subStops = None
          entrances = None
          remarks = None
          walkingSpeed = None
          startWithWalking = None
          language = None
          scheduledDays = None
          firstClass = None
          ageGroup = None
          age = None
          loyaltyCard = None
          routingMode = None
          ``when`` = None
          generateUnreliableTicketUrls = None }

    let private hasPriceOptions (opt: JourneysOptions) : bool =
        match opt.firstClass with
        | Some true -> true
        | _ -> false
        || opt.ageGroup.IsSome
        || opt.age.IsSome
        || opt.loyaltyCard.IsSome

    let private addPriceOptions (opt: JourneysOptions) : JourneysOptions =
        { emptyJourneysOptions with
            tickets = opt.tickets
            firstClass = opt.firstClass
            ageGroup = opt.ageGroup
            age = opt.age
            loyaltyCard = opt.loyaltyCard }

    // PriceOptions are used in Format.refreshJourneysRequest
    let private addPriceOptionsToJourneyId (opt: JourneysOptions option) (kontext: string option) : string option =
        match opt, kontext with
        | Some opt, Some kontext when opt.tickets = Some true && hasPriceOptions opt ->
            Some(
                Api.Serializer.Serialize<RefreshTokenWithJourneysOptions>
                    { refreshToken = kontext
                      journeysOptions = addPriceOptions opt }
            )
        | _ -> kontext

    let parsePrice (response: Raw.Angebote option) : Price option =
        match Option.getValue response (fun v -> v.preise) with
        | Some preise ->
            match preise.gesamt with
            | Some gesamt ->
                Some
                    { amount = gesamt.ab.betrag
                      currency = Some gesamt.ab.waehrung
                      hint = None }
            | None -> None
        | None -> None

    let parseJourney (opt: JourneysOptions option) (response: Raw.VerbindungEx) : Journey =
        { Default.Journey with
            legs =
                Option.getD response.verbindung.verbindungsAbschnitte [||]
                |> Array.map parseVerbindungsAbschnitt
            price = parsePrice response.angebote
            refreshToken = addPriceOptionsToJourneyId opt (trimJourneyId response.verbindung.kontext) }

    let parseJourneys (opt: JourneysOptions option) (response: Raw.JourneysResponse) : Journeys =
        { Default.Journeys with
            journeys = response.verbindungen |> Array.map (parseJourney opt) |> Some }
