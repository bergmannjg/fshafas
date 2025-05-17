namespace DbVendo.Parser

module internal Trip =

    open FsHafas.Client
    open DbVendo
    open DbVendo.Client

    let private FromLeg (id: string) (l: FsHafas.Client.Leg) : FsHafas.Client.Trip =
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
          cancelled = l.cancelled
          walking = l.walking
          loadFactor = l.loadFactor
          distance = l.distance
          ``public`` = l.``public``
          transfer = l.transfer
          cycle = l.cycle
          alternatives = l.alternatives
          polyline = l.polyline
          remarks = l.remarks
          scheduledDays = None
          currentLocation = l.currentLocation
          departurePrognosisType = l.departurePrognosisType
          arrivalPrognosisType = l.arrivalPrognosisType
          checkin = l.checkin }

    let parseTrip (id: string) (response: Raw.VerbindungsAbschnitt) : Trip =
        let leg = Journey.parseVerbindungsAbschnitt response

        { FromLeg id leg with
            scheduledDays = None }
