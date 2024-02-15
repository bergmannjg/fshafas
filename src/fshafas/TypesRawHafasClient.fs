// generated by ts2fable and transformer
namespace FsHafas.Raw

open FsHafas.Client

/// <namespacedoc>
///   <summary>Types of raw hafas api generated from <a href="https://github.com/bergmannjg/hafas-client/blob/add-types-in-jsdoc/types-raw-api.ts">TS types</a></summary>
/// </namespacedoc>

type RawPltf = { ``type``: string; txt: string }

and RawTrnCmpSX =
    { tcM: int option
      tcocX: array<int> option }

and RawDep =
    { locX: int option
      idx: int option
      dProdX: int option
      dPlatfS: string option
      dInR: bool option
      dTimeS: string option
      dProgType: string option
      dTrnCmpSX: RawTrnCmpSX option
      dTZOffset: int option
      msgL: array<RawMsg> option
      ``type``: string option
      dTimeR: string option
      dCncl: bool option
      dPltfS: RawPltf option
      dPlatfR: string option
      dPltfR: RawPltf option
      dPlatfCh: bool option }

and RawArr =
    { locX: int option
      idx: int option
      aPlatfS: string option
      aOutR: bool option
      aTimeS: string option
      aProgType: string option
      aTZOffset: int option
      msgL: array<RawMsg> option
      ``type``: string option
      aTimeR: string option
      aCncl: bool option
      aPltfS: RawPltf option
      aPlatfR: string option
      aPltfR: RawPltf option
      prodL: array<RawProd> option }

and PubCh =
    { name: string
      fDate: string
      fTime: string
      tDate: string
      tTime: string }

and RawHim =
    { hid: string
      act: bool
      pub: string option
      head: string option
      lead: string option
      text: string option
      tckr: string option
      icoX: int
      prio: int
      fLocX: int option
      tLocX: int option
      prod: int option
      lModDate: string option
      lModTime: string option
      sDate: string option
      sTime: string option
      eDate: string option
      eTime: string option
      cat: int option
      pubChL: array<PubCh> option
      edgeRefL: array<int> option
      regionRefL: array<int> option
      catRefL: array<int> option
      eventRefL: array<int> option
      affProdRefL: array<int> option
      comp: string option }

and RawMsg =
    { ``type``: string
      remX: int option
      sty: string option
      txtC: RawRGB option
      fLocX: int option
      tLocX: int option
      fIdx: int option
      tIdx: int option
      himX: int option
      tagL: array<string> option }

and RawRem =
    { ``type``: string
      code: string
      prio: int option
      icoX: int option
      txtN: string option
      txtS: string option
      jid: string option }

and RawStop =
    { locX: int option
      idx: int option
      dProdX: int option
      dInR: bool option
      dTimeS: string option
      dTimeR: string option
      dTZOffset: int option
      dCncl: bool option
      dInS: bool option
      dPlatfS: string option
      dPltfS: RawPltf option
      dPlatfR: string option
      dPltfR: RawPltf option
      dPlatfCh: bool option
      dProgType: string option
      dDirTxt: string option
      dDirFlg: string option
      dTrnCmpSX: RawTrnCmpSX option
      aProdX: int option
      aPlatfS: string option
      aPltfS: RawPltf option
      aPlatfR: string option
      aPltfR: RawPltf option
      aOutR: bool option
      aTimeS: string option
      aTimeR: string option
      aTZOffset: int option
      aCncl: bool option
      aOutS: bool option
      aPlatfCh: bool option
      aProgType: string option
      ``type``: string option
      msgL: array<RawMsg> option
      remL: array<RawRem> option
      isAdd: bool option }

and PpLocRef = { ppIdx: int; locX: int }

and RawPoly =
    { delta: bool
      dim: int
      ``type``: string option
      crdEncYX: string
      crdEncZ: string option
      crdEncS: string
      crdEncF: string
      ppLocRefL: array<PpLocRef> option }

