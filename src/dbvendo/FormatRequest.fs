namespace DbVendo.Client

type RefreshTokenWithJourneysOptions =
    { refreshToken: string
      journeysOptions: FsHafas.Client.JourneysOptions }

module internal Coordinate =

    let toFloat (v: int) = float (v) / 1000000.0

    let fromFloat (x: float) = System.Math.Round(x * 1000000.0) |> int

module internal Format =

    open FsHafas.Client
    open DbVendo.Raw

    let private locationFilter (stops: bool) (addresses: bool) (poi: bool) : string array =
        if stops && addresses && poi then
            [| "ALL" |]
        else
            let mutable filter: string array = if stops then [| "ST" |] else [||]

            filter <-
                if addresses then
                    Array.concat [ filter; [| "ADR" |] ]
                else
                    filter

            if poi then Array.concat [ filter; [| "POI" |] ] else filter

    let locationsRequest (query: string) (opt: LocationsOptions option) : Request =
        let stops = Option.getValueD opt (fun v -> v.stops) Default.LocationsOptions
        let addresses = Option.getValueD opt (fun v -> v.addresses) Default.LocationsOptions
        let poi = Option.getValueD opt (fun v -> v.poi) Default.LocationsOptions
        let results = Option.getValueD opt (fun v -> v.results) Default.LocationsOptions

        let body: LocationsRequestBody =
            { locationTypes = locationFilter stops addresses poi
              searchTerm = query
              maxResults = results }

        { endpoint = "https://app.vendo.noncd.db.de/mob/location/search"
          data = RequestData.Post("application/x.db.vendo.mob.location.v3+json", U5.Case1 body)
          xCorrelationID = $"{System.Guid.NewGuid()}_{System.Guid.NewGuid()}"
          accept = "application/x.db.vendo.mob.location.v3+json" }

    let private makeLocLTypeS (id: string) : string =
        let k =
            if System.Text.RegularExpressions.Regex.IsMatch(id, @"^\d{6,}$") then
                "L"
            else
                "O"

        $"A=1@" + k + "=" + id + "@"

    let private makeLoclTypeA (location: FsHafas.Client.Location) : string =
        let x =
            match location.longitude with
            | Some(f) -> (Coordinate.fromFloat f)
            | None -> raise (System.ArgumentException("location.longitude"))

        let y =
            match location.latitude with
            | Some(f) -> (Coordinate.fromFloat f)
            | None -> raise (System.ArgumentException("location.latitude"))

        let xs = x.ToString()
        let ys = y.ToString()

        match location.address with
        | Some name -> "A=2@O=" + name + "@X=" + xs + "@Y=" + ys + "@"
        | None -> "A=1@X=" + xs + "@Y=" + ys + "@"

    let private makeLocType (s: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>) =
        match s with
        | U4.Case1 v -> makeLocLTypeS v
        | U4.Case2 v when v.id.IsSome -> makeLocLTypeS v.id.Value
        | U4.Case3 v when v.id.IsSome -> makeLocLTypeS v.id.Value
        | U4.Case4 v when v.id.IsSome -> makeLocLTypeS v.id.Value
        | U4.Case4 v -> makeLoclTypeA v
        | _ -> raise (System.ArgumentException("makeLocType"))

    let private fromatDateTime (dateTime: System.DateTime) : System.DateTime =
        // remove milliseconds
        let dateTime =
            System.DateTime(dateTime.Ticks - (dateTime.Ticks % System.TimeSpan.TicksPerSecond), dateTime.Kind)

        dateTime

    let toReisendenTyp (ageGroup: AgeGroup option) (age: int option) : string =
        let ageGroup' =
            match ageGroup, age with
            | Some g, _ -> g
            | None, Some age when age < 6 -> B
            | None, Some age when age < 15 -> K
            | None, Some age when age < 27 -> Y
            | None, Some age when age < 65 -> E
            | None, Some _ -> S
            | _, _ -> E

        match ageGroup' with
        | B -> "KLEINKIND"
        | K -> "FAMILIENKIND"
        | Y -> "JUGENDLICHER"
        | E -> "ERWACHSENER"
        | S -> "SENIOR"

    let toErmaessigungen (loyaltyCard: LoyaltyCard option) : string array =
        let defaultErmaessigungen = [| "KEINE_ERMAESSIGUNG KLASSENLOS" |]

        match loyaltyCard with
        | Some { ``type`` = "Bahncard"
                 discount = Some discount
                 ``class`` = ``class`` } when discount = 25 || discount = 50 ->
            let klasse =
                if Option.getD ``class`` 2 = 1 then
                    "KLASSE_1"
                else
                    "KLASSE_2"

            [| $"BAHNCARD{discount} {klasse}" |]
        | _ -> defaultErmaessigungen

    let private getKlasse (opt: JourneysOptions option) : string =
        let firstClass =
            Option.getValueD opt (fun v -> v.firstClass) Default.JourneysOptions

        if firstClass then "KLASSE_1" else "KLASSE_2"

    let private getReisendenProfil (opt: JourneysOptions option) : ReisendenProfil =
        { reisende =
            [| { ermaessigungen = toErmaessigungen (opt |> Option.bind (fun opt -> opt.loyaltyCard))
                 reisendenTyp =
                   toReisendenTyp (opt |> Option.bind (fun v -> v.ageGroup)) (opt |> Option.bind (fun v -> v.age)) } |] }

    let journeysRequest
        (from: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (``to``: U4<string, FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>)
        (opt: JourneysOptions option)
        : Request =

        let departure = Option.getValueD opt (fun v -> v.departure) Default.JourneysOptions
        let products = Option.getValueD opt (fun v -> v.products) Default.JourneysOptions

        let transferTime =
            Option.getValueD opt (fun v -> v.transferTime) Default.JourneysOptions

        let transfers = Option.getValueD opt (fun v -> v.transfers) Default.JourneysOptions
        let transfers = if transfers >= 0 then transfers else 10

        let wunsch: Wunsch =
            { abgangsLocationId = makeLocType from
              verkehrsmittel = Products.fllterProducts products
              zeitWunsch =
                { reiseDatum = fromatDateTime (departure)
                  zeitPunktArt = "ABFAHRT" }
              zielLocationId = makeLocType ``to``
              minUmstiegsdauer = Some transferTime
              maxUmstiege = Some transfers
              fahrradmitnahme = false }

        let body: JourneysRequestBody =
            { autonomeReservierung = false
              einstiegsTypList = [| "STANDARD" |]
              fahrverguenstigungen =
                { deutschlandTicketVorhanden = false
                  nurDeutschlandTicketVerbindungen = false }
              klasse = getKlasse opt
              reisendenProfil = getReisendenProfil opt
              reservierungsKontingenteVorhanden = false
              reiseHin = { wunsch = wunsch } }

        { endpoint = "https://app.vendo.noncd.db.de/mob/angebote/fahrplan"
          data = RequestData.Post("application/x.db.vendo.mob.verbindungssuche.v8+json", U5.Case2 body)
          xCorrelationID = $"{System.Guid.NewGuid()}_{System.Guid.NewGuid()}"
          accept = "application/x.db.vendo.mob.verbindungssuche.v8+json" }

    // refreshToken may contain encoded JourneysOptions, see Parser.Journey.parseJourney
    let splitRefreshToken (refreshToken: string) : string * (JourneysOptions option) =
        if refreshToken.StartsWith "{" then
            let r =
                DbVendo.Api.Serializer.Deserialize<RefreshTokenWithJourneysOptions> refreshToken

            r.refreshToken, Some r.journeysOptions
        else
            refreshToken, None

    let refreshJourneysRequest
        (refreshToken: string)
        (_: RefreshJourneyOptions option)
        (journeysOptions: JourneysOptions option)
        : Request =
        let body: RefreshJourneysRequestBody =
            { autonomeReservierung = false
              einstiegsTypList = [| "STANDARD" |]
              fahrverguenstigungen =
                { deutschlandTicketVorhanden = false
                  nurDeutschlandTicketVerbindungen = false }
              klasse = getKlasse journeysOptions
              reisendenProfil = getReisendenProfil journeysOptions
              reservierungsKontingenteVorhanden = false
              verbindungHin = { kontext = refreshToken } }

        { endpoint = "https://app.vendo.noncd.db.de/mob/angebote/recon"
          data = RequestData.Post("application/x.db.vendo.mob.verbindungssuche.v8+json", U5.Case3 body)
          xCorrelationID = $"{System.Guid.NewGuid()}_{System.Guid.NewGuid()}"
          accept = "application/x.db.vendo.mob.verbindungssuche.v8+json" }

    let tripRequest (tripid: string) (opt: TripOptions option) : Request =
        { endpoint = "https://app.vendo.noncd.db.de/mob/zuglauf/"
          data = RequestData.Get tripid
          xCorrelationID = $"{System.Guid.NewGuid()}_{System.Guid.NewGuid()}"
          accept = "application/x.db.vendo.mob.zuglauf.v2+json" }

    let stopRequest (stop: U2<string, Stop>) (opt: StopOptions option) : Request =
        let stopId =
            match stop with
            | U2.Case1 id -> id
            | U2.Case2 stop -> Option.getD stop.id ""

        { endpoint = "https://app.vendo.noncd.db.de/mob/location/details/"
          data = RequestData.Get stopId
          xCorrelationID = $"{System.Guid.NewGuid()}_{System.Guid.NewGuid()}"
          accept = "application/x.db.vendo.mob.location.v3+json" }

    let stationBoardRequest
        (name: U4<string, Station, Stop, FsHafas.Client.Location>)
        (isDeparture: bool)
        (opt: DeparturesArrivalsOptions option)
        : Request =
        let date =
            Option.getValueD opt (fun v -> v.``when``) Default.DeparturesArrivalsOptions

        let body: StationBoardRequestBody =
            { anfragezeit = date.ToString("HH:mm")
              datum = date.ToString("yyyy-MM-dd")
              ursprungsBahnhofId = makeLocType name
              verkehrsmittel = [| "ALL" |] }

        { endpoint =
            "https://app.vendo.noncd.db.de/mob/bahnhofstafel/"
            + (if isDeparture then "abfahrt" else "ankunft")
          data = RequestData.Post("application/x.db.vendo.mob.bahnhofstafeln.v2+json", U5.Case4 body)
          xCorrelationID = $"{System.Guid.NewGuid()}_{System.Guid.NewGuid()}"
          accept = "application/x.db.vendo.mob.bahnhofstafeln.v2+json" }

    let nearByRequest (loc: FsHafas.Client.Location) (opt: NearByOptions option) : Request =
        let results = Option.getValueD opt (fun v -> v.results) Default.NearByOptions

        let radius = Option.getValueD opt (fun v -> v.distance) Default.NearByOptions
        let radius = if radius < 0 then 1000 else radius

        let body: NearByRequestBody =
            { area =
                { coordinates =
                    { latitude = Option.getD loc.latitude 0.0
                      longitude = Option.getD loc.longitude 0.0 }
                  radius = radius }
              maxResults = results
              products = [| "ALL" |] }

        { endpoint = "https://app.vendo.noncd.db.de/mob/location/nearby"
          data = RequestData.Post("application/x.db.vendo.mob.location.v3+json", U5.Case5 body)
          xCorrelationID = $"{System.Guid.NewGuid()}_{System.Guid.NewGuid()}"
          accept = "application/x.db.vendo.mob.location.v3+json" }
