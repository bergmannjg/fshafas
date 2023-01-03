namespace HafasClient

module Api =

    open Fable.Core
    open Types

    [<Import("profile as dbProfile", from = "hafas-client/p/db/index.js")>]
    let dbProfile: Profile = jsNative

    [<Import("profile as bvgProfile", from = "hafas-client/p/bvg/index.js")>]
    let bvgProfile: Profile = jsNative

    [<Import("createClient as _createClient", from = "hafas-client")>]
    [<Emit("_createClient($1, 'agent')")>]
    let private _createClient (_: Profile) : HafasClient = jsNative

    /// enforce mutually exclusive properties
    [<Emit("""
        (function checkJourneysOption(client, properties) { 
            function checkOption(opt, properties) { 
                properties.forEach(property => { if (property in opt && !opt[property]) { delete opt[property]; } }); 
                return opt;
            } 
            const __journeys = client.journeys; 
            function _journeys(f, t, opt) { return __journeys(f, t, checkOption(opt, properties)); } 
            client.journeys = _journeys; }
        )($0, $1)
    """)>]
    let private checkJourneysOption (_: HafasClient, _: string []) : unit = jsNative

    let createClient (profile: Profile) : HafasClient =
        let client = _createClient profile

        checkJourneysOption (
            client,
            [| "departure"
               "arrival"
               "earlierThan"
               "laterThan"
               "age" |]
        )

        client

    let getProfile (p: string) =
        match p with
        | "db" -> dbProfile
        | "bvg" -> bvgProfile
        | _ -> raise (System.Exception("profile expected: db | bvg"))