and PolyG = { polyXL: array<int> }

and RawAni =
    { mSec: array<int>
      proc: array<int>
      procAbs: array<int> option
      fLocX: array<int>
      tLocX: array<int>
      dirGeo: array<int>
      stcOutputX: array<int>
      polyG: PolyG option
      state: array<string>
      poly: RawPoly option }

and RawSDays =
    { fLocX: int option
      tLocX: int option
      fLocIdx: int option
      tLocIdx: int option
      sDaysR: string option
      sDaysI: string option
      sDaysB: string option }

and RawPolyG = { polyXL: array<int> }

and RawCrd =
    { x: int
      y: int
      z: int option
      floor: int option }

and RawFreq =
    { jnyL: array<RawJny> option
      minC: int option
      maxC: int option
      numC: int option }

and RawJny =
    { jid: string
      prodX: int
      dirTxt: string option
      status: string option
      isRchbl: bool option
      ctxRecon: string option
      remL: array<RawRem> option
      msgL: array<RawMsg> option
      stbStop: RawStop option
      subscr: string option
      poly: RawPoly option
      stopL: array<RawStop> option
      date: string option
      sDaysL: array<RawSDays> option
      dTrnCmpSX: RawTrnCmpSX option
      polyG: RawPolyG option
      ani: RawAni option
      pos: RawCrd option
      freq: RawFreq option
      prodL: array<RawProdRef> option
      dirL: array<RawDirRef> option
      sumLDrawStyleX: int option
      resLDrawStyleX: int option
      trainStartDate: string option
      durS: string option }

and RawGis =
    { dist: int option
      durS: string option
      dirGeo: int option
      ctx: string option
      gisPrvr: string option
      getDescr: bool option
      getPoly: bool option
      msgL: array<RawMsg> option }

and RawSec =
    { ``type``: string
      icoX: int option
      dep: RawDep
      arr: RawArr
      jny: RawJny option
      parJnyL: array<RawJny> option
      resState: string option
      resRecommendation: string option
      gis: RawGis option }

and RawSotCtxt =
    { cnLocX: int option
      calcDate: string
      jid: string option
      locMode: string
      pLocX: int option
      reqMode: string
      sectX: int option
      calcTime: string
      tName: string option }

and Content = { ``type``: string; content: string }
and ExtCont = { content: Content }

and RawTicket =
    { name: string
      prc: int
      cur: string
      extCont: ExtCont }

and RawPrice = { amount: int option }

and RawFare =
    { price: RawPrice option
      desc: string option
      isFromPrice: bool option
      isPartPrice: bool option
      retPriceIsCompletePrice: bool option
      retPrice: int option
      isBookable: bool option
      isUpsell: bool option
      verbundName: string option
      targetCtx: string option
      buttonText: string option
      name: string option
      ticketL: array<RawTicket> option }

and RawFareSet =
    { desc: string option
      fareL: array<RawFare> }

and RawTrfRes =
    { statusCode: string option
      fareSetL: array<RawFareSet> option }

and RawRecon = { ctx: string option }

and RawOutCon =
    { cid: string
      date: string
      dur: string
      durS: string option
      durR: string option
      chg: int
      sDays: RawSDays option
      dep: RawDep
      arr: RawArr
      secL: array<RawSec>
      ctxRecon: string option
      trfRes: RawTrfRes option
      conSubscr: string
      resState: string option
      resRecommendation: string option
      recState: string
      sotRating: int option
      isSotCon: bool option
      showARSLink: bool option
      sotCtxt: RawSotCtxt option
      cksum: string
      msgL: array<RawMsg> option
      recon: RawRecon option
      intvlSubscr: string option
      originType: string option
      freq: RawFreq option }

and RawItem =
    { col: int
      row: int
      msgL: array<RawMsg> option
      remL: array<int> option }

and RawGrid =
    { nCols: int
      nRows: int
      itemL: array<RawItem>
      ``type``: string
      title: string }

