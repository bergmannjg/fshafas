/// generated by ts2fable and transformer
namespace FsHafas.Client
/// <namespacedoc>
///   <summary>FsHafas client types generated from <a href="https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts">TS types</a></summary>
/// </namespacedoc>

#if FABLE_COMPILER
#nowarn "0059"
#endif

open System

#if FABLE_COMPILER
open Fable.Core
open Fable.Core.JS
#endif

#if !FABLE_COMPILER
type Promise<'T> = Async<'T>

[<AttributeUsage(AttributeTargets.Class)>]
type StringEnumAttribute() =
    inherit Attribute()
#endif

type Log() =
    static let mutable debug = false

    static member Debug
        with get () = debug
        and set (v) = debug <- v

    static member Print (msg: string) (o: obj) = if debug then printfn "%s %A" msg o

#if FABLE_COMPILER
type IndexMap<'s, 'b when 's: comparison>(defaultValue: 'b) =
    [<EmitIndexer>]
    member __.Item
        with get (s: 's): 'b = jsNative
        and set s b = jsNative

    [<Emit("(Object.keys($0))")>]
    member __.Keys: 's [] = jsNative
#else
type IndexMap<'s, 'b when 's: comparison>(defaultValue: 'b) =
    let mutable map: Map<'s, 'b> = Map.empty

    member __.Item
        with get (s: 's) =
            match map.TryFind s with
            | Some v -> v
            | None -> defaultValue
        and set s b =
            map <- map.Add(s, b)
            ()

    member __.Keys =
        map |> Seq.map (fun kv -> kv.Key) |> Seq.toArray
#endif

/// A ProductType relates to how a means of transport "works" in local context.
/// Example: Even though S-Bahn and U-Bahn in Berlin are both trains, they have different operators, service patterns,
/// stations and look different. Therefore, they are two distinct products subway and suburban.
and ProductType =
    { id: string
      mode: ProductTypeMode
      name: string
      short: string
      bitmasks: array<int>
      ``default``: bool }
/// A profile is a specific customisation for each endpoint.
/// It parses data from the API differently, add additional information, or enable non-default methods.
and Profile =
    abstract member locale : string 
    abstract member timezone : string 
    abstract member endpoint : string 
    abstract member products : array<ProductType>
    abstract member trip : bool option 
    abstract member radar : bool option 
    abstract member refreshJourney : bool option 
    abstract member journeysFromTrip : bool option 
    abstract member reachableFrom : bool option 
    abstract member journeysWalkingSpeed : bool option 
    abstract member tripsByName : bool option 
    abstract member remarks : bool option 
    abstract member remarksGetPolyline : bool option 
    abstract member lines : bool option 
/// A location object is used by other items to indicate their locations.
and Location =
    { ``type``: string option
      id: string option
      name: string option
      poi: bool option
      address: string option
      longitude: float option
      latitude: float option
      altitude: float option
      distance: int option }
/// Each public transportation network exposes its products as boolean properties. See {@link ProductType}
and Products = IndexMap<string, bool>
and Facilities = IndexMap<string, string>

and ReisezentrumOpeningHours =
    { Mo: string option
      Di: string option
      Mi: string option
      Do: string option
      Fr: string option
      Sa: string option
      So: string option }
/// A station is a larger building or area that can be identified by a name.
/// It is usually represented by a single node on a public transport map.
/// Whereas a stop usually specifies a location, a station often is a broader area
/// that may span across multiple levels or buildings.
and Station =
    { ``type``: string option
      id: string option
      name: string option
      station: Station option
      location: Location option
      products: Products option
      lines: array<Line> option
      isMeta: bool option
      /// region ids
      regions: array<string> option
      facilities: Facilities option
      reisezentrumOpeningHours: ReisezentrumOpeningHours option
      stops: array<U3<Station, Stop, Location>> option
      entrances: array<Location> option
      transitAuthority: string option
      distance: int option }
/// Ids of a Stop, i.e. dhid as 'DELFI Haltestellen ID'
and Ids = IndexMap<string, string>
/// A stop is a single small point or structure at which vehicles stop.
/// A stop always belongs to a station. It may for example be a sign, a basic shelter or a railway platform.
and Stop =
    { ``type``: string option
      id: string option
      name: string option
      location: Location option
      station: Station option
      products: Products option
      lines: array<Line> option
      isMeta: bool option
      reisezentrumOpeningHours: ReisezentrumOpeningHours option
      ids: Ids option
      loadFactor: string option
      entrances: array<Location> option
      transitAuthority: string option
      distance: int option }
/// A region is a group of stations, for example a metropolitan area or a geographical or cultural region.
and Region =
    { ``type``: string option
      id: string
      name: string
      /// station ids
      stations: array<string> }

and Line =
    { ``type``: string option
      id: string option
      name: string option
      adminCode: string option
      fahrtNr: string option
      additionalName: string option
      product: string option
      ``public``: bool option
      mode: ProductTypeMode option
      /// routes ids
      routes: array<string> option
      operator: Operator option
      express: bool option
      metro: bool option
      night: bool option
      nr: int option
      symbol: string option
      directions: array<string> option }
/// A route represents a single set of stations, of a single line.
and Route =
    { ``type``: string option
      id: string
      line: string
      mode: ProductTypeMode
      /// stop ids
      stops: array<string> }

and Cycle =
    { min: int option
      max: int option
      nr: int option }

and ArrivalDeparture =
    { arrival: float option
      departure: float option }
/// There are many ways to format schedules of public transport routes.
/// This one tries to balance the amount of data and consumability.
/// It is specifically geared towards urban public transport, with frequent trains and homogenous travels.
and Schedule =
    { ``type``: string option
      id: string
      route: string
      mode: ProductTypeMode
      sequence: array<ArrivalDeparture>
      /// array of Unix timestamps
      starts: array<string> }

and Operator =
    { ``type``: string option
      id: string
      name: string }

and Hint =
    { ``type``: string option
      code: string option
      summary: string option
      text: string
      tripId: string option }

and Status =
    { ``type``: string option
      code: string option
      summary: string option
      text: string
      tripId: string option }

and IcoCrd =
    { x: int
      y: int
      ``type``: string option }

and Edge =
    { fromLoc: U3<Station, Stop, Location> option
      toLoc: U3<Station, Stop, Location> option
      icon: obj option
      dir: int option
      icoCrd: IcoCrd option }

and Event =
    { fromLoc: U3<Station, Stop, Location> option
      toLoc: U3<Station, Stop, Location> option
      start: string option
      ``end``: string option
      sections: array<string> option }

and Warning =
    { ``type``: string option
      id: string option
      icon: obj option
      summary: string option
      text: string option
      category: string option
      priority: int option
      products: Products option
      edges: array<Edge> option
      events: array<Event> option
      validFrom: string option
      validUntil: string option
      modified: string option
      company: string option
      categories: array<int> option
      affectedLines: array<Line> option
      fromStops: array<U3<Station, Stop, Location>> option
      toStops: array<U3<Station, Stop, Location>> option }

and Geometry =
    { ``type``: string option
      coordinates: array<float> }

and Feature =
    { ``type``: string option
      properties: obj
      geometry: Geometry }

and FeatureCollection =
    { ``type``: string option
      features: array<Feature> }
/// A stopover represents a vehicle stopping at a stop/station at a specific time.
and StopOver =
    { stop: U2<Station, Stop> option
      /// null, if last stopOver of trip
      departure: string option
      departureDelay: int option
      prognosedDeparture: string option
      plannedDeparture: string option
      departurePlatform: string option
      prognosedDeparturePlatform: string option
      plannedDeparturePlatform: string option
      /// null, if first stopOver of trip
      arrival: string option
      arrivalDelay: int option
      prognosedArrival: string option
      plannedArrival: string option
      arrivalPlatform: string option
      prognosedArrivalPlatform: string option
      plannedArrivalPlatform: string option
      remarks: array<U3<Hint, Status, Warning>> option
      passBy: bool option
      cancelled: bool option }
/// Trip – a vehicle stopping at a set of stops at specific times
and Trip =
    { id: string
      origin: U3<Station, Stop, Location> option
      destination: U3<Station, Stop, Location> option
      departure: string option
      plannedDeparture: string option
      prognosedArrival: string option
      departureDelay: int option
      departurePlatform: string option
      prognosedDeparturePlatform: string option
      plannedDeparturePlatform: string option
      arrival: string option
      plannedArrival: string option
      prognosedDeparture: string option
      arrivalDelay: int option
      arrivalPlatform: string option
      prognosedArrivalPlatform: string option
      plannedArrivalPlatform: string option
      stopovers: array<StopOver> option
      schedule: float option
      price: Price option
      operator: float option
      direction: string option
      line: Line option
      reachable: bool option
      cancelled: bool option
      walking: bool option
      loadFactor: string option
      distance: int option
      ``public``: bool option
      transfer: bool option
      cycle: Cycle option
      alternatives: array<Alternative> option
      polyline: FeatureCollection option
      remarks: array<U3<Hint, Status, Warning>> option }

and Price =
    { amount: float
      currency: string
      hint: string option }

and Alternative =
    { tripId: string
      direction: string option
      location: Location option
      line: Line option
      stop: U2<Station, Stop> option
      ``when``: string option
      plannedWhen: string option
      prognosedWhen: string option
      delay: int option
      platform: string option
      plannedPlatform: string option
      prognosedPlatform: string option
      remarks: array<U3<Hint, Status, Warning>> option
      cancelled: bool option
      loadFactor: string option
      provenance: string option
      previousStopovers: array<StopOver> option
      nextStopovers: array<StopOver> option
      frames: array<Frame> option
      polyline: FeatureCollection option }
/// Leg of journey
and Leg =
    { tripId: string option
      origin: U3<Station, Stop, Location> option
      destination: U3<Station, Stop, Location> option
      departure: string option
      plannedDeparture: string option
      prognosedArrival: string option
      departureDelay: int option
      departurePlatform: string option
      prognosedDeparturePlatform: string option
      plannedDeparturePlatform: string option
      arrival: string option
      plannedArrival: string option
      prognosedDeparture: string option
      arrivalDelay: int option
      arrivalPlatform: string option
      prognosedArrivalPlatform: string option
      plannedArrivalPlatform: string option
      stopovers: array<StopOver> option
      schedule: float option
      price: Price option
      operator: float option
      direction: string option
      line: Line option
      reachable: bool option
      cancelled: bool option
      walking: bool option
      loadFactor: string option
      distance: int option
      ``public``: bool option
      transfer: bool option
      cycle: Cycle option
      alternatives: array<Alternative> option
      polyline: FeatureCollection option
      remarks: array<U3<Hint, Status, Warning>> option }

and ScheduledDays = IndexMap<string, bool>
/// A journey is a computed set of directions to get from A to B at a specific time.
/// It would typically be the result of a route planning algorithm.
and Journey =
    { ``type``: string option
      legs: array<Leg>
      refreshToken: string option
      remarks: array<U3<Hint, Status, Warning>> option
      price: Price option
      cycle: Cycle option
      scheduledDays: ScheduledDays option }

and Journeys =
    { earlierRef: string option
      laterRef: string option
      journeys: array<Journey> option
      realtimeDataFrom: int option }

and Duration =
    { duration: int option
      stations: array<U3<Station, Stop, Location>> }

and Frame =
    { origin: U2<Stop, Location>
      destination: U2<Stop, Location>
      t: int option }

and Movement =
    { direction: string option
      tripId: string option
      line: Line option
      location: Location option
      nextStopovers: array<StopOver> option
      frames: array<Frame> option
      polyline: FeatureCollection option }

and ServerInfo =
    { timetableStart: string option
      timetableEnd: string option
      serverTime: string option
      realtimeDataUpdatedAt: int option }

and LoyaltyCard =
    { ``type``: string option
      discount: int option }

and JourneysOptions =
    {
      /// departure date, undefined corresponds to Date.Now
      departure: DateTime option
      /// arrival date, departure and arrival are mutually exclusive.
      arrival: DateTime option
      /// earlierThan, use {@link Journeys#earlierRef}, earlierThan and departure/arrival are mutually exclusive.
      earlierThan: string option
      /// laterThan, use {@link Journeys#laterRef}, laterThan and departure/arrival are mutually exclusive.
      laterThan: string option
      /// how many search results?
      results: int option
      /// let journeys pass this station
      via: string option
      /// return stations on the way?
      stopovers: bool option
      /// Maximum nr of transfers. Default: Let HAFAS decide.
      transfers: int option
      /// minimum time for a single transfer in minutes
      transferTime: int option
      /// 'none', 'partial' or 'complete'
      accessibility: string option
      /// only bike-friendly journeys
      bike: bool option
      products: Products option
      /// return tickets? only available with some profiles
      tickets: bool option
      /// return a shape for each leg?
      polylines: bool option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// parse & expose hints & warnings?
      remarks: bool option
      /// 'slow', 'normal', 'fast'
      walkingSpeed: string option
      /// start with walking
      startWithWalking: bool option
      /// language to get results in
      language: string option
      /// parse which days each journey is valid on
      scheduledDays: bool option
      /// firstClass
      firstClass: bool option
      /// LoyaltyCard
      loyaltyCard: LoyaltyCard option
      ``when``: DateTime option }

and JourneysFromTripOptions =
    {
      /// return stations on the way?
      stopovers: bool option
      /// minimum time for a single transfer in minutes
      transferTime: int option
      /// 'none', 'partial' or 'complete'
      accessibility: string option
      /// return tickets?
      tickets: bool option
      /// return leg shapes?
      polylines: bool option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// parse & expose hints & warnings?
      remarks: bool option
      /// products
      products: Products option }

and LocationsOptions =
    {
      /// find only exact matches?
      fuzzy: bool option
      /// how many search results?
      results: int option
      /// return stops/stations?
      stops: bool option
      /// return addresses
      addresses: bool option
      /// points of interest
      poi: bool option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// parse & expose lines at each stop/station?
      linesOfStops: bool option
      /// Language of the results
      language: string option }

and TripOptions =
    {
      /// return stations on the way?
      stopovers: bool option
      /// return a shape for the trip?
      polyline: bool option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// parse & expose hints & warnings?
      remarks: bool option
      /// Language of the results
      language: string option }

and StopOptions =
    {
      /// parse & expose lines at the stop/station?
      linesOfStops: bool option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// parse & expose hints & warnings?
      remarks: bool option
      /// Language of the results
      language: string option }

and DeparturesArrivalsOptions =
    {
      /// departure date, undefined corresponds to Date.Now
      ``when``: DateTime option
      /// only show departures heading to this station
      direction: string option
      /// filter by line ID
      line: string option
      /// show departures for the next n minutes
      duration: int option
      /// max. number of results; `null` means "whatever HAFAS wants"
      results: int option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// parse & expose lines at the stop/station?
      linesOfStops: bool option
      /// parse & expose hints & warnings?
      remarks: bool option
      /// fetch & parse previous/next stopovers?
      stopovers: bool option
      /// departures at related stations
      includeRelatedStations: bool option
      /// products
      products: Products option
      /// language
      language: string option }

and RefreshJourneyOptions =
    {
      /// return stations on the way?
      stopovers: bool option
      /// return a shape for each leg?
      polylines: bool option
      /// return tickets? only available with some profiles
      tickets: bool option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// parse & expose hints & warnings?
      remarks: bool option
      /// language
      language: string option }

and NearByOptions =
    {
      /// maximum number of results
      results: int option
      /// maximum walking distance in meters
      distance: int option
      /// return points of interest?
      poi: bool option
      /// return stops/stations?
      stops: bool option
      /// products
      products: Products option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// parse & expose lines at each stop/station?
      linesOfStops: bool option
      /// language
      language: string option }

and ReachableFromOptions =
    {
      /// when
      ``when``: DateTime option
      /// maximum of transfers
      maxTransfers: int option
      /// maximum travel duration in minutes, pass `null` for infinite
      maxDuration: int option
      /// products
      products: Products option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// return leg shapes?
      polylines: bool option }

and BoundingBox =
    { north: float
      west: float
      south: float
      east: float }

and RadarOptions =
    {
      /// maximum number of vehicles
      results: int option
      /// nr of frames to compute
      frames: int option
      /// optionally an object of booleans
      products: Products option
      /// compute frames for the next n seconds
      duration: int option
      /// parse & expose sub-stops of stations?
      subStops: bool option
      /// parse & expose entrances of stops/stations?
      entrances: bool option
      /// return a shape for the trip?
      polylines: bool option
      /// when
      ``when``: DateTime option }

and TripsByNameOptions =
    {
      /// departure date, undefined corresponds to Date.Now
      ``when``: DateTime option }

and RemarksOptions =
    { from: DateTime option
      ``to``: DateTime option
      /// maximum number of remarks
      results: int option
      products: Products option
      /// return leg shapes? (not supported by all endpoints)
      polylines: bool option
      /// Language of the results
      language: string option }

and LinesOptions =
    {
      /// Language of the results
      language: string option }

and ServerOptions =
    {
      /// Language of the results
      language: string option }

and HafasClient =
    /// Retrieves journeys
    abstract member journeys :
        U4<string, Station, Stop, Location> ->
        U4<string, Station, Stop, Location> ->
        JourneysOptions option ->
        Promise<Journeys>
    /// refreshes a Journey
    abstract member refreshJourney : string -> RefreshJourneyOptions option -> Promise<Journey>
    /// Refetch information about a trip
    abstract member trip : string -> string -> TripOptions option -> Promise<Trip>
    /// Retrieves departures
    abstract member departures :
        U4<string, Station, Stop, Location> -> DeparturesArrivalsOptions option -> Promise<array<Alternative>>
    /// Retrieves arrivals
    abstract member arrivals :
        U4<string, Station, Stop, Location> -> DeparturesArrivalsOptions option -> Promise<array<Alternative>>
    /// Retrieves journeys from trip id to station
    abstract member journeysFromTrip :
        string ->
        StopOver ->
        U4<string, Station, Stop, Location> ->
        JourneysFromTripOptions option ->
        Promise<array<Journey>>
    /// Retrieves locations or stops
    abstract member locations : string -> LocationsOptions option -> Promise<array<U3<Station, Stop, Location>>>
    /// Retrieves information about a stop
    abstract member stop : U2<string, Stop> -> StopOptions option -> Promise<U3<Station, Stop, Location>>
    /// Retrieves nearby stops from location
    abstract member nearby : Location -> NearByOptions option -> Promise<array<U3<Station, Stop, Location>>>
    /// Retrieves stations reachable within a certain time from a location
    abstract member reachableFrom : Location -> ReachableFromOptions option -> Promise<array<Duration>>
    /// Retrieves all vehicles currently in an area.
    abstract member radar : BoundingBox -> RadarOptions option -> Promise<array<Movement>>
    /// Retrieves trips by name.
    abstract member tripsByName : string -> TripsByNameOptions option -> Promise<array<Trip>>
    /// Fetches all remarks known to the HAFAS endpoint
    abstract member remarks : RemarksOptions option -> Promise<array<Warning>>
    /// Fetches all lines known to the HAFAS endpoint
    abstract member lines : string -> LinesOptions option -> Promise<array<Line>>
    /// Fetches meta information from the HAFAS endpoint
    abstract member serverInfo : ServerOptions option -> Promise<ServerInfo>


and [<StringEnum; RequireQualifiedAccess>] ProductTypeMode =
    | [<CompiledName "train">] Train
    | [<CompiledName "bus">] Bus
    | [<CompiledName "watercraft">] Watercraft
    | [<CompiledName "taxi">] Taxi
    | [<CompiledName "gondola">] Gondola
    | [<CompiledName "aircraft">] Aircraft
    | [<CompiledName "car">] Car
    | [<CompiledName "bicycle">] Bicycle
    | [<CompiledName "walking">] Walking

and [<StringEnum; RequireQualifiedAccess>] HintType =
    | [<CompiledName "hint">] Hint
    | [<CompiledName "status">] Status
    | [<CompiledName "foreign-id">] ForeignId
    | [<CompiledName "local-fare-zone">] LocalFareZone
    | [<CompiledName "stop-website">] StopWebsite
    | [<CompiledName "stop-dhid">] StopDhid
    | [<CompiledName "transit-authority">] TransitAuthority

and [<StringEnum; RequireQualifiedAccess>] WarningType =
    | [<CompiledName "status">] Status
    | [<CompiledName "warning">] Warning
