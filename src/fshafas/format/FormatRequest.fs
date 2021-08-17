namespace FsHafas.Client

module internal Format =

#if FABLE_COMPILER
    open Fable.Core
#endif

    let private ParseIsoString (datetime: string) =
        let year = datetime.Substring(0, 4) |> int
        let month = datetime.Substring(5, 2) |> int
        let day = datetime.Substring(8, 2) |> int
        let hour = datetime.Substring(11, 2) |> int
        let minute = datetime.Substring(14, 2) |> int

        let tzOffset =
            datetime.Substring(20, 2) |> int |> (*) 60

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

    let formatDate (dt: System.DateTime) = dt.ToString("yyyyMMdd")
    let formatTime (dt: System.DateTime) = dt.ToString("HHmmss")

    let private formatProductsBitmask (profile: FsHafas.Parser.Profile) (products: FsHafas.Client.Products) =
        profile.products
        |> Array.filter (fun p -> products.[p.id])
        |> Array.fold (fun bitmask p -> p.bitmasks.[0] ||| bitmask) 0

    let private makeFilters (profile: FsHafas.Parser.Profile) (products: FsHafas.Client.Products) =
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
        (profile: FsHafas.Parser.Profile)
        (name: string)
        (opt: FsHafas.Client.LocationsOptions option)
        : FsHafas.Raw.LocMatchRequest =
        let fuzzy =
            getOptionValue opt (fun v -> v.fuzzy) Default.LocationsOptions

        let results =
            getOptionValue opt (fun v -> v.results) Default.LocationsOptions

        { input =
              { loc =
                    { ``type`` = "ALL"
                      name = Some(name + (if fuzzy then "?" else ""))
                      lid = None }
                maxLoc = results
                field = "S" } }

    let private makeLocLTypeS (profile: FsHafas.Parser.Profile) (id: string) : FsHafas.Raw.Loc =
        { ``type`` = "S"
          name = None
          lid = Some("A=1@L=" + (profile.formatStation id) + "@") }

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

    let stationBoardRequest
        (profile: FsHafas.Parser.Profile)
        (``type``: string)
        (name: string)
        (opt: FsHafas.Client.DeparturesArrivalsOptions option)
        : FsHafas.Raw.StationBoardRequest =
        let dt =
            getOptionValue opt (fun v -> v.``when``) Default.DeparturesArrivalsOptions

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

        { ``type`` = ``type``
          date = date
          time = time
          stbLoc = makeLocLTypeS profile name
          jnyFltrL = filters
          dur = duration
          getPasslist = stopovers
          stbFltrEquiv = includeRelatedStations }

    let reconstructionRequest
        (profile: FsHafas.Parser.Profile)
        (refreshToken: string)
        (opt: FsHafas.Client.RefreshJourneyOptions option)
        : FsHafas.Raw.ReconstructionRequest =
        let polylines =
            getOptionValue opt (fun v -> v.polylines) Default.RefreshJourneyOptions

        let stopovers =
            getOptionValue opt (fun v -> v.stopovers) Default.RefreshJourneyOptions

        let tickets =
            getOptionValue opt (fun v -> v.tickets) Default.RefreshJourneyOptions

        { getIST = true
          getPasslist = stopovers
          getPolyline = polylines
          getTariff = tickets
          ctxRecon = Some refreshToken }

    let journeyMatchRequest
        (profile: FsHafas.Parser.Profile)
        (lineName: string)
        (opt: FsHafas.Client.TripsByNameOptions option)
        : FsHafas.Raw.JourneyMatchRequest =

        let date =
            maybeGetOptionValue opt (fun v -> v.``when``)
            |> Option.map formatDate

        { input = lineName; date = date }

    let locDetailsRequest
        (profile: FsHafas.Parser.Profile)
        (stop: U2<string, FsHafas.Client.Stop>)
        (opt: FsHafas.Client.StopOptions option)
        : FsHafas.Raw.LocDetailsRequest =

        let id =
            match stop with
            | U2.Case1 s -> s
            | U2.Case2 s when s.id.IsSome -> s.id.Value
            | _ -> raise (System.ArgumentException("Stop expected"))

        { locL = [| makeLocLTypeS profile id |] }

    let locGeoPosRequest
        (profile: FsHafas.Parser.Profile)
        (location: FsHafas.Client.Location)
        (opt: FsHafas.Client.NearByOptions option)
        : FsHafas.Raw.LocGeoPosRequest =

        let results =
            getOptionValue opt (fun v -> v.results) Default.NearByOptions

        let stops =
            getOptionValue opt (fun v -> v.stops) Default.NearByOptions

        let distance =
            getOptionValue opt (fun v -> v.distance) Default.NearByOptions

        let products =
            getOptionValue opt (fun v -> v.products) Default.NearByOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let x =
            match location.longitude with
            | Some (f) -> (Coordinate.fromFloat f)
            | None -> raise (System.ArgumentException("location.longitude"))

        let y =
            match location.latitude with
            | Some (f) -> (Coordinate.fromFloat f)
            | None -> raise (System.ArgumentException("location.latitude"))

        { ring =
              { cCrd = { x = x; y = y }
                maxDist = distance
                minDist = 0 }
          locFltrL = filters
          getPOIs = false
          getStops = stops
          maxLoc = results }

    let locGeoReachRequest
        (profile: FsHafas.Parser.Profile)
        (location: FsHafas.Client.Location)
        (opt: FsHafas.Client.ReachableFromOptions option)
        : FsHafas.Raw.LocGeoReachRequest =

        let dt =
            getOptionValue opt (fun v -> v.``when``) Default.ReachableFromOptions

        let date = formatDate dt
        let time = formatTime dt

        let maxDuration =
            getOptionValue opt (fun v -> v.maxDuration) Default.ReachableFromOptions

        let maxTransfers =
            getOptionValue opt (fun v -> v.maxTransfers) Default.ReachableFromOptions

        let products =
            getOptionValue opt (fun v -> v.products) Default.ReachableFromOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        { loc = makeLoclTypeA location
          maxDur = maxDuration
          maxChg = maxTransfers
          date = date
          time = time
          period = 120
          jnyFltrL = filters }

    let journeyGeoPosRequest
        (profile: FsHafas.Parser.Profile)
        (rect: FsHafas.Client.BoundingBox)
        (opt: FsHafas.Client.RadarOptions option)
        : FsHafas.Raw.JourneyGeoPosRequest =

        if (rect.north <= rect.south) then
            raise (System.ArgumentException("north must be larger than south."))

        if (rect.east <= rect.west) then
            raise (System.ArgumentException("east must be larger than west."))

        let dt =
            getOptionValue opt (fun v -> v.``when``) Default.RadarOptions

        let date = formatDate dt
        let time = formatTime dt

        let results =
            getOptionValue opt (fun v -> v.results) Default.RadarOptions

        let duration =
            getOptionValue opt (fun v -> v.duration) Default.RadarOptions

        let frames =
            getOptionValue opt (fun v -> v.frames) Default.RadarOptions

        let products =
            getOptionValue opt (fun v -> v.products) Default.RadarOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        { maxJny = results
          onlyRT = false
          date = date
          time = time
          rect =
              { llCrd =
                    { x = Coordinate.fromFloat rect.west
                      y = Coordinate.fromFloat rect.south
                      z = None
                      ``type`` = None
                      layerX = None
                      crdSysX = None }
                urCrd =
                    { x = Coordinate.fromFloat rect.east
                      y = Coordinate.fromFloat rect.north
                      z = None
                      ``type`` = None
                      layerX = None
                      crdSysX = None } }
          perSize = duration * 1000
          perStep = duration / frames * 1000
          ageOfReport = true
          jnyFltrL = filters
          trainPosMode = "CALC" }

    let tripRequest
        (profile: FsHafas.Parser.Profile)
        (id: string)
        (name: string)
        (opt: FsHafas.Client.TripOptions option)
        : FsHafas.Raw.JourneyDetailsRequest =

        let polyline =
            getOptionValue opt (fun v -> v.polyline) Default.TripOptions

        { jid = id
          name = name
          getPolyline = polyline }

    let lineMatchRequest
        (profile: FsHafas.Parser.Profile)
        (query: string)
        (opt: FsHafas.Client.LinesOptions option)
        : FsHafas.Raw.LineMatchRequest =
        { input = query }

    let himSearchRequest
        (profile: FsHafas.Parser.Profile)
        (opt: FsHafas.Client.RemarksOptions option)
        : FsHafas.Raw.HimSearchRequest =

        let dt =
            getOptionValue opt (fun v -> v.from) Default.RemarksOptions

        let date = formatDate dt
        let time = formatTime dt

        let results =
            getOptionValue opt (fun v -> v.results) Default.RemarksOptions

        let polylines =
            getOptionValue opt (fun v -> v.polylines) Default.RemarksOptions

        let products =
            getOptionValue opt (fun v -> v.products) Default.RemarksOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        { himFltrL = filters
          getPolyline = polylines
          maxNum = results
          dateB = date
          timeB = time }

    let private makeLocType
        (profile: FsHafas.Parser.Profile)
        (s: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        =
        match s with
        | U4.Case1 v -> makeLocLTypeS profile v
        | U4.Case2 v when v.id.IsSome -> makeLocLTypeS profile v.id.Value
        | U4.Case3 v when v.id.IsSome -> makeLocLTypeS profile v.id.Value
        | U4.Case4 v -> makeLoclTypeA v
        | _ -> raise (System.ArgumentException("makeLocType"))

    let journeyRequest
        (profile: FsHafas.Parser.Profile)
        (from: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (``to``: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (opt: FsHafas.Client.JourneysOptions option)
        : FsHafas.Raw.TripSearchRequest =

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

        let results =
            getOptionValue opt (fun v -> v.results) Default.JourneysOptions

        let stopovers =
            getOptionValue opt (fun v -> v.stopovers) Default.JourneysOptions

        let transferTime =
            getOptionValue opt (fun v -> v.transferTime) Default.JourneysOptions

        let tickets =
            getOptionValue opt (fun v -> v.tickets) Default.JourneysOptions

        let polylines =
            getOptionValue opt (fun v -> v.polylines) Default.JourneysOptions

        let products =
            getOptionValue opt (fun v -> v.products) Default.JourneysOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        let viaLocL: (FsHafas.Raw.LocViaInput array) option =
            match maybeGetOptionValue opt (fun v -> v.via) with
            | Some via -> Some [| { loc = makeLocLTypeS profile via } |]
            | None -> None

        profile.transformJourneysQuery
            opt
            { getPasslist = stopovers
              maxChg = -1
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
        (profile: FsHafas.Parser.Profile)
        (tripId: string)
        (previousStopover: StopOver)
        (``to``: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (opt: FsHafas.Client.JourneysFromTripOptions option)
        : FsHafas.Raw.SearchOnTripRequest =

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

        let products =
            getOptionValue opt (fun v -> v.products) Default.JourneysFromTripOptions

        let filters: FsHafas.Raw.JnyFltr [] = makeFilters profile products

        { sotMode = "JI"
          jid = tripId
          locData =
              { loc = makeLocLTypeS profile prevStopId
                ``type`` = "DEP"
                date = formatDate depAtPrevStop
                time = formatTime depAtPrevStop }
          arrLocL = [| makeLocType profile ``to`` |]
          jnyFltrL = filters
          getPasslist = false
          getPolyline = polylines
          minChgTime = transferTime
          getTariff = tickets }
