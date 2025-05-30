namespace HafasClient

module Defaults =

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
          products = None
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
          age = None
          ageGroup = None
          loyaltyCard = None
          routingMode = None
          ``when`` = None
          generateUnreliableTicketUrls = None }

    let DeparturesArrivalsOptions: DeparturesArrivalsOptions =
        { ``when`` = Some System.DateTime.Now
          direction = None
          line = None
          duration = Some 10
          results = None
          subStops = None
          entrances = None
          linesOfStops = Some false
          remarks = None
          stopovers = Some false
          includeRelatedStations = Some false
          products = None
          language = Some "de" }

    let ReachableFromOptions: ReachableFromOptions =
        { ``when`` = Some System.DateTime.Now
          maxTransfers = Some 5
          maxDuration = Some 10
          products = None
          subStops = None
          entrances = None
          polylines = None }

    let RadarOptions: RadarOptions =
        { results = Some 256
          frames = Some 3
          products = None
          duration = Some 30
          subStops = Some true
          entrances = Some true
          polylines = Some true
          ``when`` = Some System.DateTime.Now }

    let Location: Location =
        { ``type`` = LocationType.Location
          id = None
          name = None
          poi = None
          address = Some "unused"
          longitude = None
          latitude = None
          altitude = None
          distance = None }

module Api =

    open Fable.Core
    open Fable.Core.JsInterop

    // todo: add more profiles
    [<StringEnum>]
    type ProfileId =
        | Bvg
        | Db
        | Irishrail
        | Oebb
        | Rejseplanen

    let private endpoint = "https://app.vendo.noncd.db.de"

    // dummy profile to use db vendo client with dbnav profile
    let dbProfile: Profile =
        { new Profile with
            member _.locale = ""
            member _.timezone = ""
            member _.endpoint = endpoint
            member _.products = [||]
            member _.trip = None
            member _.radar = None
            member _.refreshJourney = None
            member _.reachableFrom = None
            member _.journeysWalkingSpeed = None
            member _.tripsByName = None
            member _.remarks = None
            member _.journeysFromTrip = None
            member _.remarksGetPolyline = None
            member _.lines = None }

    [<Import("profile as dbnavProfile", from = "db-vendo-client/p/dbnav/index.js")>]
    let private dbnavProfile: obj = jsNative

    [<Import("createClient as createDbVendoClient", from = "db-vendo-client")>]
    [<Emit("createDbVendoClient($1, 'db-vendo-client bindings')")>]
    let private createDbVendoClient (_: obj) : HafasClient = jsNative

    [<Import("profile as bvgProfile", from = "hafas-client/p/bvg/index.js")>]
    let bvgProfile: Profile = jsNative

    [<Import("profile as oebbProfile", from = "hafas-client/p/oebb/index.js")>]
    let oebbProfile: Profile = jsNative

    [<Import("profile as irishrailProfile", from = "hafas-client/p/irish-rail/index.js")>]
    let irishrailProfile: Profile = jsNative

    [<Import("profile as rejseplanenProfile", from = "hafas-client/p/rejseplanen/index.js")>]
    let rejseplanenProfile: Profile = jsNative

    [<Import("createClient as createHafasClient", from = "hafas-client")>]
    [<Emit("createHafasClient($1, 'agent')")>]
    let private createHafasClient (_: Profile) : HafasClient = jsNative

    [<Emit("""function deleteProperties (opt, arr1, arr2) { 
            arr1.forEach(property => { if (property in opt && !opt[property]) { delete opt[property]; } });
            arr2.forEach(property => { if (property in opt) { delete opt[property]; } }); 
            return opt;
    }""")>]
    let private deleteProperties () : unit = jsNative

    deleteProperties ()

    [<Emit("""{ 
        const _journeys = $0.journeys;
        function journeys(f, t, opt) { return _journeys(f, t, (deleteProperties(opt, $1, $2))); } 
        $0.journeys = journeys; 
    }""")>]
    let private deleteJourneysProperties (_: HafasClient, _: string[], _: string[]) : unit = jsNative

    [<Emit("""{ 
        const _departures = $0.departures;
        function departures(s, opt) { return _departures(s, (deleteProperties(opt, $1, $2))); } 
        $0.departures = departures; 
    }""")>]
    let private deleteDeparturesProperties (_: HafasClient, _: string[], _: string[]) : unit = jsNative

    [<Emit("""{ 
        const _arrivals = $0.arrivals;
        function arrivals(s, opt) { return _arrivals(s, (deleteProperties(opt, $1, $2))); } 
        $0.arrivals = arrivals; 
    }""")>]
    let private deleteArrivalsProperties (_: HafasClient, _: string[], _: string[]) : unit = jsNative

    let createClient (profile: Profile) : HafasClient =
        let client =
            if profile.endpoint = endpoint then
                createDbVendoClient dbnavProfile
            else
                createHafasClient profile

        // see https://github.com/public-transport/hafas-client/blob/9f85a9af54c95eed91ce04c7e7fdaabbab30c8f5/index.js#L101
        deleteJourneysProperties (client, [| "departure"; "arrival"; "earlierThan"; "laterThan"; "age" |], [||])

        // see https://github.com/public-transport/hafas-client/blob/8278ff9c621f3d0671d7e109f15ab1c89fabfc0e/index.js#L48
        let departurearrivalproperties =
            seq {
                if not profile?departuresGetPasslist then
                    yield "stopovers"

                if not profile?departuresStbFltrEquiv then
                    yield "includeRelatedStations"
            }
            |> Seq.toArray

        deleteDeparturesProperties (client, [||], departurearrivalproperties)
        deleteArrivalsProperties (client, [||], departurearrivalproperties)

        client

    let getProfile (p: ProfileId) =
        match p with
        | Bvg -> bvgProfile
        | Db -> dbProfile
        | Irishrail -> irishrailProfile
        | Oebb -> oebbProfile
        | Rejseplanen -> rejseplanenProfile
