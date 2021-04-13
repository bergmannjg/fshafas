namespace FsHafas.Profiles

/// <exclude>Db</exclude>
module Db =

    open FsHafas.Client
    open System.Text.RegularExpressions

#if FABLE_COMPILER
    open Fable.Core
#endif

    let private products : ProductType [] =
        [| { id = "nationalExpress"
             mode = ProductTypeMode.Train
             bitmasks = [| 1 |]
             name = "InterCityExpress"
             short = "ICE"
             ``default`` = true }
           { id = "national"
             mode = ProductTypeMode.Train
             bitmasks = [| 2 |]
             name = "InterCity & EuroCity"
             short = "IC/EC"
             ``default`` = true }
           { id = "regionalExp"
             mode = ProductTypeMode.Train
             bitmasks = [| 4 |]
             name = "RegionalExpress & InterRegio"
             short = "RE/IR"
             ``default`` = true }
           { id = "regional"
             mode = ProductTypeMode.Train
             bitmasks = [| 8 |]
             name = "Regio"
             short = "RB"
             ``default`` = true }
           { id = "suburban"
             mode = ProductTypeMode.Train
             bitmasks = [| 16 |]
             name = "S-Bahn"
             short = "S"
             ``default`` = true }
           { id = "bus"
             mode = ProductTypeMode.Bus
             bitmasks = [| 32 |]
             name = "Bus"
             short = "B"
             ``default`` = true }
           { id = "ferry"
             mode = ProductTypeMode.Watercraft
             bitmasks = [| 64 |]
             name = "Ferry"
             short = "F"
             ``default`` = true }
           { id = "subway"
             mode = ProductTypeMode.Train
             bitmasks = [| 128 |]
             name = "U-Bahn"
             short = "U"
             ``default`` = true }
           { id = "tram"
             mode = ProductTypeMode.Train
             bitmasks = [| 256 |]
             name = "Tram"
             short = "T"
             ``default`` = true }
           { id = "taxi"
             mode = ProductTypeMode.Taxi
             bitmasks = [| 512 |]
             name = "Group Taxi"
             short = "Taxi"
             ``default`` = true } |]

    type private HintByCode =
        { ``type``: string
          code: string
          summary: string }

    let private hintsByCode =
        [| ("fb",
            { ``type`` = "hint"
              code = "bicycle-conveyance"
              summary = "bicycles conveyed" })
           ("fr",
            { ``type`` = "hint"
              code = "bicycle-conveyance-reservation"
              summary = "bicycles conveyed, subject to reservation" })

           ("nf",
            { ``type`` = "hint"
              code = "no-bicycle-conveyance"
              summary = "bicycles not conveyed" })
           ("k2",
            { ``type`` = "hint"
              code = "2nd-class-only"
              summary = "2. class only" })
           ("eh",
            { ``type`` = "hint"
              code = "boarding-ramp"
              summary = "vehicle-mounted boarding ramp available" })
           ("ro",
            { ``type`` = "hint"
              code = "wheelchairs-space"
              summary = "space for wheelchairs" })
           ("oa",
            { ``type`` = "hint"
              code = "wheelchairs-space-reservation"
              summary = "space for wheelchairs, subject to reservation" })
           ("wv",
            { ``type`` = "hint"
              code = "wifi"
              summary = "WiFi available" })
           ("wi",
            { ``type`` = "hint"
              code = "wifi"
              summary = "WiFi available" })
           ("sn",
            { ``type`` = "hint"
              code = "snacks"
              summary = "snacks available for purchase" })
           ("mb",
            { ``type`` = "hint"
              code = "snacks"
              summary = "snacks available for purchase" })
           ("mp",
            { ``type`` = "hint"
              code = "snacks"
              summary = "snacks available for purchase at the seat" })
           ("bf",
            { ``type`` = "hint"
              code = "barrier-free"
              summary = "barrier-free" })
           ("rg",
            { ``type`` = "hint"
              code = "barrier-free-vehicle"
              summary = "barrier-free vehicle" })
           ("bt",
            { ``type`` = "hint"
              code = "on-board-bistro"
              summary = "Bordbistro available" })
           ("br",
            { ``type`` = "hint"
              code = "on-board-restaurant"
              summary = "Bordrestaurant available" })
           ("ki",
            { ``type`` = "hint"
              code = "childrens-area"
              summary = "children's area available" })
           ("kk",
            { ``type`` = "hint"
              code = "parents-childrens-compartment"
              summary = "parent-and-children compartment available" })
           ("kr",
            { ``type`` = "hint"
              code = "kids-service"
              summary = "DB Kids Service available" })
           ("ls",
            { ``type`` = "hint"
              code = "power-sockets"
              summary = "power sockets available" })
           ("ev",
            { ``type`` = "hint"
              code = "replacement-service"
              summary = "replacement service" })
           ("kl",
            { ``type`` = "hint"
              code = "air-conditioned"
              summary = "air-conditioned vehicle" })
           ("r0",
            { ``type`` = "hint"
              code = "upward-escalator"
              summary = "upward escalator" })
           ("au",
            { ``type`` = "hint"
              code = "elevator"
              summary = "elevator available" })
           ("ck",
            { ``type`` = "hint"
              code = "komfort-checkin"
              summary = "Komfort-Checkin available" })
           ("it",
            { ``type`` = "hint"
              code = "ice-sprinter"
              summary = "ICE Sprinter service" })
           ("rp",
            { ``type`` = "hint"
              code = "compulsory-reservation"
              summary = "compulsory seat reservation" })
           ("rm",
            { ``type`` = "hint"
              code = "optional-reservation"
              summary = "optional seat reservation" })
           ("scl",
            { ``type`` = "hint"
              code = "all-2nd-class-seats-reserved"
              summary = "all 2nd class seats reserved" })
           ("cacl",
            { ``type`` = "hint"
              code = "all-seats-reserved"
              summary = "all seats reserved" })
           ("sk",
            { ``type`` = "hint"
              code = "oversize-luggage-forbidden"
              summary = "oversize luggage not allowed" })
           ("hu",
            { ``type`` = "hint"
              code = "animals-forbidden"
              summary = "animals not allowed, except guide dogs" })
           ("ik",
            { ``type`` = "hint"
              code = "baby-cot-required"
              summary = "baby cot/child seat required" })
           ("ee",
            { ``type`` = "hint"
              code = "on-board-entertainment"
              summary = "on-board entertainment available" })
           ("toilet",
            { ``type`` = "hint"
              code = "toilet"
              summary = "toilet available" })
           ("oc",
            { ``type`` = "hint"
              code = "wheelchair-accessible-toilet"
              summary = "wheelchair-accessible toilet available" })
           ("iz",
            { ``type`` = "hint"
              code = "intercity-2"
              summary = "Intercity 2" }) |]

    let codesByText =
        [| ("journey cancelled", "journey-cancelled")
           ("stop cancelled", "stop-cancelled")
           ("signal failure", "signal-failure")
           ("signalstörung", "signal-failure")
           ("additional stop", "additional-stopover")
           ("platform change", "changed platform") |]

    let private parseHintByCode (parsed: Hint) (raw: FsHafas.Raw.RawRem) : Hint =
        if raw.``type`` = "K" then
            match raw.txtN with
            | Some (value) -> { parsed with text = value }
            | None -> parsed
        else

        if raw.``type`` = "A" then
            match hintsByCode
                  |> Array.tryFind (fun (c, _) -> c = raw.code.ToLower()) with
            | Some (_, h) ->
                { parsed with
                      code = Some h.code
                      summary = Some h.summary }
            | None -> parsed

        else if raw.txtN.IsSome then
            let code =
                codesByText
                |> Array.tryFind (fun (s, t) -> s = raw.txtN.Value.ToLower())
                |> Option.bind (fun (s, t) -> Some t)

            { parsed with code = code }
        else
            parsed

    let private parseStatusByCode (parsed: Status) (raw: FsHafas.Raw.RawRem) : Status =
        if raw.``type`` = "K" then
            match raw.txtN with
            | Some (value) -> { parsed with text = value }
            | None -> parsed
        else

        if raw.txtN.IsSome then
            let code =
                codesByText
                |> Array.tryFind (fun (s, t) -> s = raw.txtN.Value.ToLower())
                |> Option.bind (fun (s, t) -> Some t)

            { parsed with code = code }
        else
            parsed

    let private parseHint
        (parsed: U3<Hint, Status, Warning> option)
        (h: FsHafas.Raw.RawRem)
        : U3<Hint, Status, Warning> option =
        match parsed with
        | Some (U3.Case1 parsedHint) -> U3.Case1(parseHintByCode parsedHint h) |> Some
        | Some (U3.Case2 parsedStatus) -> U3.Case2(parseStatusByCode parsedStatus h) |> Some
        | _ -> parsed


    let private parseLineWithAdditionalName (parsed: Line) (p: FsHafas.Raw.RawProd) : Line =
        match p.addName with
        | Some _ ->
            { parsed with
                  additionalName = parsed.name
                  name = p.addName }
        | None -> parsed

    let loadFactors =
        [| ""
           "low-to-medium"
           "high"
           "very-high"
           "exceptionally-high" |]

    let parseLoadFactor (opt: FsHafas.Parser.Options) (tcocL: FsHafas.Raw.RawTcoc []) (tcocX: int []) : string option =
        let cls =
            if opt.firstClass then
                "FIRST"
            else
                "SECOND"

        match tcocL |> Array.tryFind (fun t -> t.c = cls) with
        | Some tcoc when tcoc.r.IsSome -> Some(loadFactors.[tcoc.r.Value])
        | _ -> None

    let parseJourneyLegWithLoadFactor (parsed: Leg) (ctx: FsHafas.Parser.Context) (pt: FsHafas.Raw.RawSec) (date: string) : Leg =
        let tcocX =
            match pt.jny with
            | Some jny ->
                match jny.dTrnCmpSX with
                | Some dTrnCmpSX -> dTrnCmpSX.tcocX
                | None -> None
            | None -> None

        let tcocL =
            match ctx.res.common with
            | Some common -> common.tcocL
            | None -> None

        match tcocX, tcocL with
        | Some tcocX, Some tcocL ->
            let lf = parseLoadFactor ctx.opt tcocL tcocX

            { parsed with loadFactor = lf }

        | _ -> parsed

    let parseArrOrDepWithLoadFactor (parsed: Alternative) (ctx: FsHafas.Parser.Context) (d: FsHafas.Raw.RawJny) : Alternative =
        let tcocX =
            match d.stbStop with
            | Some stbStop ->
                match stbStop.dTrnCmpSX with
                | Some dTrnCmpSX -> dTrnCmpSX.tcocX
                | None -> None
            | None -> None

        let tcocL =
            match ctx.res.common with
            | Some common -> common.tcocL
            | None -> None

        match tcocX, tcocL with
        | Some tcocX, Some tcocL ->
            let lf = parseLoadFactor ctx.opt tcocL tcocX

            { parsed with loadFactor = lf }

        | _ -> parsed

    let private parseJourneyWithPrice (parsed: Journey) (raw: FsHafas.Raw.RawOutCon) : Journey =
        match raw.trfRes with
        | Some trfRes when
            trfRes.fareSetL.Length > 0
            && trfRes.fareSetL.[0].fareL.Length > 0 ->
            match trfRes.fareSetL.[0].fareL.[0].prc with
            | Some prc when prc > 0 ->
                { parsed with
                      price =
                          Some(
                              { amount = System.Math.Round(float (prc) / 100.0, 2)
                                currency = "EUR"
                                hint = None }
                          ) }
            | _ -> parsed
        | _ -> parsed

    let private IBNR = Regex(@"^\d{6,}$")

    let formatStation (id: string) =
        if IBNR.IsMatch id then
            id
        else
            raise (System.ArgumentException("station id: " + id))

    let bikeFltr : FsHafas.Raw.JnyFltr =
        { ``type`` = "BC"
          mode = "INC"
          value = ""
          meta = None }

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

    let private formatLoyaltyCard (firstClass: bool) (data: LoyaltyCard) =
        match data.``type``, data.discount with
        | Some ``type``, Some discount ->
            if ``type`` = "Bahncard" then
                if discount = 25 then
                    if firstClass then 1 else 2
                else if discount = 50 then
                    if firstClass then 3 else 4
                else
                    0
            else
                0
        | _ -> 0

    let transformJourneysQuery (opt: JourneysOptions option) (q: FsHafas.Raw.TripSearchRequest) : FsHafas.Raw.TripSearchRequest =
        let bike =
            getOptionValue opt (fun v -> v.bike) Default.JourneysOptions

        let firstClass =
            getOptionValue opt (fun v -> v.firstClass) Default.JourneysOptions

        let jnyFltrL =
            if bike then
                Array.append [| bikeFltr |] q.jnyFltrL
            else
                q.jnyFltrL

        let redtnCard =
            match opt with
            | Some opt when opt.loyaltyCard.IsSome -> Some(formatLoyaltyCard firstClass opt.loyaltyCard.Value)
            | _ -> None

        let trfReq : FsHafas.Raw.TrfReq =
            { jnyCl = if firstClass then 1 else 2
              tvlrProf =
                  [| { ``type`` = "E"
                       redtnCard = redtnCard } |]
              cType = "PK" }

        { q with
              trfReq = Some trfReq
              jnyFltrL = jnyFltrL }

    let private req : FsHafas.Raw.RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
              { id = "DB"
                v = "16040000"
                ``type`` = "IPH"
                name = "DB Navigator" }
          ext = "DB.R19.04.a"
          ver = "1.15"
          auth =
              { ``type`` = "AID"
                aid = "n91dB8Z77MLdoR0K" } }

    let getProfile () =
        let profile = FsHafas.Api.Parser.defaultProfile

        { profile with
              locale = "de-DE"
              timezone = "Europe/Berlin"
              endpoint = "https://reiseauskunft.bahn.de/bin/mgate.exe"
              salt = "bdI8UVj40K5fvxwf"
              cfg =
                  Some
                      { polyEnc = "GPA"
                        rtMode = Some "HYBRID" }
              baseRequest = Some req
              products = products
              trip = Some true
              radar = Some true
              journeysOutFrwd = true
              formatStation = formatStation
              transformJourneysQuery = transformJourneysQuery
              parseJourney =
                  (fun (ctx: FsHafas.Parser.Context) (p: FsHafas.Raw.RawOutCon) -> parseJourneyWithPrice (profile.parseJourney ctx p) p)
              parseJourneyLeg =
                  (fun (ctx: FsHafas.Parser.Context) (pt: FsHafas.Raw.RawSec) (date: string) ->
                      parseJourneyLegWithLoadFactor (profile.parseJourneyLeg ctx pt date) ctx pt date)
              parseDeparture =
                  (fun (ctx: FsHafas.Parser.Context) (pt: FsHafas.Raw.RawJny) ->
                      parseArrOrDepWithLoadFactor (profile.parseDeparture ctx pt) ctx pt)
              parseHint = (fun (ctx: FsHafas.Parser.Context) (p: FsHafas.Raw.RawRem) -> parseHint (profile.parseHint ctx p) p)
              parseLine =
                  (fun (ctx: FsHafas.Parser.Context) (p: FsHafas.Raw.RawProd) -> parseLineWithAdditionalName (profile.parseLine ctx p) p)
              reachableFrom = Some true }
