namespace DbVendo.Api

open System

open FsHafas.Api
open FsHafas.Client
open DbVendo.Client
open DbVendo

/// <namespacedoc>
///   <summary>Types of db vendo api</summary>
/// </namespacedoc>
/// <summary>F# async based interface for db vendo endpoint</summary>
type DbVendoAsyncClient() =

#if !FABLE_COMPILER
    static let converter =
        new Converter.U5EraseConverter<
            Raw.LocationsRequestBody,
            Raw.JourneysRequestBody,
            Raw.RefreshJourneysRequestBody,
            Raw.StationBoardRequestBody,
            Raw.NearByRequestBody
         >(
            Converter.UnionCaseSelection.Disabled
        )

    do Serializer.addSerializeConverters [| converter |]

#endif
    let log msg o = FsHafas.Client.Log.Print msg o
    let httpClient = Request.HttpClient()

    let SendAsync (request: Raw.Request) : Async<string> =
        let data: DbVendo.Client.Request.Data =
            match request.data with
            | Raw.RequestData.Post(contentType, body) -> Request.Data.Post(contentType, Serializer.U5.Serialize body)
            | Raw.RequestData.Get path -> Request.Data.Get(path)

        httpClient.SendAsync request.endpoint request.xCorrelationID request.accept data

    interface IDisposable with
        member __.Dispose() = httpClient.Dispose()

    interface IAsyncClient with
        member __.AsyncJourneys
            (from: U4<string, Station, Stop, Location>)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysOptions option)
            : Async<Journeys> =

            async {
                let request = Format.journeysRequest from ``to`` opt
                let! response = SendAsync request
                return Parser.Journey.parseJourneys opt (Serializer.Deserialize<Raw.JourneysResponse> response)
            }

        member __.AsyncBestPrices
            (from: U4<string, Station, Stop, Location>)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysOptions option)
            : Async<Journeys> =

            failwith "nyi"

        member __.AsyncRefreshJourney
            (refreshToken: string)
            (opt: RefreshJourneyOptions option)
            : Async<JourneyWithRealtimeData> =

            async {
                let refreshToken, journeysOptions = Format.splitRefreshToken refreshToken
                let request = Format.refreshJourneysRequest refreshToken opt journeysOptions
                let! response = SendAsync request

                return
                    { realtimeDataUpdatedAt = None
                      journey =
                        Parser.Journey.parseJourney journeysOptions (Serializer.Deserialize<Raw.VerbindungEx> response) }
            }

        member __.AsyncJourneysFromTrip
            (fromTripId: string)
            (previousStopOver: StopOver)
            (``to``: U4<string, Station, Stop, Location>)
            (opt: JourneysFromTripOptions option)
            : Async<Journeys> =

            failwith "nyi"

        member __.AsyncTrip (id: string) (opt: TripOptions option) : Async<TripWithRealtimeData> =

            async {
                let request = Format.tripRequest id opt
                let! response = SendAsync request

                return
                    { realtimeDataUpdatedAt = None
                      trip = Parser.Trip.parseTrip id (Serializer.Deserialize<Raw.VerbindungsAbschnitt> response) }
            }

        member __.AsyncDepartures
            (name: U4<string, Station, Stop, Location>)
            (opt: DeparturesArrivalsOptions option)
            : Async<Departures> =

            async {
                let request = Format.stationBoardRequest name true opt
                let! response = SendAsync request

                return
                    { realtimeDataUpdatedAt = None
                      departures =
                        Parser.StationBoard.parseStationBoard (
                            Serializer.Deserialize<Raw.StationBoardResponse> response
                        ) }
            }

        member __.AsyncArrivals
            (name: U4<string, Station, Stop, Location>)
            (opt: DeparturesArrivalsOptions option)
            : Async<Arrivals> =

            async {
                let request = Format.stationBoardRequest name false opt
                let! response = SendAsync request

                return
                    { realtimeDataUpdatedAt = None
                      arrivals =
                        Parser.StationBoard.parseStationBoard (
                            Serializer.Deserialize<Raw.StationBoardResponse> response
                        ) }
            }

        member _.AsyncLocations (name: string) (opt: LocationsOptions option) : Async<array<StationStopLocation>> =

            async {
                let request = Format.locationsRequest name opt
                let! response = SendAsync request
                return DbVendo.Parser.Location.parseLocations (Serializer.Deserialize<Raw.LocationsResponse> response)
            }

        member __.AsyncStop (stop: U2<string, Stop>) (opt: StopOptions option) : Async<StationStopLocation> =

            async {
                let request = Format.stopRequest stop opt
                let! response = SendAsync request
                return DbVendo.Parser.Stop.parseStopResponse stop (Serializer.Deserialize<Raw.StopResponse> response)
            }

        member __.AsyncNearby (l: Location) (opt: NearByOptions option) : Async<array<StationStopLocation>> =

            async {
                let request = Format.nearByRequest l opt
                let! response = SendAsync request

                return DbVendo.Parser.Location.parseLocations (Serializer.Deserialize<Raw.LocationsResponse> response)
            }

        member __.AsyncReachableFrom
            (l: Location)
            (opt: ReachableFromOptions option)
            : Async<DurationsWithRealtimeData> =

            failwith "nyi"

        member __.AsyncRadar (rect: BoundingBox) (opt: RadarOptions option) : Async<Radar> =

            failwith "nyi"

        member __.AsyncTripsByName (lineName: string) (opt: TripsByNameOptions option) : Async<TripsWithRealtimeData> =

            failwith "nyi"

        member __.AsyncRemarks(opt: RemarksOptions option) : Async<WarningsWithRealtimeData> =

            failwith "nyi"

        member __.AsyncLines (query: string) (opt: LinesOptions option) : Async<LinesWithRealtimeData> =

            failwith "nyi"

        member __.AsyncServerInfo(opt: ServerOptions option) : Async<ServerInfo> =

            failwith "nyi"
