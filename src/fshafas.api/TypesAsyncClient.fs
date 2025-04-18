namespace FsHafas.Api

open FsHafas.Client

type IAsyncClient =
    abstract member AsyncJourneys:
        from: U4<string, Station, Stop, Location> ->
        ``to``: U4<string, Station, Stop, Location> ->
        opt: JourneysOptions option ->
            Async<Journeys>

    abstract member AsyncBestPrices:
        from: U4<string, Station, Stop, Location> ->
        ``to``: U4<string, Station, Stop, Location> ->
        opt: JourneysOptions option ->
            Async<Journeys>

    abstract member AsyncRefreshJourney:
        refreshToken: string -> opt: RefreshJourneyOptions option -> Async<JourneyWithRealtimeData>

    abstract member AsyncJourneysFromTrip:
        fromTripId: string ->
        previousStopOver: StopOver ->
        ``to``: U4<string, Station, Stop, Location> ->
        opt: JourneysFromTripOptions option ->
            Async<Journeys>

    abstract member AsyncTrip: id: string -> opt: TripOptions option -> Async<TripWithRealtimeData>

    abstract member AsyncDepartures:
        name: U4<string, Station, Stop, Location> -> opt: DeparturesArrivalsOptions option -> Async<Departures>

    abstract member AsyncArrivals:
        name: U4<string, Station, Stop, Location> -> opt: DeparturesArrivalsOptions option -> Async<Arrivals>

    abstract member AsyncLocations: name: string -> opt: LocationsOptions option -> Async<array<StationStopLocation>>

    abstract member AsyncStop: stop: U2<string, Stop> -> opt: StopOptions option -> Async<StationStopLocation>

    abstract member AsyncNearby: l: Location -> opt: NearByOptions option -> Async<array<StationStopLocation>>

    abstract member AsyncReachableFrom:
        l: Location -> opt: ReachableFromOptions option -> Async<DurationsWithRealtimeData>

    abstract member AsyncRadar: rect: BoundingBox -> opt: RadarOptions option -> Async<Radar>
    abstract member AsyncTripsByName: lineName: string -> opt: TripsByNameOptions option -> Async<TripsWithRealtimeData>

    abstract member AsyncRemarks: opt: RemarksOptions option -> Async<WarningsWithRealtimeData>

    abstract member AsyncLines: query: string -> opt: LinesOptions option -> Async<LinesWithRealtimeData>
    abstract member AsyncServerInfo: opt: ServerOptions option -> Async<ServerInfo>

