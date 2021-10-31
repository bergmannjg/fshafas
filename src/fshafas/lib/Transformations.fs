namespace FsHafas.Client

module internal Coordinate =

    let toFloat (v: int) = float (v) / 1000000.0

    let fromFloat (x: float) = System.Math.Round(x * 1000000.0) |> int

module internal RawDep =

    open FsHafas.Client

    let FromRawStopL (s: FsHafas.Raw.RawStop) : FsHafas.Raw.RawDep =
        { locX = Some s.locX
          idx = s.idx
          dProdX = s.dProdX
          dPlatfS = s.dPlatfS
          dInR = s.dInR
          dTimeS = s.dTimeS
          dProgType = s.dProgType
          dTrnCmpSX = None
          dTZOffset = s.dTZOffset
          ``type`` = None
          dTimeR = s.dTimeR
          dCncl = s.dCncl
          dPltfS = s.dPltfS
          dPlatfR = s.dPlatfR
          dPltfR = s.dPltfR }

module internal RawArr =

    open FsHafas.Client

    let FromRawStopL (s: FsHafas.Raw.RawStop) : FsHafas.Raw.RawArr =
        { locX = Some s.locX
          idx = s.idx
          aPlatfS = s.aPlatfS
          aOutR = s.aOutR
          aTimeS = s.aTimeS
          aProgType = s.aProgType
          aTZOffset = s.aTZOffset
          ``type`` = None
          aTimeR = s.aTimeR
          aCncl = s.aCncl
          aPltfS = s.aPltfS
          aPlatfR = s.aPlatfR
          aPltfR = s.aPltfR }


module internal ToTrip =

    open FsHafas.Client

    let FromLeg (id: string) (l: FsHafas.Client.Leg) : FsHafas.Client.Trip =
        { id = id
          origin = l.origin
          destination = l.destination
          departure = l.departure
          plannedDeparture = l.plannedDeparture
          prognosedArrival = l.prognosedArrival
          departureDelay = l.departureDelay
          departurePlatform = l.departurePlatform
          prognosedDeparturePlatform = l.prognosedDeparturePlatform
          plannedDeparturePlatform = l.plannedDeparturePlatform
          arrival = l.arrival
          plannedArrival = l.plannedArrival
          prognosedDeparture = l.prognosedDeparture
          arrivalDelay = l.arrivalDelay
          arrivalPlatform = l.arrivalPlatform
          prognosedArrivalPlatform = l.prognosedArrivalPlatform
          plannedArrivalPlatform = l.plannedArrivalPlatform
          stopovers = l.stopovers
          schedule = l.schedule
          price = l.price
          operator = l.operator
          direction = l.direction
          line = l.line
          reachable = l.reachable
          cancelled = l.cancelled
          walking = l.walking
          loadFactor = l.loadFactor
          distance = l.distance
          ``public`` = l.``public``
          transfer = l.transfer
          cycle = l.cycle
          alternatives = l.alternatives
          polyline = l.polyline
          remarks = l.remarks }

module internal U2StationStop =

#if FABLE_COMPILER
    open Fable.Core
#endif

    open FsHafas.Client
    open FsHafas.Client

    let FromU3StationStopLocation (u3: U3<Station, Stop, Location>) =
        match u3 with
        | U3.Case1 s ->
            U2<FsHafas.Client.Station, FsHafas.Client.Stop>.Case1 s
            |> Some
        | U3.Case2 s ->
            U2<FsHafas.Client.Station, FsHafas.Client.Stop>.Case2 s
            |> Some
        | _ -> None

    let FromSomeU3StationStopLocation (u3: U3<Station, Stop, Location> option) =
        match u3 with
        | Some (U3.Case1 s) ->
            U2<FsHafas.Client.Station, FsHafas.Client.Stop>.Case1 s
            |> Some
        | Some (U3.Case2 s) ->
            U2<FsHafas.Client.Station, FsHafas.Client.Stop>.Case2 s
            |> Some
        | _ -> None

module internal U2StopLocation =

#if FABLE_COMPILER
    open Fable.Core
#endif

    open FsHafas.Client

    let FromU3StationStopLocation (u3: U3<Station, Stop, Location>) =
        match u3 with
        | U3.Case2 s ->
            U2<FsHafas.Client.Stop, FsHafas.Client.Location>.Case1 s
            |> Some
        | U3.Case3 s ->
            U2<FsHafas.Client.Stop, FsHafas.Client.Location>.Case2 s
            |> Some
        | _ -> None

    let FromSomeU3StationStopLocation (u3: U3<Station, Stop, Location> option) =
        match u3 with
        | Some (U3.Case2 s) ->
            U2<FsHafas.Client.Stop, FsHafas.Client.Location>.Case1 s
            |> Some
        | Some (U3.Case3 s) ->
            U2<FsHafas.Client.Stop, FsHafas.Client.Location>.Case2 s
            |> Some
        | _ -> None

