namespace FsHafas.Api

open FsHafas.Client
open FsHafas.Raw

#if FABLE_COMPILER
open Fable.Core
open Thoth.Json
#endif

/// <summary>Direct (`raw`) interface to the hafas endpoints</summary>
type HafasRawClient(endpoint: string, salt: string, cfg:  FsHafas.Raw.Cfg, baseRequest:  RawRequest) =

    let log msg o = FsHafas.Client.Log.Print msg o

    let httpClient = Request.HttpClient()

    let makeRequest
        (meth: string)
        (parameters: U13<LocMatchRequest, TripSearchRequest, JourneyDetailsRequest, StationBoardRequest, ReconstructionRequest, JourneyMatchRequest, LocGeoPosRequest, LocGeoReachRequest, LocDetailsRequest, JourneyGeoPosRequest, HimSearchRequest, LineMatchRequest, ServerInfoRequest>)
        : RawRequest =
        let svcReqL : SvcReq =
            { cfg = cfg
              meth = meth
              req = parameters }

        { baseRequest with
              svcReqL = [| svcReqL |] }

#if FABLE_COMPILER
    let encode (request: RawRequest) =
        let encoder =
            Encode.Auto.generateEncoderCached (caseStrategy = CamelCase)

        request |> encoder |> Encode.toString 0

    let decode (json: string) : RawResponse =
        let decoded =
            Decode.Auto.fromString<RawResponse> (json, caseStrategy = CamelCase)

        match decoded with
        | Ok response -> response
        | Error decodingError -> failwith (sprintf "was unable to decode: %s. Reason: %s" json decodingError)
#else
    let converter =
        Converter.U13EraseConverter<LocMatchRequest, TripSearchRequest, JourneyDetailsRequest, StationBoardRequest, ReconstructionRequest, JourneyMatchRequest, LocGeoPosRequest, LocGeoReachRequest, LocDetailsRequest, JourneyGeoPosRequest, HimSearchRequest, LineMatchRequest, ServerInfoRequest>(
            Converter.UnionCaseSelection.Disabled
        )

    let encode (request: RawRequest) =
        Serializer.SerializeWithConverter request converter

    let decode (json: string) : RawResponse =
        Serializer.Deserialize<RawResponse>(json)
#endif

    let asyncPost (request: RawRequest) : Async<RawResult> =

        let json = encode request

        log "request:" json

        async {
            let! result = httpClient.PostAsync endpoint salt json

            log "response:" result

            let response = decode result

            if response.svcResL.Length = 1 then
                let svcRes = response.svcResL.[0]

                match svcRes.err, svcRes.errTxt with
                | Some err, Some errTxt when err <> "OK" -> return raise (System.Exception(err + ":" + errTxt))
                | Some err, _ when err <> "OK" -> return raise (System.Exception(err))
                | _ -> return svcRes.res
            else
                match response.err, response.errTxt with
                | Some err, Some errTxt -> return raise (System.Exception(err + ":" + errTxt))
                | Some err, _ -> return raise (System.Exception(err))
                | _ -> return raise (System.Exception("invalid response"))
        }

    member __.Dispose() = httpClient.Dispose()

    member __.AsyncLocMatch(locMatchRequest: LocMatchRequest) =
        async {
            let! res = asyncPost (makeRequest "LocMatch" (U13.Case1 locMatchRequest))

            match res.``match`` with
            | Some ``match`` -> return (res.common, Some res, ``match``.locL)
            | _ -> return (None, None, Array.empty)
        }

    member __.AsyncTripSearch(tripSearchRequest: TripSearchRequest) =
        async {
            let! res = asyncPost (makeRequest "TripSearch" (U13.Case2 tripSearchRequest))
            return (res.common, Some res, res.outConL)
        }

    member __.AsyncJourneyDetails(journeyDetailsRequest: JourneyDetailsRequest) =
        async {
            let! res = asyncPost (makeRequest "JourneyDetails" (U13.Case3 journeyDetailsRequest))
            return (res.common, Some res, res.journey)
        }

    member __.AsyncStationBoard(stationBoardRequest: StationBoardRequest) =
        async {
            let! res = asyncPost (makeRequest "StationBoard" (U13.Case4 stationBoardRequest))
            return (res.common, Some res, res.jnyL)
        }

    member __.AsyncReconstruction(reconstructionRequest: ReconstructionRequest) =
        async {
            let! res = asyncPost (makeRequest "Reconstruction" (U13.Case5 reconstructionRequest))
            return (res.common, Some res, res.outConL)
        }

    member __.AsyncJourneyMatch(journeyMatchRequest: JourneyMatchRequest) =
        async {
            let! res = asyncPost (makeRequest "JourneyMatch" (U13.Case6 journeyMatchRequest))
            return (res.common, Some res, res.jnyL)
        }

    member __.AsyncLocGeoPos(locGeoPosRequest: LocGeoPosRequest) =
        async {
            let! res = asyncPost (makeRequest "LocGeoPos" (U13.Case7 locGeoPosRequest))

            match res.locL with
            | Some locL -> return (res.common, Some res, locL)
            | _ -> return (None, None, Array.empty)
        }

    member __.AsyncLocGeoReach(locGeoReachRequest: LocGeoReachRequest) =
        async {
            let! res = asyncPost (makeRequest "LocGeoReach" (U13.Case8 locGeoReachRequest))

            match res.posL with
            | Some posL -> return (res.common, Some res, posL)
            | _ -> return (None, None, Array.empty)
        }

    member __.AsyncLocDetails(locDetailsRequest: LocDetailsRequest) =
        async {
            let! res = asyncPost (makeRequest "LocDetails" (U13.Case9 locDetailsRequest))

            match res.locL with
            | Some locL when locL.Length > 0 -> return (res.common, Some res, Some locL.[0])
            | _ -> return (None, None, None)
        }

    member __.AsyncJourneyGeoPos(journeyGeoPosRequest: JourneyGeoPosRequest) =
        async {
            let! res = asyncPost (makeRequest "JourneyGeoPos" (U13.Case10 journeyGeoPosRequest))
            return (res.common, Some res, res.jnyL)
        }

    member __.AsyncHimSearch(himSearchRequest: HimSearchRequest) =
        async {
            let! res = asyncPost (makeRequest "HimSearch" (U13.Case11 himSearchRequest))
            return (res.common, Some res, res.msgL)
        }

    member __.AsyncLineMatch(lineMatchRequest: LineMatchRequest) =
        async {
            let! res = asyncPost (makeRequest "LineMatch" (U13.Case12 lineMatchRequest))
            return (res.common, Some res, res.lineL)
        }

    member __.AsyncServerInfo(serverInfoRequest: ServerInfoRequest) =
        async {
            let! res = asyncPost (makeRequest "ServerInfo" (U13.Case13 serverInfoRequest))
            return (res.common, Some res)
        }
