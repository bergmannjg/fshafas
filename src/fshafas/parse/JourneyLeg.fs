namespace FsHafas.Parser

module internal JourneyLeg =

    open FsHafas.Client
    open FsHafas.Endpoint

    let parsePlatform (ctx: Context) (platfS: string option) (platfR: string option) (cncl: bool option) : Platform =
        let planned = platfS
        let prognosed = platfR

        if (cncl |> Option.exists id) then
            { platform = None
              plannedPlatform = planned
              prognosedPlatform = prognosed }
        else
            { platform = prognosed |> Option.orElse planned
              plannedPlatform = planned
              prognosedPlatform = None }

    type RemarkRange =
        { remX: int
          wholeLeg: bool
          fromIndex: int
          toIndex: int }

    let private getRemarkRange (msg: FsHafas.Raw.RawMsg) (common: CommonData) (stopovers: FsHafas.Client.StopOver []) =
        let fromLoc =
            Common.getElementAtSome msg.fLocX common.locations
            |> U2StationStop.FromSomeU3StationStopLocation

        let toLoc =
            Common.getElementAtSome msg.tLocX common.locations
            |> U2StationStop.FromSomeU3StationStopLocation

        let fromIndex =
            stopovers
            |> Array.tryFindIndex (fun s -> s.stop = fromLoc)

        let toIndex =
            stopovers
            |> Array.tryFindIndex (fun s -> s.stop = toLoc)

        match msg.remX, fromIndex, toIndex with
        | Some remX, Some fromIndex, Some toIndex ->
            let wholeLeg =
                fromIndex = 0 && toIndex = stopovers.Length - 1

            Some
                { remX = remX
                  wholeLeg = wholeLeg
                  fromIndex = fromIndex
                  toIndex = toIndex }
        | _ -> None

    let private getRemarkRanges
        (msgL: FsHafas.Raw.RawMsg [])
        (commonData: CommonData)
        (stopovers: FsHafas.Client.StopOver [])
        =
        msgL
        |> Array.map (fun msg -> getRemarkRange msg commonData stopovers)
        |> Array.choose id

    let private applyRemarkRange
        (remarkRange: RemarkRange)
        (commonData: CommonData)
        (stopover: FsHafas.Client.StopOver)
        =
        let hint =
            Common.getElementAt remarkRange.remX commonData.hints

        let remarks =
            match stopover.remarks, hint with
            | Some remarks, Some hint -> remarks
            | None, Some hint when hint.IsSome -> [| hint.Value |]
            | _ -> Array.empty

        { stopover with remarks = Some remarks }

    let private applyRemarkRanges
        (commonData: CommonData)
        (stopovers: FsHafas.Client.StopOver [])
        (remarkRanges: RemarkRange [])
        =
        stopovers
        |> Array.mapi (fun i s ->
            match remarkRanges
                  |> Array.tryFind (fun r ->
                      not r.wholeLeg
                      && r.fromIndex <= i
                      && r.toIndex >= i)
                with
            | Some range -> applyRemarkRange range commonData s
            | None -> s)

    let parseJourneyLeg (ctx: Context) (pt: FsHafas.Raw.RawSec) (date: string) : FsHafas.Client.Leg =
        let mutable leg = Default.Leg

        let origin =
            Common.getElementAtSome pt.dep.locX ctx.common.locations

        let destination =
            Common.getElementAtSome pt.arr.locX ctx.common.locations

        let dep =
            ctx.profile.parseWhen ctx date pt.dep.dTimeS pt.dep.dTimeR pt.dep.dTZOffset pt.dep.dCncl

        let arr =
            ctx.profile.parseWhen ctx date pt.arr.aTimeS pt.arr.aTimeR pt.arr.aTZOffset pt.arr.aCncl

        let matchPlatfS (aPlatfS: string option) (aPltfS: FsHafas.Raw.RawPltf option) =
            match aPlatfS with
            | Some platfS -> Some platfS
            | None ->
                match aPltfS with
                | Some aPltfS -> Some aPltfS.txt
                | _ -> None

        let dPlatfS = matchPlatfS pt.dep.dPlatfS pt.dep.dPltfS
        let dPlatfR = matchPlatfS pt.dep.dPlatfR pt.dep.dPltfR

        let depPl =
            ctx.profile.parsePlatform ctx dPlatfS dPlatfR pt.dep.dCncl

        let aPlatfS = matchPlatfS pt.arr.aPlatfS pt.arr.aPltfS
        let aPlatfR = matchPlatfS pt.arr.aPlatfR pt.arr.aPltfR

        let arrPl =
            ctx.profile.parsePlatform ctx aPlatfS aPlatfR pt.arr.aCncl

        if pt.``type`` = "WALK"
           || pt.``type`` = "TRSF"
           || pt.``type`` = "DEVI" then
            leg <-
                { leg with
                    ``public`` = Some true
                    walking = Some true
                    distance =
                        match pt.gis with
                        | Some gis -> gis.dist
                        | None -> None
                    transfer = Some(pt.``type`` = "TRSF" || pt.``type`` = "DEVI") }

        if pt.``type`` = "JNY" then
            pt.jny
            |> Option.iter (fun jny ->
                let line =
                    Common.getElementAt jny.prodX ctx.common.lines

                let polyline =
                    match jny.poly with
                    | Some (value) -> Some(ctx.profile.parsePolyline ctx value)
                    | None ->
                        match jny.polyG with
                        | Some polyG ->
                            let idx = polyG.polyXL.[0]

                            if idx < ctx.common.polylines.Length then
                                Some ctx.common.polylines.[idx]
                            else
                                None
                        | None -> None

                let stopovers =
                    match ctx.opt.stopovers with
                    | true -> ctx.profile.parseStopovers ctx jny.stopL date
                    | _ -> None

                let remarkRanges =
                    match jny.msgL, stopovers with
                    | Some msgL, Some stopovers -> getRemarkRanges msgL ctx.common stopovers
                    | _ -> Array.empty

                let stopoversWithRemarks =
                    match stopovers with
                    | Some stopovers ->
                        applyRemarkRanges ctx.common stopovers remarkRanges
                        |> Some
                    | _ -> None

                let msgL =
                    match jny.msgL with
                    | Some msgL ->
                        msgL
                        |> Array.filter (fun msg -> // remove msgs used in stopoversWithRemarks
                            remarkRanges
                            |> Array.exists (fun r -> Some r.remX = msg.remX && not r.wholeLeg)
                            |> not)
                        |> Some
                    | None -> None

                let currentLocation: Location option =
                    match jny.pos with
                    | Some pos ->
                        Some
                            { Default.Location with
                                longitude = Some(float pos.x / 1000000.0)
                                latitude = Some(float pos.y / 1000000.0) }
                    | None -> None

                let remarks =
                    if ctx.opt.remarks then
                        Common.msgLToRemarks ctx msgL
                    else
                        None

                let cycle =
                    match jny.freq with
                    | Some freq ->
                        match freq.minC, freq.maxC with
                        | Some minC, Some maxC ->
                            { min = Some(minC * 60)
                              max = Some(maxC * 60)
                              nr = freq.numC }
                            |> Some
                        | Some minC, _ ->
                            { min = Some(minC * 60)
                              max = None
                              nr = None }
                            |> Some
                        | _ -> None
                    | None -> None

                let parseAlternative (a: FsHafas.Raw.RawJny) : Alternative =
                    let line =
                        Common.getElementAt a.prodX ctx.common.lines

                    let parsedWhen =
                        match a.stopL with
                        | Some stopL when stopL.Length > 0 ->
                            let st0 = stopL.[0]
                            Some(ctx.profile.parseWhen ctx date st0.dTimeS st0.dTimeR st0.dTZOffset st0.dCncl)
                        | _ -> None

                    { Default.Alternative with
                        tripId = a.jid
                        line = line
                        direction = a.dirTxt
                        ``when`` = parsedWhen |> Option.bind (fun v -> v.``when``)
                        plannedWhen = parsedWhen |> Option.bind (fun v -> v.plannedWhen)
                        prognosedWhen =
                            parsedWhen
                            |> Option.bind (fun v -> v.prognosedWhen)
                        delay = parsedWhen |> Option.bind (fun v -> v.delay) }

                let alternatives =
                    jny.freq
                    |> Option.bind (fun freq ->
                        freq.jnyL
                        |> Option.map (Array.map parseAlternative))

                leg <-
                    { leg with
                        tripId = Some jny.jid
                        direction = jny.dirTxt
                        reachable = jny.isRchbl
                        line = line
                        stopovers = stopoversWithRemarks
                        polyline = polyline
                        remarks = remarks
                        currentLocation = currentLocation
                        cycle = cycle
                        alternatives = alternatives })

        { leg with
            origin = origin
            destination = destination
            departure = dep.``when``
            departureDelay = dep.delay
            plannedDeparture = dep.plannedWhen
            prognosedDeparture = dep.prognosedWhen
            departurePlatform = depPl.platform
            plannedDeparturePlatform = depPl.plannedPlatform
            prognosedDeparturePlatform = depPl.prognosedPlatform
            arrival = arr.``when``
            arrivalDelay = arr.delay
            plannedArrival = arr.plannedWhen
            prognosedArrival = arr.prognosedWhen
            arrivalPlatform = arrPl.platform
            plannedArrivalPlatform = arrPl.plannedPlatform
            prognosedArrivalPlatform = arrPl.prognosedPlatform
            cancelled = pt.dep.dCncl |> Option.orElse pt.arr.aCncl }
