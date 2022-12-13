namespace FsHafas.Client

module internal Format =

#if FABLE_PY
    open FsHafas.Extensions
#endif

    let private ParseIsoString (datetime: string) =
        let year = datetime.Substring(0, 4) |> int
        let month = datetime.Substring(5, 2) |> int
        let day = datetime.Substring(8, 2) |> int
        let hour = datetime.Substring(11, 2) |> int
        let minute = datetime.Substring(14, 2) |> int

        let tzOffset = datetime.Substring(20, 2) |> int |> (*) 60

#if FABLE_PY
        // workaround: missing code DateTimeOffset
        System.DateTime(year, month, day, hour, minute, 0)
#else
        System
            .DateTimeOffset(
                year,
                month,
                day,
                hour,
                minute,
                0,
                System.TimeSpan(tzOffset / 60, 0, 0)
            )
            .DateTime
#endif

    let private maybeGetOptionValue<'a, 'b> (opt: 'a option) (getter: 'a -> 'b option) =
        match opt with
        | Some (value) -> getter value
        | None -> None

    let private getOptionValue<'a, 'b> (opt: 'a option) (getter: 'a -> 'b option) (defaultOpt: 'a) =
        let defaultValue =
            match getter defaultOpt with
            | Some value -> value
            | None -> failwith "getOptionValue: value expected"

        match opt with
        | Some (value) ->
            match getter value with
            | Some result -> result
            | None -> defaultValue
        | None -> defaultValue

    let formatDate (dt: System.DateTime) =
#if FABLE_PY
        DateTimeEx.formatDate dt "yyyyMMdd"
#else
        dt.ToString("yyyyMMdd")
#endif

    let formatTime (dt: System.DateTime) =
#if FABLE_PY
        DateTimeEx.formatTime dt "HHmm" + "00"
#else
        dt.ToString("HHmm") + "00"
#endif

    let private formatProductsBitmask (profile: FsHafas.Endpoint.Profile) (products: FsHafas.Client.Products) =
        (profile :> FsHafas.Client.Profile).products
        |> Array.filter (fun p -> products.[p.id])
        |> Array.fold (fun bitmask p -> p.bitmasks.[0] ||| bitmask) 0

    let private makeFilters (profile: FsHafas.Endpoint.Profile) (products: FsHafas.Client.Products) =
        let bitmask = formatProductsBitmask profile products

        let filters: FsHafas.Raw.JnyFltr [] =
            if bitmask <> 0 then
                [| { ``type`` = "PROD"
                     mode = "INC"
                     value = Some(bitmask.ToString())
                     meta = None } |]
            else
                [||]

        filters

    let locationRequest
        (profile: FsHafas.Endpoint.Profile)
        (name: string)
        (opt: FsHafas.Client.LocationsOptions option)
        : string * FsHafas.Raw.LocMatchRequest =
        let fuzzy = getOptionValue opt (fun v -> v.fuzzy) Default.LocationsOptions

        let results = getOptionValue opt (fun v -> v.results) Default.LocationsOptions

        let language = getOptionValue opt (fun v -> v.language) Default.LocationsOptions

        language,
        { input =
            { loc =
                { ``type`` = "ALL"
                  name = Some(name + (if fuzzy then "?" else ""))
                  lid = None }
              maxLoc = results
              field = "S" } }

    let private makeLocLTypeS (profile: FsHafas.Endpoint.Profile) (id: string) : FsHafas.Raw.Loc =
        let k =
            if System.Text.RegularExpressions.Regex.IsMatch(id, @"^\d{6,}$") then
                "L"
            else
                "O"

        { ``type`` = "S"
          name = None
          lid = Some($"A=1@" + k + "=" + id + "@") }

    let private makeLoclTypeA (location: FsHafas.Client.Location) : FsHafas.Raw.Loc =
        let x =
            match location.longitude with
            | Some (f) -> (Coordinate.fromFloat f)
            | None -> raise (System.ArgumentException("location.longitude"))

        let y =
            match location.latitude with
            | Some (f) -> (Coordinate.fromFloat f)
            | None -> raise (System.ArgumentException("location.latitude"))

        let xs = x.ToString()
        let ys = y.ToString()

        match location.address with
        | Some name ->
            { ``type`` = "A"
              name = Some(name)
              lid = Some("A=2@O=" + name + "@X=" + xs + "@Y=" + ys + "@") }
        | None ->
            { ``type`` = "A"
              name = None
              lid = Some("A=1@X=" + xs + "@Y=" + ys + "@") }

    let private makeLocType
        (profile: FsHafas.Endpoint.Profile)
        (s: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        =
        match s with
        | U4.Case1 v -> makeLocLTypeS profile v
        | U4.Case2 v when v.id.IsSome -> makeLocLTypeS profile v.id.Value
        | U4.Case3 v when v.id.IsSome -> makeLocLTypeS profile v.id.Value
        | U4.Case4 v when v.id.IsSome -> makeLocLTypeS profile v.id.Value
        | U4.Case4 v -> makeLoclTypeA v
        | _ -> raise (System.ArgumentException("makeLocType"))

    let stationBoardRequest
        (profile: FsHafas.Endpoint.Profile)
        (``type``: string)
        (name: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (opt: FsHafas.Client.DeparturesArrivalsOptions option)
        : string * FsHafas.Raw.StationBoardRequest =
        let dt = getOptionValue opt (fun v -> v.``when``) Default.DeparturesArrivalsOptions

        let date = formatDate dt
        let time = formatTime dt

        let duration =
            getOptionValue opt (fun v -> v.duration) Default.DeparturesArrivalsOptions

        let stopovers =
            profile.departuresGetPasslist
            && getOptionValue opt (fun v -> v.stopovers) Default.DeparturesArrivalsOptions

        let includeRelatedStations =
            profile.departuresStbFltrEquiv
            && getOptionValue opt (fun v -> v.includeRelatedStations) Default.DeparturesArrivalsOptions

        let products =
            getOptionValue opt (fun v -> v.products) Default.DeparturesArrivalsOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let language =
            getOptionValue opt (fun v -> v.language) Default.DeparturesArrivalsOptions

        language,
        { ``type`` = ``type``
          date = date
          time = time
          stbLoc = makeLocType profile name
          jnyFltrL = filters
          dur = duration }

    let reconstructionRequest
        (profile: FsHafas.Endpoint.Profile)
        (refreshToken: string)
        (opt: FsHafas.Client.RefreshJourneyOptions option)
        : string * FsHafas.Raw.ReconstructionRequest =
        let polylines =
            getOptionValue opt (fun v -> v.polylines) Default.RefreshJourneyOptions

        let stopovers =
            getOptionValue opt (fun v -> v.stopovers) Default.RefreshJourneyOptions

        let tickets = getOptionValue opt (fun v -> v.tickets) Default.RefreshJourneyOptions

        let language =
            getOptionValue opt (fun v -> v.language) Default.RefreshJourneyOptions

        language,
        { getIST = true
          getPasslist = stopovers
          getPolyline = polylines
          getTariff = tickets
          ctxRecon = None
          outReconL = Some [| { ctx = Some refreshToken } |] }

    let journeyMatchRequest
        (profile: FsHafas.Endpoint.Profile)
        (lineName: string)
        (opt: FsHafas.Client.TripsByNameOptions option)
        : string * FsHafas.Raw.JourneyMatchRequest =

        let date =
            maybeGetOptionValue opt (fun v -> v.``when``)
            |> Option.map formatDate

        let language = getOptionValue opt (fun v -> Some "de") Default.TripsByNameOptions

        let filters: FsHafas.Raw.JnyFltr [] =
            match maybeGetOptionValue opt (fun v -> v.operatorNames) with
            | Some operatorNames when operatorNames.Length > 0 ->
                [| { ``type`` = "OP"
                     mode = "INC"
                     value = Some operatorNames.[0]
                     meta = None } |]
            | _ -> [||]

        language,
        { input = lineName
          date = date
          jnyFltrL = filters }

    let locDetailsRequest
        (profile: FsHafas.Endpoint.Profile)
        (stop: U2<string, FsHafas.Client.Stop>)
        (opt: FsHafas.Client.StopOptions option)
        : string * FsHafas.Raw.LocDetailsRequest =

        let id =
            match stop with
            | U2.Case1 s -> s
            | U2.Case2 s when s.id.IsSome -> s.id.Value
            | _ -> raise (System.ArgumentException("Stop expected"))

        let language = getOptionValue opt (fun v -> v.language) Default.StopOptions

        language, { locL = [| makeLocLTypeS profile id |] }

    let locGeoPosRequest
        (profile: FsHafas.Endpoint.Profile)
        (location: FsHafas.Client.Location)
        (opt: FsHafas.Client.NearByOptions option)
        : string * FsHafas.Raw.LocGeoPosRequest =

        let results = getOptionValue opt (fun v -> v.results) Default.NearByOptions

        let stops = getOptionValue opt (fun v -> v.stops) Default.NearByOptions

        let distance = getOptionValue opt (fun v -> v.distance) Default.NearByOptions

        let products = getOptionValue opt (fun v -> v.products) Default.NearByOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let x =
            match location.longitude with
            | Some (f) -> (Coordinate.fromFloat f)
            | None -> raise (System.ArgumentException("location.longitude"))

        let y =
            match location.latitude with
            | Some (f) -> (Coordinate.fromFloat f)
            | None -> raise (System.ArgumentException("location.latitude"))

        let language = getOptionValue opt (fun v -> v.language) Default.NearByOptions

        language,
        { ring =
            { cCrd = { x = x; y = y }
              maxDist = distance
              minDist = 0 }
          locFltrL = filters
          getPOIs = false
          getStops = stops
          maxLoc = results }

    let locGeoReachRequest
        (profile: FsHafas.Endpoint.Profile)
        (location: FsHafas.Client.Location)
        (opt: FsHafas.Client.ReachableFromOptions option)
        : string * FsHafas.Raw.LocGeoReachRequest =

        let dt = getOptionValue opt (fun v -> v.``when``) Default.ReachableFromOptions

        let date = formatDate dt
        let time = formatTime dt

        let maxDuration =
            getOptionValue opt (fun v -> v.maxDuration) Default.ReachableFromOptions

        let maxTransfers =
            getOptionValue opt (fun v -> v.maxTransfers) Default.ReachableFromOptions

        let products = getOptionValue opt (fun v -> v.products) Default.ReachableFromOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let language = getOptionValue opt (fun v -> Some "de") Default.ReachableFromOptions

        language,
        { loc = makeLoclTypeA location
          maxDur = maxDuration
          maxChg = maxTransfers
          date = date
          time = time
          period = 120
          jnyFltrL = filters }

    let journeyGeoPosRequest
        (profile: FsHafas.Endpoint.Profile)
        (rect: FsHafas.Client.BoundingBox)
        (opt: FsHafas.Client.RadarOptions option)
        : string * FsHafas.Raw.JourneyGeoPosRequest =

        if (rect.north <= rect.south) then
            raise (System.ArgumentException("north must be larger than south."))

        if (rect.east <= rect.west) then
            raise (System.ArgumentException("east must be larger than west."))

        let dt = getOptionValue opt (fun v -> v.``when``) Default.RadarOptions

        let date = formatDate dt
        let time = formatTime dt

        let results = getOptionValue opt (fun v -> v.results) Default.RadarOptions

        let duration = getOptionValue opt (fun v -> v.duration) Default.RadarOptions

        let frames = getOptionValue opt (fun v -> v.frames) Default.RadarOptions

        let products = getOptionValue opt (fun v -> v.products) Default.RadarOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let language = getOptionValue opt (fun v -> Some "de") Default.RadarOptions

        language,
        { maxJny = results
          onlyRT = false
          date = date
          time = time
          rect =
            { llCrd =
                { x = Coordinate.fromFloat rect.west
                  y = Coordinate.fromFloat rect.south
                  z = None
                  floor = None }
              urCrd =
                { x = Coordinate.fromFloat rect.east
                  y = Coordinate.fromFloat rect.north
                  z = None
                  floor = None } }
          perSize = duration * 1000
          perStep = duration / frames * 1000
          ageOfReport = true
          jnyFltrL = filters
          trainPosMode = "CALC" }

    let tripRequest
        (profile: FsHafas.Endpoint.Profile)
        (id: string)
        (name: string)
        (opt: FsHafas.Client.TripOptions option)
        : string * FsHafas.Raw.JourneyDetailsRequest =

        let polyline = getOptionValue opt (fun v -> v.polyline) Default.TripOptions

        let language = getOptionValue opt (fun v -> v.language) Default.TripOptions

        language,
        { jid = id
          name = name
          getPolyline = polyline }

    let lineMatchRequest
        (profile: FsHafas.Endpoint.Profile)
        (query: string)
        (opt: FsHafas.Client.LinesOptions option)
        : string * FsHafas.Raw.LineMatchRequest =

        let language = getOptionValue opt (fun v -> v.language) Default.LinesOptions

        language, { input = query }

    let serverInfoRequest
        (profile: FsHafas.Endpoint.Profile)
        (opt: ServerOptions option)
        : string * FsHafas.Raw.ServerInfoRequest =
        let versionInfo = getOptionValue opt (fun v -> v.versionInfo) Default.ServerOptions

        "de", { getVersionInfo = versionInfo }

    let himSearchRequest
        (profile: FsHafas.Endpoint.Profile)
        (opt: FsHafas.Client.RemarksOptions option)
        : string * FsHafas.Raw.HimSearchRequest =

        let dt = getOptionValue opt (fun v -> v.from) Default.RemarksOptions

        let date = formatDate dt
        let time = formatTime dt

        let results = getOptionValue opt (fun v -> v.results) Default.RemarksOptions

        let polylines = getOptionValue opt (fun v -> v.polylines) Default.RemarksOptions

        let products = getOptionValue opt (fun v -> v.products) Default.RemarksOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let language = getOptionValue opt (fun v -> v.language) Default.RemarksOptions

        language,
        { himFltrL = filters
          getPolyline = polylines
          maxNum = results
          dateB = date
          timeB = time }

    let journeyRequest
        (profile: FsHafas.Endpoint.Profile)
        (from: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (``to``: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (opt: FsHafas.Client.JourneysOptions option)
        : string * FsHafas.Raw.TripSearchRequest =

        match opt with
        | Some opt ->
            if
                opt.arrival.IsSome
                && not (profile.journeysOutFrwd)
            then
                raise (System.ArgumentException("opt.arrival is unsupported"))

            if opt.earlierThan.IsSome then
                raise (System.ArgumentException("opt.earlierThan nyi")) // todo

            if opt.laterThan.IsSome then
                raise (System.ArgumentException("opt.laterThan nyi")) // todo
        | None -> ()

        let dt =
            match opt with
            | Some optV when optV.arrival.IsSome -> optV.arrival.Value
            | _ -> getOptionValue opt (fun v -> v.departure) Default.JourneysOptions

        let date = formatDate dt
        let time = formatTime dt

        let outFrwd =
            if opt.IsSome && opt.Value.arrival.IsSome then
                false
            else
                true

        let results = getOptionValue opt (fun v -> v.results) Default.JourneysOptions

        let stopovers = getOptionValue opt (fun v -> v.stopovers) Default.JourneysOptions

        let transfers = getOptionValue opt (fun v -> v.transfers) Default.JourneysOptions

        let transferTime =
            getOptionValue opt (fun v -> v.transferTime) Default.JourneysOptions

        let tickets = getOptionValue opt (fun v -> v.tickets) Default.JourneysOptions

        let polylines = getOptionValue opt (fun v -> v.polylines) Default.JourneysOptions

        let products = getOptionValue opt (fun v -> v.products) Default.JourneysOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let viaLocL: (FsHafas.Raw.LocViaInput array) option =
            match maybeGetOptionValue opt (fun v -> v.via) with
            | Some via -> Some [| { loc = makeLocLTypeS profile via } |]
            | None -> None

        let language = getOptionValue opt (fun v -> v.language) Default.JourneysOptions

        language,
        profile.transformJourneysQuery
            opt
            { getPasslist = stopovers
              maxChg = transfers
              minChgTime = transferTime
              depLocL = [| makeLocType profile from |]
              viaLocL = viaLocL
              arrLocL = [| makeLocType profile ``to`` |]
              jnyFltrL = filters
              gisFltrL = [||]
              getTariff = tickets
              ushrp = true
              getPT = true
              getIV = false
              getPolyline = polylines
              outDate = date
              outTime = time
              numF = results
              outFrwd = outFrwd
              trfReq = None }

    let searchOnTripRequest
        (profile: FsHafas.Endpoint.Profile)
        (tripId: string)
        (previousStopover: StopOver)
        (``to``: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (opt: FsHafas.Client.JourneysFromTripOptions option)
        : string * FsHafas.Raw.SearchOnTripRequest =

        let prevStopId =
            match previousStopover.stop with
            | Some (U2.Case1 station) when station.id.IsSome -> station.id.Value
            | Some (U2.Case2 stop) when stop.id.IsSome -> stop.id.Value
            | _ -> raise (System.ArgumentException("previousStopover.stop must be a valid stop or station."))

        let depAtPrevStop =
            match previousStopover.departure, previousStopover.plannedDeparture with
            | Some dt, _ -> ParseIsoString dt
            | _, Some dt -> ParseIsoString dt
            | _ -> raise (System.ArgumentException("previousStopover.(planned)departure is invalid."))

        (* not necessary for SearchOnTrip
        if depAtPrevStop > System.DateTime.Now then
            raise (System.ArgumentException("previousStopover.(planned)departure must be in the past"))
        *)

        let tickets =
            getOptionValue opt (fun v -> v.tickets) Default.JourneysFromTripOptions

        let transferTime =
            getOptionValue opt (fun v -> v.transferTime) Default.JourneysFromTripOptions

        let polylines =
            getOptionValue opt (fun v -> v.polylines) Default.JourneysFromTripOptions

        let stopovers =
            getOptionValue opt (fun v -> v.stopovers) Default.JourneysFromTripOptions

        let products =
            getOptionValue opt (fun v -> v.products) Default.JourneysFromTripOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let language = "de"

        language,
        { sotMode = "JI"
          jid = tripId
          locData =
            { loc = makeLocLTypeS profile prevStopId
              ``type`` = "DEP"
              date = formatDate depAtPrevStop
              time = formatTime depAtPrevStop }
          arrLocL = [| makeLocType profile ``to`` |]
          jnyFltrL = filters
          getPasslist = stopovers
          getPolyline = polylines
          minChgTime = transferTime
          getTariff = tickets }