module internal MergeOptions =

    open FsHafas.Client
    open FsHafas.Endpoint

    let private getOptionValue<'a, 'b> (opt: 'a option) (getter: 'a -> 'b option) (defaultValue: 'b) =
        match opt with
        | Some (value) ->
            match getter value with
            | Some result -> result
            | None -> defaultValue
        | None -> defaultValue

    let JourneysOptions (options: FsHafas.Endpoint.Options) (opt: JourneysOptions option) =
        { options with
              stopovers = getOptionValue opt (fun v -> v.stopovers) options.stopovers
              scheduledDays = getOptionValue opt (fun v -> v.scheduledDays) options.scheduledDays
              firstClass = getOptionValue opt (fun v -> v.firstClass) options.firstClass }

    let JourneysFromTripOptions (options: FsHafas.Endpoint.Options) (opt: JourneysFromTripOptions option) =
        { options with
              stopovers = getOptionValue opt (fun v -> v.stopovers) options.stopovers }

    let LocationsOptions (options: FsHafas.Endpoint.Options) (opt: LocationsOptions option) =
        { options with
              linesOfStops = getOptionValue opt (fun v -> v.linesOfStops) options.linesOfStops }

    let NearByOptions (options: FsHafas.Endpoint.Options) (opt: NearByOptions option) =
        { options with
              linesOfStops = getOptionValue opt (fun v -> v.linesOfStops) options.linesOfStops }

