namespace FsHafas.Client

module Default =

    open FsHafas.Client

    let TripOptions: TripOptions =
        { stopovers = Some true
          polyline = Some true
          subStops = Some true
          entrances = Some true
          remarks = Some true
          scheduledDays = None
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
          scheduledDays = None
          language = Some "de"
          generateUnreliableTicketUrls = Some false }

    let TripsByNameOptions: TripsByNameOptions =
        { ``when`` = Some System.DateTime.Now
          fromWhen = None
          untilWhen = None
          onlyCurrentlyRunning = None
          products = None
          currentlyStoppingAt = None
          lineName = None
          operatorNames = None
          additionalFilters = None }

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
          ageGroup = None
          age = None
          loyaltyCard = None
          routingMode = None
          ``when`` = None
          generateUnreliableTicketUrls = Some false }

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
          language = Some "de" }

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

    let ServerOptions: ServerOptions =
        { versionInfo = Some true
          language = Some "de" }

    let Location: Location =
        { ``type`` = LocationType.Location
          id = None
          name = None
          poi = None
          address = None
          longitude = None
          latitude = None
          altitude = None
          distance = None }

    let Stop: Stop =
        { ``type`` = StopType.Stop
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
          distance = None
          facilities = None }

    let Station: Station =
        { ``type`` = StationType.Station
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
          remarks = None
          currentLocation = None
          departurePrognosisType = None
          arrivalPrognosisType = None
          checkin = None }

    let StopOver: StopOver =
        { stop = None
          departure = None
          departureDelay = None
          prognosedDeparture = None
          plannedDeparture = None
          departurePlatform = None
          prognosedDeparturePlatform = None
          plannedDeparturePlatform = None
          arrival = None
          arrivalDelay = None
          prognosedArrival = None
          plannedArrival = None
          arrivalPlatform = None
          prognosedArrivalPlatform = None
          plannedArrivalPlatform = None
          remarks = None
          passBy = None
          cancelled = None
          departurePrognosisType = None
          arrivalPrognosisType = None
          additional = None }

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
          cancelled = None
          walking = None
          loadFactor = None
          distance = None
          ``public`` = None
          transfer = None
          cycle = None
          alternatives = None
          polyline = None
          remarks = None
          scheduledDays = None
          currentLocation = None
          departurePrognosisType = None
          arrivalPrognosisType = None
          checkin = None }

    let TripWithRealtimeData: TripWithRealtimeData =
        { trip = Trip
          realtimeDataUpdatedAt = None }

    let TripsWithRealtimeData: TripsWithRealtimeData =
        { trips = [||]
          realtimeDataUpdatedAt = None }

    let WarningsWithRealtimeData: WarningsWithRealtimeData =
        { remarks = [||]
          realtimeDataUpdatedAt = None }

    let LinesWithRealtimeData: LinesWithRealtimeData =
        { lines = None
          realtimeDataUpdatedAt = None }

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
          currentTripPosition = None
          origin = None
          destination = None
          prognosisType = None }

    let Journey: Journey =
        { ``type`` = JourneyType.Journey
          legs = [||]
          refreshToken = None
          remarks = None
          price = None
          cycle = None
          scheduledDays = None
          tickets = None }

    let JourneyWithRealtimeData: JourneyWithRealtimeData =
        { journey = Journey
          realtimeDataUpdatedAt = None }

    let Journeys: Journeys =
        { earlierRef = None
          laterRef = None
          journeys = None
          realtimeDataUpdatedAt = None }

    let DurationsWithRealtimeData: DurationsWithRealtimeData =
        { reachable = [||]
          realtimeDataUpdatedAt = None }

    let Radar: Radar =
        { movements = None
          realtimeDataUpdatedAt = None }

    let Line: FsHafas.Client.Line =
        { ``type`` = LineType.Line
          id = None
          name = None
          adminCode = None
          matchId = None
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
          directions = None
          productName = None }

    let Warning: Warning =
        { ``type`` = WarningType.Warning
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
        { hciVersion = None
          timetableStart = None
          timetableEnd = None
          serverTime = None
          realtimeDataUpdatedAt = None }
