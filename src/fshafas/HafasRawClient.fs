namespace FsHafas.Api

open FsHafas
open FsHafas.Raw

#if FABLE_COMPILER
open Fable.Core
open Thoth.Json
#endif

type HafasRawClient(endpoint: string, salt: string, cfg: Raw.Cfg, baseRequest: Raw.RawRequest) =

    let log msg o = Log.Print msg o

    let httpClient = Request.HttpClient()

    let makeRequest
        (meth: string)
        (parameters: U13<LocMatchRequest, TripSearchRequest, JourneyDetailsRequest, StationBoardRequest, ReconstructionRequest, JourneyMatchRequest, LocGeoPosRequest, LocGeoReachRequest, LocDetailsRequest, JourneyGeoPosRequest, HimSearchRequest, LineMatchRequest, ServerInfoRequest>)
        : Raw.RawRequest =
        let svcReqL : Raw.SvcReq =
            { cfg = cfg
              meth = meth
              req = parameters }

        { baseRequest with
              svcReqL = [| svcReqL |] }

#if FABLE_COMPILER
    let encode (request: Raw.RawRequest) =
        let encoder =
            Encode.Auto.generateEncoderCached (caseStrategy = CamelCase)

        request |> encoder |> Encode.toString 0

    let decode (json: string) : Raw.RawResponse =
        let decoded =
            Decode.Auto.fromString<Raw.RawResponse> (json, caseStrategy = CamelCase)

        match decoded with
        | Ok response -> response
        | Error decodingError -> failwith (sprintf "was unable to decode: %s. Reason: %s" json decodingError)
#else
    let converter =
        Converter.U13EraseConverter<LocMatchRequest, TripSearchRequest, JourneyDetailsRequest, StationBoardRequest, ReconstructionRequest, JourneyMatchRequest, LocGeoPosRequest, LocGeoReachRequest, LocDetailsRequest, JourneyGeoPosRequest, HimSearchRequest, LineMatchRequest, ServerInfoRequest>(
            Converter.UnionCaseSelection.Disabled
        )

    let encode (request: Raw.RawRequest) =
        Serializer.SerializeWithConverter request converter

    let decode (json: string) : Raw.RawResponse =
        Serializer.Deserialize<Raw.RawResponse>(json)
#endif

    let asyncPost (request: Raw.RawRequest) : Async<Raw.RawResult option> =

        let json = encode request

        log "request:" json

        async {
            try
                let! result = httpClient.PostAsync endpoint salt json

                log "response:" result

                let response = decode result

                if response.svcResL.Length = 1 then
                    let svcRes = response.svcResL.[0]

                    match svcRes.err, svcRes.errTxt with
                    | Some err, Some errTxt when err <> "OK" -> return raise (System.Exception(err + ":" + errTxt))
                    | Some err, _ when err <> "OK" -> return raise (System.Exception(err))
                    | _ -> return Some(svcRes.res)
                else
                    match response.err, response.errTxt with
                    | Some err, Some errTxt -> return raise (System.Exception(err + ":" + errTxt))
                    | Some err, _ -> return raise (System.Exception(err))
                    | _ -> return None
            with ex -> return raise (System.Exception(ex.Message))
        }

    member __.Dispose() = httpClient.Dispose()

    member __.AsyncLocMatch(locMatchRequest: Raw.LocMatchRequest) =
        async {
            match! asyncPost (makeRequest "LocMatch" (U13.Case1 locMatchRequest)) with
            | Some (res) when res.``match``.IsSome -> return (res.common, Some res, res.``match``.Value.locL)
            | _ -> return (None, None, Array.empty)
        }

    member __.AsyncTripSearch(tripSearchRequest: Raw.TripSearchRequest) =
        async {
            match! asyncPost (makeRequest "TripSearch" (U13.Case2 tripSearchRequest)) with
            | Some (res) -> return (res.common, Some res, res.outConL)
            | None -> return (None, None, None)
        }

    member __.AsyncJourneyDetails(journeyDetailsRequest: Raw.JourneyDetailsRequest) =
        async {
            match! asyncPost (makeRequest "JourneyDetails" (U13.Case3 journeyDetailsRequest)) with
            | Some (res) -> return (res.common, Some res, res.journey)
            | None -> return (None, None, None)
        }

    member __.AsyncStationBoard(stationBoardRequest: Raw.StationBoardRequest) =
        async {
            match! asyncPost (makeRequest "StationBoard" (U13.Case4 stationBoardRequest)) with
            | Some (res) -> return (res.common, Some res, res.jnyL)
            | None -> return (None, None, None)
        }

    member __.AsyncReconstruction(reconstructionRequest: Raw.ReconstructionRequest) =
        async {
            match! asyncPost (makeRequest "Reconstruction" (U13.Case5 reconstructionRequest)) with
            | Some (res) -> return (res.common, Some res, res.outConL)
            | None -> return (None, None, None)
        }

    member __.AsyncJourneyMatch(journeyMatchRequest: Raw.JourneyMatchRequest) =
        async {
            match! asyncPost (makeRequest "JourneyMatch" (U13.Case6 journeyMatchRequest)) with
            | Some (res) -> return (res.common, Some res, res.jnyL)
            | None -> return (None, None, None)
        }

    member __.AsyncLocGeoPos(locGeoPosRequest: Raw.LocGeoPosRequest) =
        async {
            match! asyncPost (makeRequest "LocGeoPos" (U13.Case7 locGeoPosRequest)) with
            | Some (res) when res.locL.IsSome -> return (res.common, Some res, res.locL.Value)
            | _ -> return (None, None, Array.empty)
        }

    member __.AsyncLocGeoReach(locGeoReachRequest: Raw.LocGeoReachRequest) =
        async {
            match! asyncPost (makeRequest "LocGeoReach" (U13.Case8 locGeoReachRequest)) with
            | Some (res) when res.posL.IsSome -> return (res.common, Some res, res.posL.Value)
            | _ -> return (None, None, Array.empty)
        }

    member __.AsyncLocDetails(locDetailsRequest: Raw.LocDetailsRequest) =
        async {
            match! asyncPost (makeRequest "LocDetails" (U13.Case9 locDetailsRequest)) with
            | Some (res) when res.locL.IsSome && res.locL.Value.Length > 0 ->
                return (res.common, Some res, Some res.locL.Value.[0])
            | _ -> return (None, None, None)
        }

    member __.AsyncJourneyGeoPos(journeyGeoPosRequest: Raw.JourneyGeoPosRequest) =
        async {
            match! asyncPost (makeRequest "JourneyGeoPos" (U13.Case10 journeyGeoPosRequest)) with
            | Some (res) -> return (res.common, Some res, res.jnyL)
            | _ -> return (None, None, None)
        }

    member __.AsyncHimSearch(himSearchRequest: Raw.HimSearchRequest) =
        async {
            match! asyncPost (makeRequest "HimSearch" (U13.Case11 himSearchRequest)) with
            | Some (res) -> return (res.common, Some res, res.msgL)
            | _ -> return (None, None, None)
        }

    member __.AsyncLineMatch(lineMatchRequest: Raw.LineMatchRequest) =
        async {
            match! asyncPost (makeRequest "LineMatch" (U13.Case12 lineMatchRequest)) with
            | Some (res) -> return (res.common, Some res, res.lineL)
            | _ -> return (None, None, None)
        }

    member __.AsyncServerInfo(serverInfoRequest: Raw.ServerInfoRequest) =
        async {
            match! asyncPost (makeRequest "ServerInfo" (U13.Case13 serverInfoRequest)) with
            | Some (res) -> return (res.common, Some res)
            | _ -> return (None, None)
        }
