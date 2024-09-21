namespace FsHafas.Api

open FsHafas.Client
open FsHafas.Raw

#if FABLE_COMPILER
open Fable.Core.JsInterop
#endif

/// <summary>Direct (`raw`) interface to the hafas endpoints</summary>
type HafasRawClient
    (
        endpoint: string,
        addChecksum: bool,
        addMicMac: bool,
        salt: string,
        cfg: FsHafas.Raw.Cfg,
        transformReq: FsHafas.Raw.RawRequest -> FsHafas.Raw.RawRequest,
        baseRequest: RawRequest
    ) =

    let toException (ex: HafasError) =
#if FABLE_JS
        // Fable compiles HafasError as not derived from JavaScript Error
        // here: copy HafasError fields to a JavaScript Error object and raise JavaScript Error
        let ex1 = System.Exception(ex.Message)
        ex1?code <- ex.code
        ex1?isHafasError <- true
        ex1
#else
        ex
#endif

    let log msg o = FsHafas.Client.Log.Print msg o

    let httpClient = Request.HttpClient()

    let makeConfiguredRequest
        (cfg: FsHafas.Raw.Cfg)
        (meth: string)
        (lang: string)
        (parameters:
            U14<
                LocMatchRequest,
                TripSearchRequest,
                JourneyDetailsRequest,
                StationBoardRequest,
                ReconstructionRequest,
                JourneyMatchRequest,
                LocGeoPosRequest,
                LocGeoReachRequest,
                LocDetailsRequest,
                JourneyGeoPosRequest,
                HimSearchRequest,
                LineMatchRequest,
                ServerInfoRequest,
                SearchOnTripRequest
             >)
        : RawRequest =
        let svcReqL: SvcReq =
            { cfg = cfg
              meth = meth
              req = parameters }

        transformReq (
            { baseRequest with
                lang = lang
                svcReqL = [| svcReqL |] }
        )

    let makeRequest
        (meth: string)
        (lang: string)
        (parameters:
            U14<
                LocMatchRequest,
                TripSearchRequest,
                JourneyDetailsRequest,
                StationBoardRequest,
                ReconstructionRequest,
                JourneyMatchRequest,
                LocGeoPosRequest,
                LocGeoReachRequest,
                LocDetailsRequest,
                JourneyGeoPosRequest,
                HimSearchRequest,
                LineMatchRequest,
                ServerInfoRequest,
                SearchOnTripRequest
             >)
        : RawRequest =
        makeConfiguredRequest cfg meth lang parameters

    let asyncPost (request: RawRequest) : Async<RawResult> =

        let json = FsHafas.Extensions.RawRequestEx.encode request

        log "request:" json

        async {
            let! result = httpClient.PostAsync endpoint addChecksum addMicMac salt json

            log "response:" result

            try
                if result.Length = 0 then
                    return raise (System.Exception("invalid response"))
                else
                    let response = FsHafas.Extensions.RawResponseEx.decode result

                    let svcResL = Option.defaultValue [||] response.svcResL

                    if svcResL.Length = 1 then
                        let svcRes = svcResL.[0]

                        match svcRes.err, svcRes.errTxt, svcRes.res with
                        | Some err, Some errTxt, _ when err <> "OK" -> return raise (HafasError(err, errTxt))
                        | Some err, _, _ when err <> "OK" -> return raise (HafasError(err, err))
                        | _, _, Some res -> return res
                        | _ -> return raise (System.Exception("invalid response"))
                    else
                        match response.err, response.errTxt with
                        | Some err, Some errTxt -> return raise (HafasError(err, errTxt))
                        | Some err, _ -> return raise (HafasError(err, err))
                        | _ -> return raise (System.Exception("invalid response"))
            with
            | :? (HafasError) as ex -> return raise (toException ex)
            | ex ->
                printfn "error: %s" ex.Message
                return raise (System.Exception("invalid response"))
        }

    member __.Dispose() = httpClient.Dispose()

    member __.AsyncLocMatch(lang: string, locMatchRequest: LocMatchRequest) =
        async {
            let! res = asyncPost (makeRequest "LocMatch" lang (U14.Case1 locMatchRequest))

            match res.``match`` with
            | Some ``match`` -> return (res.common, Some res, ``match``.locL)
            | _ -> return (None, None, None)
        }

    member __.AsyncTripSearch
        (lang: string, tripSearchRequest: TripSearchRequest)
        (transformCfg: FsHafas.Raw.Cfg -> FsHafas.Raw.Cfg)
        =
        async {
            let! res =
                asyncPost (makeConfiguredRequest (transformCfg cfg) "TripSearch" lang (U14.Case2 tripSearchRequest))

            return (res.common, Some res, res.outConL)
        }

    member __.AsyncBestPriceSearch(lang: string, tripSearchRequest: TripSearchRequest) =
        async {
            let! res = asyncPost (makeRequest "BestPriceSearch" lang (U14.Case2 tripSearchRequest))
            return (res.common, Some res, res.outConL)
        }

    member __.AsyncJourneyDetails(lang: string, journeyDetailsRequest: JourneyDetailsRequest) =
        async {
            let! res = asyncPost (makeRequest "JourneyDetails" lang (U14.Case3 journeyDetailsRequest))
            return (res.common, Some res, res.journey)
        }

    member __.AsyncStationBoard(lang: string, stationBoardRequest: StationBoardRequest) =
        async {
            let! res = asyncPost (makeRequest "StationBoard" lang (U14.Case4 stationBoardRequest))
            return (res.common, Some res, res.jnyL)
        }

    member __.AsyncReconstruction(lang: string, reconstructionRequest: ReconstructionRequest) =
        async {
            let! res = asyncPost (makeRequest "Reconstruction" lang (U14.Case5 reconstructionRequest))
            return (res.common, Some res, res.outConL)
        }

    member __.AsyncJourneyMatch(lang: string, journeyMatchRequest: JourneyMatchRequest) =
        async {
            let! res = asyncPost (makeRequest "JourneyMatch" lang (U14.Case6 journeyMatchRequest))
            return (res.common, Some res, res.jnyL)
        }

    member __.AsyncLocGeoPos(lang: string, locGeoPosRequest: LocGeoPosRequest) =
        async {
            let! res = asyncPost (makeRequest "LocGeoPos" lang (U14.Case7 locGeoPosRequest))

            return (res.common, Some res, res.locL)
        }

    member __.AsyncLocGeoReach(lang: string, locGeoReachRequest: LocGeoReachRequest) =
        async {
            let! res = asyncPost (makeRequest "LocGeoReach" lang (U14.Case8 locGeoReachRequest))

            match res.posL with
            | Some posL -> return (res.common, Some res, posL)
            | _ -> return (None, None, Array.empty)
        }

    member __.AsyncLocDetails(lang: string, locDetailsRequest: LocDetailsRequest) =
        async {
            let! res = asyncPost (makeRequest "LocDetails" lang (U14.Case9 locDetailsRequest))

            match res.locL with
            | Some locL when locL.Length > 0 -> return (res.common, Some res, Some locL.[0])
            | _ -> return (None, None, None)
        }

    member __.AsyncJourneyGeoPos(lang: string, journeyGeoPosRequest: JourneyGeoPosRequest) =
        async {
            let! res = asyncPost (makeRequest "JourneyGeoPos" lang (U14.Case10 journeyGeoPosRequest))
            return (res.common, Some res, res.jnyL)
        }

    member __.AsyncHimSearch(lang: string, himSearchRequest: HimSearchRequest) =
        async {
            let! res = asyncPost (makeRequest "HimSearch" lang (U14.Case11 himSearchRequest))
            return (res.common, Some res, res.msgL)
        }

    member __.AsyncLineMatch(lang: string, lineMatchRequest: LineMatchRequest) =
        async {
            let! res = asyncPost (makeRequest "LineMatch" lang (U14.Case12 lineMatchRequest))
            return (res.common, Some res, res.lineL)
        }

    member __.AsyncServerInfo(lang: string, serverInfoRequest: ServerInfoRequest) =
        async {
            let! res = asyncPost (makeRequest "ServerInfo" lang (U14.Case13 serverInfoRequest))
            return (res.common, Some res)
        }

    member __.AsyncSearchOnTrip(lang: string, searchOnTripRequest: SearchOnTripRequest) =
        async {
            let! res = asyncPost (makeRequest "SearchOnTrip" lang (U14.Case14 searchOnTripRequest))
            return (res.common, Some res, res.outConL)
        }
