namespace HafasClient

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

    [<Import("profile as dbProfile", from = "hafas-client/p/db/index.js")>]
    let dbProfile: Profile = jsNative

    [<Import("profile as bvgProfile", from = "hafas-client/p/bvg/index.js")>]
    let bvgProfile: Profile = jsNative

    [<Import("profile as oebbProfile", from = "hafas-client/p/oebb/index.js")>]
    let oebbProfile: Profile = jsNative

    [<Import("profile as irishrailProfile", from = "hafas-client/p/irish-rail/index.js")>]
    let irishrailProfile: Profile = jsNative

    [<Import("profile as rejseplanenProfile", from = "hafas-client/p/rejseplanen/index.js")>]
    let rejseplanenProfile: Profile = jsNative

    [<Import("createClient as _createClient", from = "hafas-client")>]
    [<Emit("_createClient($1, 'agent')")>]
    let private _createClient (_: Profile) : HafasClient = jsNative

    [<Emit("""{ 
            const _journeys = $0.journeys;
            function journeys(f, t, opt) { 
                $1.forEach(property => { if (property in opt && !opt[property]) { delete opt[property]; } });
                $2.forEach(property => { if (property in opt) { delete opt[property]; } }); 
                return _journeys(f, t, opt); 
            } 
            $0.journeys = journeys; 
    }""")>]
    let private deleteJourneysProperties (_: HafasClient, _: string [], _: string []) : unit = jsNative

    [<Emit("""{ 
            const _departures = $0.departures;
            function departures(s, opt) { 
                $1.forEach(property => { if (property in opt && !opt[property]) { delete opt[property]; } });
                $2.forEach(property => { if (property in opt) { delete opt[property]; } }); 
                return _departures(s, opt); 
            } 
            $0.departures = departures; 
    }""")>]
    let private deleteDeparturesProperties (_: HafasClient, _: string [], _: string []) : unit = jsNative

    [<Emit("""{ 
            const _arrivals = $0.arrivals;
            function arrivals(s, opt) { 
                $1.forEach(property => { if (property in opt && !opt[property]) { delete opt[property]; } });
                $2.forEach(property => { if (property in opt) { delete opt[property]; } }); 
                return _arrivals(s, opt); 
            } 
            $0.arrivals = arrivals; 
    }""")>]
    let private deleteArrivalsProperties (_: HafasClient, _: string [], _: string []) : unit = jsNative

    let createClient (profile: Profile) : HafasClient =
        let client = _createClient profile

        // see https://github.com/public-transport/hafas-client/blob/9f85a9af54c95eed91ce04c7e7fdaabbab30c8f5/index.js#L101
        deleteJourneysProperties (
            client,
            [| "departure"
               "arrival"
               "earlierThan"
               "laterThan"
               "age" |],
            [||]
        )

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