and RawLoc =
    { lid: string option
      ``type``: string option
      name: string
      icoX: int option
      extId: string option
      state: string
      crd: RawCrd option
      pCls: int option
      entry: bool option
      mMastLocX: int option
      pRefL: array<int> option
      wt: int option
      entryLocL: array<int> option
      stopLocL: array<int> option
      msgL: array<RawMsg> option
      gridL: array<RawGrid> option
      isMainMast: bool option
      meta: bool option
      dist: int option
      dur: int option
      gidL: array<string> option }

and RawProdCtx =
    { name: string option
      num: string option
      matchId: string option
      catOut: string option
      catOutS: string option
      catOutL: string option
      catIn: string option
      catCode: string option
      admin: string option
      addName: string option
      lineId: string option }

and RawOp = { name: string; icoX: int }

and RawProd =
    { name: string option
      number: string option
      icoX: int option
      oprX: int option
      prodCtx: RawProdCtx option
      cls: int option
      line: string option
      addName: string option }

and RawProdRef =
    { fLocX: int option
      tLocX: int option
      prodX: int
      fIdx: int option
      tIdx: int option }

and RawRGB = { r: int option; g: int; b: int }

and RawIco =
    { res: string option
      txt: string option
      text: string option
      txtS: string option
      fg: RawRGB option
      bg: RawRGB option }

and RawDir = { txt: string; flg: string option }

and RawDirRef =
    { dirX: int option
      fLocX: int option
      tLocX: int option
      fIdx: int option
      tIdx: int option }

and RawTcoc = { c: string; r: int option }
and RawHimMsgCat = { id: int }

and IcoCrd =
    { x: int
      y: int
      ``type``: string option }

and RawHimMsgEdge =
    { fLocX: int option
      tLocX: int option
      dir: int option
      icoCrd: IcoCrd
      msgRefL: array<int> option
      icoX: int option }

and RawHimMsgEvent =
    { fLocX: int option
      tLocX: int option
      fDate: string
      fTime: string
      tDate: string
      tTime: string }

and RawCommon =
    { locL: array<RawLoc> option
      prodL: array<RawProd> option
      remL: array<RawRem> option
      icoL: array<RawIco> option
      opL: array<RawOp> option
      maxC: int option
      numC: int option
      himL: array<RawHim> option
      polyL: array<RawPoly> option
      dirL: array<RawDir> option
      tcocL: array<RawTcoc> option
      himMsgCatL: array<RawHimMsgCat> option
      himMsgEdgeL: array<RawHimMsgEdge> option
      himMsgEventL: array<RawHimMsgEvent> option }

and RawMatch =
    { field: string option
      state: string option
      locL: array<RawLoc> option }

and RawPos = { locX: int; dur: int }

and RawLine =
    { lineId: string option
      prodX: int
      dirRefL: array<int> option
      jnyL: array<RawJny> option }

and RawResult =
    { common: RawCommon option
      msgL: array<RawHim> option
      ``type``: string option
      jnyL: array<RawJny> option
      outConL: array<RawOutCon> option
      outCtxScrB: string option
      outCtxScrF: string option
      planrtTS: string option
      ``match``: RawMatch option
      locL: array<RawLoc> option
      journey: RawJny option
      hciVersion: string option
      fpE: string option
      sD: string option
      sT: string option
      fpB: string option
      posL: array<RawPos> option
      lineL: array<RawLine> option }

and SvcRes =
    { meth: string
      err: string option
      errTxt: string option
      res: RawResult option }

and RawResponse =
    { ver: string
      ext: string option
      lang: string
      id: string option
      err: string option
      errTxt: string option
      svcResL: array<SvcRes> option }

and Cfg =
    { polyEnc: string option
      rtMode: string option }

and Loc =
    { ``type``: string
      name: string option
      lid: string option }

and LocViaInput = { loc: Loc }

and LocMatchInput =
    { loc: Loc
      maxLoc: int
      field: string }

