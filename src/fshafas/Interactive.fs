namespace FsHafas

module Interactive =

    open System
    open FsHafas
    open Client

#if FABLE_COMPILER
    open Fable.Core
#endif

    let private idOfU3StationStopLocation (location: U3<Station, Stop, Location>) =
        match location with
        | U3.Case3 l -> l.id
        | U3.Case2 s -> s.id
        | U3.Case1 s -> s.id

    let private getProfile (id: ProfileId) =
        match id with
        | Db -> FsHafas.Profiles.Db.getProfile ()
        | Bvg -> FsHafas.Profiles.Bvg.getProfile ()
        | Svv -> FsHafas.Profiles.Svv.getProfile ()

    let productsOfMode (id: ProfileId) (mode: Client.ProductTypeMode) : Client.Products =
        getProfile(id).products
        |> Array.filter (fun p -> p.mode = mode && p.name <> "Tram")
        |> Array.fold
            (fun m p ->
                m.[p.id] <- true
                m)
            (Products(false))

    let AsyncLocations (id: ProfileId) (name: string) (opt: LocationsOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! locations = client.AsyncLocations name (Some opt)

                if (FsHafas.Log.Debug) then
                    printfn "%A" locations

                return locations
            }

        with ex ->
            printfn "error: %s %s" ex.Message ex.StackTrace
            async { return Array.empty }

#if !FABLE_COMPILER
    let locations (id: ProfileId) (name: string) (opt: LocationsOptions) =
        try
            Api.HafasAsyncClient.initSerializer()

            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! locations = client.AsyncLocations name (Some opt)

                if (FsHafas.Log.Debug) then
                    printfn "%A" locations

                return locations
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "error: %s %s" ex.Message ex.StackTrace
            Array.empty
