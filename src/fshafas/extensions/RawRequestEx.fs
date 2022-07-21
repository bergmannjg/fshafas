namespace FsHafas.Extensions

open FsHafas.Client
open FsHafas.Raw

#if FABLE_COMPILER
open Fable.Core
open Fable.Core.JsInterop
#endif

#if FABLE_JS
open Thoth.Json
#endif

#if FABLE_PY
open System.Text.RegularExpressions

open Fable.SimpleJson.Python
#endif

module internal RawRequestEx =

#if FABLE_PY
    // generated by script 'scripts/names.fsx'
    let private replacements =
        Map [ ("a_cncl", "aCncl")
              ("a_out_r", "aOutR")
              ("a_out_s", "aOutS")
              ("a_platf_ch", "aPlatfCh")
              ("a_platf_r", "aPlatfR")
              ("a_platf_s", "aPlatfS")
              ("a_pltf_r", "aPltfR")
              ("a_pltf_s", "aPltfS")
              ("a_prod_x", "aProdX")
              ("a_prog_type", "aProgType")
              ("a_tzoffset", "aTZOffset")
              ("a_time_r", "aTimeR")
              ("a_time_s", "aTimeS")
              ("add_name", "addName")
              ("aff_prod_ref_l", "affProdRefL")
              ("age_of_report", "ageOfReport")
              ("arr_loc_l", "arrLocL")
              ("button_text", "buttonText")
              ("c_crd", "cCrd")
              ("c_type", "cType")
              ("calc_date", "calcDate")
              ("calc_time", "calcTime")
              ("cat_code", "catCode")
              ("cat_in", "catIn")
              ("cat_out", "catOut")
              ("cat_out_l", "catOutL")
              ("cat_out_s", "catOutS")
              ("cat_ref_l", "catRefL")
              ("cn_loc_x", "cnLocX")
              ("con_subscr", "conSubscr")
              ("crd_enc_f", "crdEncF")
              ("crd_enc_s", "crdEncS")
              ("crd_enc_yx", "crdEncYX")
              ("crd_enc_z", "crdEncZ")
              ("crd_sys_x", "crdSysX")
              ("ctx_recon", "ctxRecon")
              ("d_cncl", "dCncl")
              ("d_dir_flg", "dDirFlg")
              ("d_dir_txt", "dDirTxt")
              ("d_in_r", "dInR")
              ("d_in_s", "dInS")
              ("d_platf_r", "dPlatfR")
              ("d_platf_s", "dPlatfS")
              ("d_pltf_r", "dPltfR")
              ("d_pltf_s", "dPltfS")
              ("d_prod_x", "dProdX")
              ("d_prog_type", "dProgType")
              ("d_tzoffset", "dTZOffset")
              ("d_time_r", "dTimeR")
              ("d_time_s", "dTimeS")
              ("d_trn_cmp_sx", "dTrnCmpSX")
              ("date_b", "dateB")
              ("dep_loc_l", "depLocL")
              ("dir_geo", "dirGeo")
              ("dir_l", "dirL")
              ("dir_ref_l", "dirRefL")
              ("dir_txt", "dirTxt")
              ("e_date", "eDate")
              ("e_time", "eTime")
              ("edge_ref_l", "edgeRefL")
              ("entry_loc_l", "entryLocL")
              ("err_txt", "errTxt")
              ("event_ref_l", "eventRefL")
              ("ext_cont", "extCont")
              ("ext_id", "extId")
              ("f_date", "fDate")
              ("f_idx", "fIdx")
              ("f_loc_x", "fLocX")
              ("f_time", "fTime")
              ("fare_l", "fareL")
              ("fare_set_l", "fareSetL")
              ("fp_b", "fpB")
              ("fp_e", "fpE")
              ("get_ist", "getIST")
              ("get_iv", "getIV")
              ("get_pois", "getPOIs")
              ("get_pt", "getPT")
              ("get_passlist", "getPasslist")
              ("get_polyline", "getPolyline")
              ("get_stops", "getStops")
              ("get_tariff", "getTariff")
              ("gis_fltr_l", "gisFltrL")
              ("grid_l", "gridL")
              ("him_fltr_l", "himFltrL")
              ("him_l", "himL")
              ("him_msg_cat_l", "himMsgCatL")
              ("him_msg_edge_l", "himMsgEdgeL")
              ("him_msg_event_l", "himMsgEventL")
              ("him_x", "himX")
              ("ico_crd", "icoCrd")
              ("ico_l", "icoL")
              ("ico_x", "icoX")
              ("is_bookable", "isBookable")
              ("is_from_price", "isFromPrice")
              ("is_main_mast", "isMainMast")
              ("is_rchbl", "isRchbl")
              ("is_sot_con", "isSotCon")
              ("is_upsell", "isUpsell")
              ("item_l", "itemL")
              ("jny_cl", "jnyCl")
              ("jny_fltr_l", "jnyFltrL")
              ("jny_l", "jnyL")
              ("l_mod_date", "lModDate")
              ("l_mod_time", "lModTime")
              ("layer_x", "layerX")
              ("line_id", "lineId")
              ("line_l", "lineL")
              ("ll_crd", "llCrd")
              ("loc_data", "locData")
              ("loc_fltr_l", "locFltrL")
              ("loc_l", "locL")
              ("loc_mode", "locMode")
              ("loc_x", "locX")
              ("m_mast_loc_x", "mMastLocX")
              ("m_sec", "mSec")
              ("match_id", "matchId")
              ("max_c", "maxC")
              ("max_chg", "maxChg")
              ("max_dist", "maxDist")
              ("max_dur", "maxDur")
              ("max_jny", "maxJny")
              ("max_loc", "maxLoc")
              ("max_num", "maxNum")
              ("min_c", "minC")
              ("min_chg_time", "minChgTime")
              ("min_dist", "minDist")
              ("msg_l", "msgL")
              ("msg_ref_l", "msgRefL")
              ("n_cols", "nCols")
              ("n_rows", "nRows")
              ("num_c", "numC")
              ("num_f", "numF")
              ("only_rt", "onlyRT")
              ("op_l", "opL")
              ("opr_x", "oprX")
              ("out_con_l", "outConL")
              ("out_ctx_scr_b", "outCtxScrB")
              ("out_ctx_scr_f", "outCtxScrF")
              ("out_date", "outDate")
              ("out_frwd", "outFrwd")
              ("out_time", "outTime")
              ("p_cls", "pCls")
              ("p_loc_x", "pLocX")
              ("p_ref_l", "pRefL")
              ("per_size", "perSize")
              ("per_step", "perStep")
              ("planrt_ts", "planrtTS")
              ("poly_enc", "polyEnc")
              ("poly_g", "polyG")
              ("poly_l", "polyL")
              ("poly_xl", "polyXL")
              ("pos_l", "posL")
              ("pp_idx", "ppIdx")
              ("pp_loc_ref_l", "ppLocRefL")
              ("proc_abs", "procAbs")
              ("prod_ctx", "prodCtx")
              ("prod_l", "prodL")
              ("prod_x", "prodX")
              ("pub_ch_l", "pubChL")
              ("rec_state", "recState")
              ("redtn_card", "redtnCard")
              ("region_ref_l", "regionRefL")
              ("rem_l", "remL")
              ("rem_x", "remX")
              ("req_mode", "reqMode")
              ("res_recommendation", "resRecommendation")
              ("res_state", "resState")
              ("rt_mode", "rtMode")
              ("s_d", "sD")
              ("s_date", "sDate")
              ("s_days", "sDays")
              ("s_days_b", "sDaysB")
              ("s_days_i", "sDaysI")
              ("s_days_l", "sDaysL")
              ("s_days_r", "sDaysR")
              ("s_t", "sT")
              ("s_time", "sTime")
              ("sec_l", "secL")
              ("sect_x", "sectX")
              ("show_arslink", "showARSLink")
              ("sot_ctxt", "sotCtxt")
              ("sot_mode", "sotMode")
              ("sot_rating", "sotRating")
              ("status_code", "statusCode")
              ("stb_fltr_equiv", "stbFltrEquiv")
              ("stb_loc", "stbLoc")
              ("stb_stop", "stbStop")
              ("stc_output_x", "stcOutputX")
              ("stop_l", "stopL")
              ("stop_loc_l", "stopLocL")
              ("svc_req_l", "svcReqL")
              ("svc_res_l", "svcResL")
              ("t_date", "tDate")
              ("t_idx", "tIdx")
              ("t_loc_x", "tLocX")
              ("t_time", "tTime")
              ("tag_l", "tagL")
              ("target_ctx", "targetCtx")
              ("tc_m", "tcM")
              ("tcoc_l", "tcocL")
              ("tcoc_x", "tcocX")
              ("ticket_l", "ticketL")
              ("time_b", "timeB")
              ("train_pos_mode", "trainPosMode")
              ("trf_req", "trfReq")
              ("trf_res", "trfRes")
              ("tvlr_prof", "tvlrProf")
              ("txt_n", "txtN")
              ("txt_s", "txtS")
              ("ur_crd", "urCrd")
              ("via_loc_l", "viaLocL") ]

    let private toUndashed (xs: string list) =
        match replacements.TryFind(List.last xs) with
        | Some v -> Some v
        | None -> None