and LocMatchRequest = { input: LocMatchInput }
and LineMatchRequest = { input: string }
and JourneyDetailsRequest = { jid: string; getPolyline: bool }

and JnyFltr =
    { ``type``: string
      mode: string
      value: string option
      meta: string option }

and TvlrProf =
    { ``type``: string
      redtnCard: int option }

and TrfReq =
    { jnyCl: int
      tvlrProf: array<TvlrProf>
      cType: string }

and StationBoardRequest =
    { ``type``: string
      date: string
      time: string
      stbLoc: Loc
      jnyFltrL: array<JnyFltr>
      dur: int
      dirLoc: Loc option
      maxJny: int option }

and HimSearchRequest =
    { himFltrL: array<JnyFltr>
      getPolyline: bool option
      maxNum: int option
      dateB: string option
      timeB: string option
      dateE: string option
      timeE: string option }

and ReconstructionRequest =
    { getIST: bool
      getPasslist: bool
      getPolyline: bool
      getTariff: bool
      ctxRecon: string option
      outReconL: array<RawRecon> option }

and LocData =
    { loc: Loc
      ``type``: string
      date: string
      time: string }

and SearchOnTripRequest =
    { sotMode: string
      jid: string
      locData: LocData
      arrLocL: array<Loc>
      jnyFltrL: array<JnyFltr>
      getPasslist: bool
      getPolyline: bool
      minChgTime: int
      getTariff: bool }

and TripSearchRequest =
    { getPasslist: bool
      maxChg: int
      minChgTime: int
      depLocL: array<Loc>
      viaLocL: array<LocViaInput> option
      arrLocL: array<Loc>
      jnyFltrL: array<JnyFltr>
      gisFltrL: array<JnyFltr>
      getTariff: bool
      ushrp: bool
      getPT: bool
      getIV: bool
      getPolyline: bool
      outDate: string
      outTime: string
      numF: int
      outFrwd: bool
      trfReq: TrfReq option }

and JourneyMatchRequest =
    { input: string
      date: string option
      time: string option
      dateB: string option
      timeB: string option
      dateE: string option
      timeE: string option
      onlyCR: bool option
      jnyFltrL: array<JnyFltr> }

and RawcCrd = { x: int; y: int }

and RawRing =
    { cCrd: RawcCrd
      maxDist: int
      minDist: int }

and LocGeoPosRequest =
    { ring: RawRing
      locFltrL: array<JnyFltr>
      getPOIs: bool
      getStops: bool
      maxLoc: int }

and LocGeoReachRequest =
    { loc: Loc
      maxDur: int
      maxChg: int
      date: string
      time: string
      period: int
      jnyFltrL: array<JnyFltr> }

and LocDetailsRequest = { locL: array<Loc> }
and ServerInfoRequest = { getVersionInfo: bool }
and RawRect = { llCrd: RawCrd; urCrd: RawCrd }

and JourneyGeoPosRequest =
    { maxJny: int
      onlyRT: bool
      date: string
      time: string
      rect: RawRect
      perSize: int
      perStep: int
      ageOfReport: bool
      jnyFltrL: array<JnyFltr>
      trainPosMode: string }

and SvcReq =
    { cfg: Cfg
      meth: string
      req: U14<LocMatchRequest, TripSearchRequest, JourneyDetailsRequest, StationBoardRequest, ReconstructionRequest, JourneyMatchRequest, LocGeoPosRequest, LocGeoReachRequest, LocDetailsRequest, JourneyGeoPosRequest, HimSearchRequest, LineMatchRequest, ServerInfoRequest, SearchOnTripRequest> }

and RawRequestClient =
    { id: string
      v: string
      ``type``: string
      name: string }

and RawRequestAuth = { ``type``: string; aid: string }

and RawRequest =
    { lang: string
      svcReqL: array<SvcReq>
      client: RawRequestClient
      ext: string option
      ver: string
      auth: RawRequestAuth }