module Default =

    open FsHafas.Client

    let TripOptions: TripOptions =
        { stopovers = Some true
          polyline = Some true
          subStops = Some true
          entrances = Some true
          remarks = Some true
          language = Some "de" }

    let LinesOptions: LinesOptions = { language = Some "de" }

    let RemarksOptions: RemarksOptions =
        { from = Some System.DateTime.Now
          ``to`` = None
          results = Some 100
          products = Some(Products(false))
          polylines = Some false
          language = Some "de" }

    let LocationsOptions: LocationsOptions =
        { fuzzy = Some true
          results = Some 5
          stops = Some true
          addresses = Some true
          poi = Some true
          subStops = Some true
          entrances = Some true
          linesOfStops = Some false
          language = Some "de" }

    let RefreshJourneyOptions: RefreshJourneyOptions =
        { stopovers = Some true
          polylines = Some false
          tickets = Some false
          subStops = None
          entrances = None
          remarks = None
          language = Some "de" }

    let TripsByNameOptions: TripsByNameOptions = { ``when`` = Some System.DateTime.Now }

    let NearByOptions: NearByOptions =
        { results = Some 8
          distance = Some -1
          poi = None
          stops = Some true
          products = Some(Products(false))
          subStops = None
          entrances = None
          linesOfStops = None
          language = Some "de" }

    let JourneysOptions: JourneysOptions =
        { departure = Some System.DateTime.Now
          arrival = None
          earlierThan = None
          laterThan = None
          results = Some 3
          via = None
          stopovers = Some true
          transfers = Some -1
          transferTime = Some 0
          accessibility = Some "none"
          bike = Some false
          products = Some(Products(false))
          tickets = Some false
          polylines = Some false
          subStops = Some true
          entrances = Some true
          remarks = Some true
          walkingSpeed = Some "normal"
          startWithWalking = Some true
          language = Some "de"
          scheduledDays = Some false
          firstClass = Some false
          loyaltyCard = None
          ``when`` = None }

    let JourneysFromTripOptions: JourneysFromTripOptions =
        { stopovers = Some false
          transferTime = Some 0
          accessibility = Some "none"
          tickets = Some false
          polylines = Some false
          subStops = Some true
          entrances = Some true
          remarks = Some true
          products = Some(Products(false)) }

    let DeparturesArrivalsOptions: DeparturesArrivalsOptions =
        { ``when`` = Some System.DateTime.Now
          direction = None
          line = None
          duration = Some 10
          results = None
          subStops = None
          entrances = None
          linesOfStops = None
          remarks = None
          stopovers = Some false
          includeRelatedStations = Some false
          products = Some(Products(false))
          language = Some "de" }

    let StopOptions: StopOptions =
        { linesOfStops = None
          subStops = None
          entrances = None
          remarks = None
          language = None }

    let ReachableFromOptions: ReachableFromOptions =
        { ``when`` = Some System.DateTime.Now
          maxTransfers = Some 5
          maxDuration = Some 20
          products = Some(Products(false))
          subStops = None
          entrances = None
          polylines = None }

    let RadarOptions: RadarOptions =
        { results = Some 256
          frames = Some 3
          products = Some(Products(false))
          duration = Some 30
          subStops = Some true
          entrances = Some true
          polylines = Some true
          ``when`` = Some System.DateTime.Now }

    let ServerOptions: ServerOptions = { language = Some "de" }

    let Location: Location =
        { ``type`` = Some "location"
          id = None
          name = None
          poi = None
          address = None
          longitude = None
          latitude = None
          altitude = None
          distance = None }

    let Stop: Stop =
        { ``type`` = Some "stop"
          id = None
          name = None
          location = None
          station = None
          products = None
          lines = None
          isMeta = None
          reisezentrumOpeningHours = None
          ids = None
          loadFactor = None
          entrances = None
          transitAuthority = None
          distance = None }

    let Station: Station =
        { ``type`` = Some "station"
          id = None
          name = None
          station = None
          location = None
          products = None
          isMeta = None
          regions = None
          lines = None
          facilities = None
          reisezentrumOpeningHours = None
          stops = None
          entrances = None
          transitAuthority = None
          distance = None }

    let Leg: Leg =
        { tripId = None
          origin = None
          destination = None
          departure = None
          plannedDeparture = None
          prognosedArrival = None
          departureDelay = None
          departurePlatform = None
          prognosedDeparturePlatform = None
          plannedDeparturePlatform = None
          arrival = None
          plannedArrival = None
          prognosedDeparture = None
          arrivalDelay = None
          arrivalPlatform = None
          prognosedArrivalPlatform = None
          plannedArrivalPlatform = None
          stopovers = None
          schedule = None
          price = None
          operator = None
          direction = None
          line = None
          reachable = None
          cancelled = None
          walking = None
          loadFactor = None
          distance = None
          ``public`` = None
          transfer = None
          cycle = None
          alternatives = None
          polyline = None
          remarks = None }

    let Trip: Trip =
        { id = ""
          origin = None
          destination = None
          departure = None
          plannedDeparture = None
          prognosedArrival = None
          departureDelay = None
          departurePlatform = None
          prognosedDeparturePlatform = None
          plannedDeparturePlatform = None
          arrival = None
          plannedArrival = None
          prognosedDeparture = None
          arrivalDelay = None
          arrivalPlatform = None
          prognosedArrivalPlatform = None
          plannedArrivalPlatform = None
          stopovers = None
          schedule = None
          price = None
          operator = None
          direction = None
          line = None
          reachable = None
          cancelled = None
          walking = None
          loadFactor = None
          distance = None
          ``public`` = None
          transfer = None
          cycle = None
          alternatives = None
          polyline = None
          remarks = None }

    let Movement: Movement =
        { direction = None
          tripId = None
          line = None
          location = None
          nextStopovers = None
          frames = None
          polyline = None }

    let Alternative: FsHafas.Client.Alternative =
        { tripId = ""
          direction = None
          location = None
          line = None
          stop = None
          ``when`` = None
          plannedWhen = None
          prognosedWhen = None
          delay = None
          platform = None
          plannedPlatform = None
          prognosedPlatform = None
          remarks = None
          cancelled = None
          loadFactor = None
          provenance = None
          previousStopovers = None
          nextStopovers = None
          frames = None
          polyline = None }

    let Journey: Journey =
        { ``type`` = Some "journey"
          legs = [||]
          refreshToken = None
          remarks = None
          price = None
          cycle = None
          scheduledDays = None }

    let Journeys: Journeys =
        { earlierRef = None
          laterRef = None
          journeys = None
          realtimeDataFrom = None }

    let Line: FsHafas.Client.Line =
        { ``type`` = Some "line"
          id = None
          name = None
          adminCode = None
          fahrtNr = None
          additionalName = None
          product = None
          ``public`` = None
          mode = None
          routes = None
          operator = None
          express = None
          metro = None
          night = None
          nr = None
          symbol = None
          directions = None }

    let Warning: Warning =
        { ``type`` = Some "warning"
          id = None
          icon = None
          summary = None
          text = None
          category = None
          priority = None
          products = None
          edges = None
          events = None
          validFrom = None
          validUntil = None
          modified = None
          company = None
          categories = None
          affectedLines = None
          fromStops = None
          toStops = None }

    let ServerInfo: ServerInfo =
        { timetableStart = None
          timetableEnd = None
          serverTime = None
          realtimeDataUpdatedAt = None }
