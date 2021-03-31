namespace FsHafas.Parser

module internal JourneyLeg =

    open FsHafas

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

    let private getRemarkRange (msg: Raw.RawMsg) (common: CommonData) (stopovers: Client.StopOver []) =
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

    let private getRemarkRanges (msgL: Raw.RawMsg []) (commonData: CommonData) (stopovers: Client.StopOver []) =
        msgL
        |> Array.map (fun msg -> getRemarkRange msg commonData stopovers)
        |> Array.choose id

    let private applyRemarkRange (remarkRange: RemarkRange) (commonData: CommonData) (stopover: Client.StopOver) =
        let hint =
            Common.getElementAt remarkRange.remX commonData.hints

        let remarks =
            match stopover.remarks, hint with
            | Some remarks, Some hint -> remarks
            | None, Some hint -> [| hint |]
            | _ -> Array.empty

        { stopover with remarks = Some remarks }

    let private applyRemarkRanges
        (commonData: CommonData)
        (stopovers: Client.StopOver [])
        (remarkRanges: RemarkRange [])
        =
        stopovers
        |> Array.mapi
            (fun i s ->
                match remarkRanges
                      |> Array.tryFind
                          (fun r ->
                              not r.wholeLeg
                              && r.fromIndex <= i
                              && r.toIndex >= i) with
                | Some range -> applyRemarkRange range commonData s
                | None -> s)

    let parseJourneyLeg (ctx: Context) (pt: Raw.RawSec) (date: string) : Client.Leg =
        let mutable leg = Default.Leg

        let origin =
            Common.getElementAtSome pt.dep.locX ctx.common.locations

        let destination =
            Common.getElementAtSome pt.arr.locX ctx.common.locations

        let dep =
            ctx.profile.parseWhen ctx date pt.dep.dTimeS pt.dep.dTimeR pt.dep.dTZOffset pt.dep.dCncl

        let arr =
            ctx.profile.parseWhen ctx date pt.arr.aTimeS pt.arr.aTimeR pt.arr.aTZOffset pt.arr.aCncl

        let depPl =
            ctx.profile.parsePlatform ctx pt.dep.dPlatfS pt.dep.dPlatfR pt.dep.dCncl

        let arrPl =
            ctx.profile.parsePlatform ctx pt.arr.aPlatfS pt.arr.aPlatfR pt.arr.aCncl

        if pt.``type`` = "WALK"
           || pt.``type`` = "TRSF"
           || pt.``type`` = "DEVI" then
            leg <-
                { leg with
                      ``public`` = Some true
                      walking = Some true
                      transfer = Some(pt.``type`` = "TRSF" || pt.``type`` = "DEVI") }

        if pt.``type`` = "JNY" then
            pt.jny
            |> Option.iter
                (fun jny ->
                    let line =
                        Common.getElementAt jny.prodX ctx.common.lines

                    let polyline =
                        match jny.poly with
                        | Some (value) -> Some(ctx.profile.parsePolyline ctx value)
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
                            |> Array.filter // remove msgs used in stopoversWithRemarks
                                (fun msg ->
                                    remarkRanges
                                    |> Array.exists (fun r -> Some r.remX = msg.remX && not r.wholeLeg)
                                    |> not)
                            |> Some
                        | None -> None

                    leg <-
                        { leg with
                              tripId = Some jny.jid
                              direction = jny.dirTxt
                              reachable = jny.isRchbl
                              line = line
                              stopovers = stopoversWithRemarks
                              polyline = polyline
                              remarks = Common.msgLToRemarks ctx msgL })

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
