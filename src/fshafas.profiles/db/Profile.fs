namespace FsHafas.Profiles

module Db =

    open System.Text.RegularExpressions

    open FsHafas.Client
    open FsHafas.Endpoint
    open FsHafas.Raw

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

    let private codesByText =
        [| ("journey cancelled", "journey-cancelled")
           ("stop cancelled", "stop-cancelled")
           ("signal failure", "signal-failure")
           ("signalstörung", "signal-failure")
           ("additional stop", "additional-stopover")
           ("platform change", "changed platform") |]

    let private parseHintByCode (parsed: Hint) (raw: RawRem) : Hint =
        if raw.``type`` = "K" then
            match raw.txtN with
            | Some(value) -> { parsed with text = value }
            | None -> parsed
        else if

            raw.``type`` = "A"
        then
            match hintsByCode |> Array.tryFind (fun (c, _) -> c = raw.code.ToLower()) with
            | Some(_, h) ->
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

    let private parseStatusByCode (parsed: Status) (raw: RawRem) : Status =
        if raw.``type`` = "K" then
            match raw.txtN with
            | Some(value) -> { parsed with text = value }
            | None -> parsed
        else if

            raw.txtN.IsSome
        then
            let code =
                codesByText
                |> Array.tryFind (fun (s, t) -> s = raw.txtN.Value.ToLower())
                |> Option.bind (fun (s, t) -> Some t)

            { parsed with code = code }
        else
            parsed

    let private parseHint (parsed: HintStatusWarning option) (h: RawRem) : HintStatusWarning option =
        match parsed with
        | Some(HintStatusWarning.Hint parsedHint) -> HintStatusWarning.Hint(parseHintByCode parsedHint h) |> Some
        | Some(HintStatusWarning.Status parsedStatus) ->
            HintStatusWarning.Status(parseStatusByCode parsedStatus h) |> Some
        | _ -> parsed


    let private parseLineWithAdditionalName (parsed: Line) (p: RawProd) : Line =
        match p.addName with
        | Some _ ->
            { parsed with
                additionalName = parsed.name
                name = p.addName }
        | None -> parsed

    let private loadFactors =
        [| ""; "low-to-medium"; "high"; "very-high"; "exceptionally-high" |]

    let private parseLoadFactor (opt: FsHafas.Endpoint.Options) (tcocL: RawTcoc[]) (tcocX: int[]) : string option =
        let cls = if opt.firstClass then "FIRST" else "SECOND"

        match tcocX |> Array.map (fun i -> tcocL.[i]) |> Array.tryFind (fun t -> t.c = cls) with
        | Some tcoc when tcoc.r.IsSome -> Some(loadFactors.[tcoc.r.Value])
        | _ -> None

    let private parseJourneyLegWithLoadFactor
        (parsed: Leg)
        (ctx: FsHafas.Endpoint.Context)
        (pt: RawSec)
        (date: string)
        : Leg =
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

    let private parseArrOrDepWithLoadFactor
        (parsed: Alternative)
        (ctx: FsHafas.Endpoint.Context)
        (d: RawJny)
        : Alternative =
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

    let private updatePrice (parsed: Journey) (raw: RawOutCon) : Journey =
        match raw.trfRes with
        | Some trfRes when
            trfRes.fareSetL.IsSome
            && trfRes.fareSetL.Value.Length > 0
            && trfRes.fareSetL.Value.[0].fareL.Length > 0
            ->
            match trfRes.fareSetL.Value.[0].fareL.[0].price with
            | Some price when price.amount > 0 ->
                { parsed with
                    price =
                        Some(
                            { amount = System.Math.Round(float price.amount / 100.0, 2)
                              currency = Some "EUR"
                              hint = None }
                        ) }
            | _ -> parsed
        | _ -> parsed

    let private updateTickets (parsed: Journey) (raw: RawOutCon) : Journey =
        match raw.trfRes with
        | Some trfRes when trfRes.fareSetL.IsSome && trfRes.fareSetL.Value.Length > 0 ->
            let tickets: Ticket[] =
                trfRes.fareSetL.Value
                |> Array.choose (fun s ->
                    if s.fareL.Length > 0 then
                        let fare = s.fareL[0]

                        match fare.ticketL with
                        | Some ticketL when ticketL.Length > 0 -> // refreshJourney()
                            let rawTicket = ticketL[0]

                            if rawTicket.price.IsSome then
                                Some
                                    { name = fare.name |> Option.defaultValue rawTicket.name
                                      priceObj = Some { amount = rawTicket.price.Value.amount }
                                      url = None }
                            else
                                None
                        | _ -> // journeys()
                            if fare.buttonText.IsSome && fare.price.IsSome then
                                Some
                                    { name = fare.buttonText.Value
                                      priceObj = Some { amount = fare.price.Value.amount }
                                      url = None }
                            else
                                None
                    else
                        None)

            { parsed with
                tickets = Some tickets
                price =
                    if tickets.Length > 0 && parsed.price.IsNone then
                        Some
                            { amount = System.Math.Round(tickets[0].priceObj.Value.amount / 100.0, 2)
                              currency = Some "EUR"
                              hint = None }
                    else
                        parsed.price }
        | _ -> parsed

    let private parseJourneyWithPriceAndTickets (parsed: Journey) (raw: RawOutCon) : Journey =
        updateTickets (updatePrice parsed raw) raw

    let private formatStation (id: string) =
        if Regex.IsMatch(id, @"^\d{6,}$") then
            id
        else
            raise (System.ArgumentException("station id: " + id))

    let private bikeFltr: JnyFltr =
        { ``type`` = "BC"
          mode = "INC"
          value = None
          meta = None }

    let private getOptionValue<'a, 'b> (opt: 'a option) (getter: 'a -> 'b option) (defaultOpt: 'a) =
        let defaultValue =
            match getter defaultOpt with
            | Some value -> value
            | None -> failwith "getOptionValue: value expected"

        match opt with
        | Some(value) ->
            match getter value with
            | Some result -> result
            | None -> defaultValue
        | None -> defaultValue

    let private formatLoyaltyCard (data: LoyaltyCard) =
        let cls = data.``class`` |> Option.defaultValue 2

        match data.discount with
        | Some discount ->
            if data.``type`` = "Bahncard" then
                if discount = 25 then
                    if cls = 1 then 1 else 2
                else if discount = 50 then
                    if cls = 1 then 3 else 4
                else
                    0
            else
                0
        | _ -> 0

    let private ageGroupFromAge (age: int) =
        if age < 6 then "B"
        else if age < 15 then "K"
        else if age < 27 then "Y"
        else if age < 65 then "E"
        else "S"

    let private transformCfg (routingMode: RoutingMode option) (cfg: FsHafas.Raw.Cfg) : FsHafas.Raw.Cfg =
        match routingMode with
        | Some routingMode ->
            { cfg with
                rtMode = Some(routingMode.ToString()) }
        | None ->
            { cfg with
                rtMode = Some(RoutingMode.REALTIME.ToString()) }

    let private trfReq
        (firstClass: option<bool>)
        (loyaltyCard: option<LoyaltyCard>)
        (tickets: option<bool>)
        (age: option<int>)
        (ageGroup: option<AgeGroup>)
        (isRefreshJourney: bool)
        : TrfReq =
        let firstClass = firstClass |> Option.defaultValue false

        let redtnCard =
            match loyaltyCard with
            | Some loyaltyCard -> Some(formatLoyaltyCard loyaltyCard)
            | _ -> None

        let rType =
            match isRefreshJourney, tickets with
            | true, Some true -> Some("DB-PE")
            | _ -> None

        let trfReq: TrfReq =
            { jnyCl = if firstClass then 1 else 2
              tvlrProf =
                [| { ``type`` =
                       (match age, ageGroup with
                        | Some age, _ -> ageGroupFromAge age
                        | None, Some ageGroup -> ageGroup.ToString()
                        | _ -> "E")
                     redtnCard = redtnCard } |]
              cType = "PK"
              rType = rType }

        trfReq

    let private transformJourneysQuery (opt: JourneysOptions option) (q: TripSearchRequest) : TripSearchRequest =
        let bike = getOptionValue opt (fun v -> v.bike) Default.JourneysOptions

        if opt.IsSome && opt.Value.age.IsSome && opt.Value.ageGroup.IsSome then
            raise (System.ArgumentException("opt.age and opt.ageGroup are mutually exclusive."))

        let jnyFltrL =
            if bike then
                Array.append [| bikeFltr |] q.jnyFltrL
            else
                q.jnyFltrL

        { q with
            trfReq =
                match opt with
                | Some opt -> Some(trfReq opt.firstClass opt.loyaltyCard opt.tickets opt.age opt.ageGroup false)
                | None -> None
            jnyFltrL = jnyFltrL }

    let private transformRefreshJourneyQuery
        (opt: RefreshJourneyOptions option)
        (q: ReconstructionRequest)
        : ReconstructionRequest =
        { q with
            trfReq =
                match opt with
                | Some opt -> Some(trfReq None None opt.tickets None None false)
                | None -> None }

    let private transformReq (req: FsHafas.Raw.RawRequest) : FsHafas.Raw.RawRequest =
        if (req.svcReqL.Length > 0 && req.svcReqL.[0].meth = "LocDetails") then
            { req with ver = "1.16" } // LocDetails seems broken with ver >1.16, all other methods work
        else
            req

    let profile = FsHafas.Api.Profile.defaultProfile ()

    profile._locale <- "de-DE"
    profile._timezone <- "Europe/Berlin"
    profile._endpoint <- "https://reiseauskunft.bahn.de/bin/mgate.exe"
    profile.salt <- "bdI8UVj40K5fvxwf"
    profile.addChecksum <- true

    profile.cfg <- Some { polyEnc = Some "GPA"; rtMode = None }

    profile.baseRequest <- Some DbConfig.Request.request
    profile._products <- DbConfig.Products.products
    profile._trip <- Some true
    profile._radar <- Some true
    profile._tripsByName <- Some true
    profile._reachableFrom <- Some true
    profile._journeysFromTrip <- Some true
    profile.journeysOutFrwd <- true
    profile.formatStation <- formatStation
    profile.transformJourneysQuery <- transformJourneysQuery
    profile.transformRefreshJourneyQuery <- transformRefreshJourneyQuery
    profile.transformCfg <- transformCfg

    let private defaultTransformReq = profile.transformReq

    profile.transformReq <- (fun (req: FsHafas.Raw.RawRequest) -> transformReq (defaultTransformReq req))

    let private defaultParseJourney = profile.parseJourney

    profile.parseJourney <-
        (fun (ctx: FsHafas.Endpoint.Context) (p: RawOutCon) ->
            parseJourneyWithPriceAndTickets (defaultParseJourney ctx p) p)

    let private defaultParseJourneyLeg = profile.parseJourneyLeg

    profile.parseJourneyLeg <-
        (fun (ctx: FsHafas.Endpoint.Context) (pt: RawSec) (date: string) ->
            parseJourneyLegWithLoadFactor (defaultParseJourneyLeg ctx pt date) ctx pt date)

    let private defaultParseDeparture = profile.parseDeparture

    profile.parseDeparture <-
        (fun (ctx: FsHafas.Endpoint.Context) (pt: RawJny) ->
            parseArrOrDepWithLoadFactor (defaultParseDeparture ctx pt) ctx pt)

    let private defaultParseHint = profile.parseHint
    profile.parseHint <- (fun (ctx: FsHafas.Endpoint.Context) (p: RawRem) -> parseHint (defaultParseHint ctx p) p)

    let private defaultParseLine = profile.parseLine

    profile.parseLine <-
        (fun (ctx: FsHafas.Endpoint.Context) (p: RawProd) -> parseLineWithAdditionalName (defaultParseLine ctx p) p)