#endif

    let asyncJourneys (id: ProfileId) (from: string) (``to``: string) (opt: JourneysOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            let locationsOptions =
                { Default.LocationsOptions with
                      results = Some 1 }

            async {
                let! fromlocations = client.AsyncLocations from (Some locationsOptions)
                let! tolocations = client.AsyncLocations ``to`` (Some locationsOptions)

                if fromlocations.Length > 0 && tolocations.Length > 0 then
                    match idOfU3StationStopLocation fromlocations.[0], idOfU3StationStopLocation tolocations.[0] with
                    | Some fromId, Some toId ->
                        let! journeys = client.AsyncJourneys (U4.Case1 fromId) (U4.Case1 toId) (Some opt)

                        if (FsHafas.Log.Debug) then
                            printfn "%A" journeys

                        match journeys.journeys with
                        | Some journeys when
                            journeys.Length > 0
                            && journeys.[0].legs.Length > 0 ->
                            let leg = journeys.[0].legs.[0]
                            let! trip = client.AsyncTrip leg.tripId.Value "" None

                            if (FsHafas.Log.Debug) then
                                printfn "%A" trip
                        | _ -> ()

                        return journeys
                    | _ -> return Default.Journeys
                else
                    return Default.Journeys
            }

        with ex ->
            // fprintfn stderr "journeys: %s %s" ex.Message ex.StackTrace
            printfn "journeys: %s" ex.Message
            async { return Default.Journeys }

#if !FABLE_COMPILER
    let journeys (id: ProfileId) (from: string) (``to``: string) (opt: JourneysOptions) =
        try
            Api.HafasAsyncClient.initSerializer()

            use client =
                new Api.HafasAsyncClient(getProfile (id))

            let locationsOptions =
                { Default.LocationsOptions with
                      results = Some 1 }

            async {
                let! fromlocations = client.AsyncLocations from (Some locationsOptions)
                let! tolocations = client.AsyncLocations ``to`` (Some locationsOptions)

                if fromlocations.Length > 0 && tolocations.Length > 0 then
                    match idOfU3StationStopLocation fromlocations.[0], idOfU3StationStopLocation tolocations.[0] with
                    | Some fromId, Some toId ->
                        let! journeys = client.AsyncJourneys (U4.Case1 fromId) (U4.Case1 toId) (Some opt)

                        if (FsHafas.Log.Debug) then
                            printfn "%A" journeys

                        match journeys.journeys with
                        | Some journeys when
                            journeys.Length > 0
                            && journeys.[0].legs.Length > 0 ->
                            let leg = journeys.[0].legs.[0]
                            let! trip = client.AsyncTrip leg.tripId.Value "" None

                            if (FsHafas.Log.Debug) then
                                printfn "%A" trip
                        | _ -> ()

                        return journeys
                    | _ -> return Default.Journeys
                else
                    return Default.Journeys
            }
            |> Async.RunSynchronously

        with ex ->
            // fprintfn stderr "journeys: %s %s" ex.Message ex.StackTrace
            printfn "journeys: %s" ex.Message
            Default.Journeys
#endif

#if !FABLE_COMPILER
    let departures (id: ProfileId) (name: string) (opt: DeparturesArrivalsOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            let locationsOptions =
                { Default.LocationsOptions with
                      results = Some 1 }

            async {
                let! locations = client.AsyncLocations name (Some locationsOptions)

                if locations.Length > 0 then
                    match idOfU3StationStopLocation locations.[0] with
                    | Some id ->
                        let! departures = client.AsyncDepartures (U2.Case1 id) (Some opt)

                        if (FsHafas.Log.Debug) then
                            printfn "%A" departures

                        return departures
                    | _ -> return Array.empty
                else
                    return Array.empty
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "departures: %s" ex.Message
            Array.empty
#endif

#if !FABLE_COMPILER
    let refreshJourney (id: ProfileId) (refreshToken: string) (opt: RefreshJourneyOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! journey = client.AsyncRefreshJourney refreshToken (Some opt)

                return journey
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "refreshJourney: %s" ex.Message
            Default.Journey
#endif

#if !FABLE_COMPILER
    let tripsByName (id: ProfileId) (name: string) (opt: TripsByNameOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! trips = client.AsyncTripsByName name (Some opt)

                return trips
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "tripsByName: %s" ex.Message
            [||]
#endif

#if !FABLE_COMPILER
    let nearby (id: ProfileId) (l: Location) (opt: NearByOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! trips = client.AsyncNearby l (Some opt)

                return trips
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "nearby: %s" ex.Message
            [||]
#endif

#if !FABLE_COMPILER
    let reachableFrom (id: ProfileId) (l: Location) (opt: ReachableFromOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! trips = client.AsyncReachableFrom l (Some opt)

                return trips
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "nearby: %s" ex.Message
            [||]
#endif

#if !FABLE_COMPILER
    let radar (id: ProfileId) (r: BoundingBox) (opt: RadarOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! trips = client.AsyncRadar r (Some opt)

                return trips
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "radar: %s" ex.Message
            [||]
#endif

#if !FABLE_COMPILER
    let stop (id: ProfileId) (stop: string) (opt: StopOptions) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! trips = client.AsyncStop (U2.Case1 stop) (Some opt)

                return trips
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "stop: %s" ex.Message
            U3.Case3 Default.Location
#endif

#if !FABLE_COMPILER
    let remarks (id: ProfileId) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! warnings = client.AsyncRemarks (Some Default.RemarksOptions)

                return warnings
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "remarks: %s" ex.Message
            Array.empty
#endif

#if !FABLE_COMPILER
    let lines (id: ProfileId) (query:string) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! lines = client.AsyncLines query (Some Default.LinesOptions)

                return lines
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "lines: %s" ex.Message
            Array.empty
#endif

#if !FABLE_COMPILER
    let serverInfo (id: ProfileId) =
        try
            use client =
                new Api.HafasAsyncClient(getProfile (id))

            async {
                let! serverInfo = client.AsyncServerInfo (Some Default.ServerOptions)

                return serverInfo
            }
            |> Async.RunSynchronously

        with ex ->
            printfn "serverInfo: %s" ex.Message
            Default.ServerInfo
#endif
