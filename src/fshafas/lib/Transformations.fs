namespace FsHafas.Client

module internal Coordinate =

    let toFloat (v: int) = float (v) / 1000000.0

    let fromFloat (x: float) = System.Math.Round(x * 1000000.0) |> int

module internal RawDep =

    open FsHafas.Client

    let FromRawStopL (s: FsHafas.Raw.RawStop) : FsHafas.Raw.RawDep =
        { locX = s.locX
          idx = s.idx
          dProdX = s.dProdX
          dPlatfS = s.dPlatfS
          dInR = s.dInR
          dTimeS = s.dTimeS
          dProgType = s.dProgType
          dTrnCmpSX = s.dTrnCmpSX
          dTZOffset = s.dTZOffset
          msgL = s.msgL
          dPlatfCh = s.dPlatfCh
          ``type`` = s.``type``
          dTimeR = s.dTimeR
          dCncl = s.dCncl
          dPltfS = s.dPltfS
          dPlatfR = s.dPlatfR
          dPltfR = s.dPltfR }

module internal RawArr =

    open FsHafas.Client

    let FromRawStopL (s: FsHafas.Raw.RawStop) : FsHafas.Raw.RawArr =
        { locX = s.locX
          idx = s.idx
          aPlatfS = s.aPlatfS
          aOutR = s.aOutR
          aTimeS = s.aTimeS
          aProgType = s.aProgType
          aTZOffset = s.aTZOffset
          msgL = s.msgL
          ``type`` = s.``type``
          aTimeR = s.aTimeR
          aCncl = s.aCncl
          aPltfS = s.aPltfS
          aPlatfR = s.aPlatfR
          aPltfR = s.aPltfR
          prodL = None }


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

module internal U2StationStop =

    open FsHafas.Client

    let FromU3StationStopLocation (u3: U3<Station, Stop, Location>) =
        match u3 with
        | U3.Case1 s -> U2<FsHafas.Client.Station, FsHafas.Client.Stop>.Case1 s |> Some
        | U3.Case2 s -> U2<FsHafas.Client.Station, FsHafas.Client.Stop>.Case2 s |> Some
        | _ -> None

    let FromSomeU3StationStopLocation (u3: StationStopLocation option) =
        match u3 with
        | Some(StationStopLocation.Station s) -> StationStop.Station s |> Some
        | Some(StationStopLocation.Stop s) -> StationStop.Stop s |> Some
        | _ -> None

module internal U2StopLocation =

    open FsHafas.Client

    let FromU3StationStopLocation (u3: U3<Station, Stop, Location>) =
        match u3 with
        | U3.Case2 s -> U2<FsHafas.Client.Stop, FsHafas.Client.Location>.Case1 s |> Some
        | U3.Case3 s -> U2<FsHafas.Client.Stop, FsHafas.Client.Location>.Case2 s |> Some
        | _ -> None

    let FromSomeU3StationStopLocation (u3: StationStopLocation option) =
        match u3 with
        | Some(StationStopLocation.Stop s) -> StopLocation.Stop s |> Some
        | Some(StationStopLocation.Location s) -> StopLocation.Location s |> Some
        | _ -> None

module internal MergeOptions =

    open FsHafas.Client
    open FsHafas.Endpoint

    let private getOptionValue<'a, 'b> (opt: 'a option) (getter: 'a -> 'b option) (defaultValue: 'b) =
        match opt with
        | Some(value) ->
            match getter value with
            | Some result -> result
            | None -> defaultValue
        | None -> defaultValue

    let JourneysOptions (options: FsHafas.Endpoint.Options) (opt: JourneysOptions option) =
        { options with
            remarks = getOptionValue opt (fun v -> v.remarks) options.remarks
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