#endif

#if FABLE_JS
    let encode (request: RawRequest) =
        let encoder = Encode.Auto.generateEncoderCached (caseStrategy = CamelCase)

        request |> encoder |> Encode.toString 0

#else

#if FABLE_PY

    [<Emit("isinstance($1, ServerInfoRequest)")>]
    let private isInstanceServerInfoRequest (_: obj) : bool = jsNative

    [<Emit("isinstance($1, SearchOnTripRequest)")>]
    let private isInstanceSearchOnTripRequest (_: obj) : bool = jsNative

    let private encodeSvcReq (svcReq: SvcReq) =
        let cfg = Json.serialize svcReq.cfg

        let req =
            match svcReq.req with
            | U14.Case1 r -> Json.serialize r
            | U14.Case2 r -> Json.serialize r
            | U14.Case3 r -> Json.serialize r
            | U14.Case4 r -> Json.serialize r
            | U14.Case5 r -> Json.serialize r
            | U14.Case6 r -> Json.serialize r
            | U14.Case7 r -> Json.serialize r
            | U14.Case8 r -> Json.serialize r
            | U14.Case9 r -> Json.serialize r
            | U14.Case10 r -> Json.serialize r
            | U14.Case11 r -> Json.serialize r
            | U14.Case12 r -> Json.serialize r
            | U14.Case14 r -> Json.serialize r
            | _ -> "{}" // U14.Case13 ServerInfoRequest is empty

        sprintf "{\"cfg\":%s, \"meth\":\"%s\", \"req\":%s}" cfg svcReq.meth req

    /// encode RawRequest to json and erase type U14
    let encode (request: RawRequest) =
        try
            let svcreql = "[" + (encodeSvcReq request.svcReqL.[0]) + "]"

            let client = Json.serialize request.client
            let auth = Json.serialize request.auth

            let json =
                sprintf
                    "{\"lang\":\"%s\", \"svcReqL\":%s, \"client\":%s, \"ext\":\"%s\", \"ver\":\"%s\", \"auth\":%s}"
                    request.lang
                    svcreql
                    client
                    request.ext
                    request.ver
                    auth

            // workaround: missing feature in Fable.SimpleJson.Python
            json
                .Replace(", \"meta\": null", "")
                .Replace(",\"name\":null", "")
            |> SimpleJson.parseNative
            |> SimpleJson.mapKeysByPath toUndashed
            |> SimpleJson.toString
        with
        | e ->
            printf "error encode: %s" e.Message
            raise (System.Exception(e.Message))

#else

    let private converter =
        FsHafas.Api.Converter.U14EraseConverter<LocMatchRequest, TripSearchRequest, JourneyDetailsRequest, StationBoardRequest, ReconstructionRequest, JourneyMatchRequest, LocGeoPosRequest, LocGeoReachRequest, LocDetailsRequest, JourneyGeoPosRequest, HimSearchRequest, LineMatchRequest, ServerInfoRequest, SearchOnTripRequest>(
            FsHafas.Api.Converter.UnionCaseSelection.Disabled
        )

    let encode (request: RawRequest) =
        Serializer.SerializeWithConverter request converter

#endif
#endif
