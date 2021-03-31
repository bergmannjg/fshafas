namespace FsHafas.Parser

module internal ArrivalOrDeparture =

    open FsHafas

    let DEP = "DEP"
    let ARR = "ARR"

    let private parseDepartureArrival (``type``: string) (ctx: Context) (d: Raw.RawJny) : Client.Alternative =

        let (locX, xTimeS, xTimeR, xTZOffset, xCncl, xPlatfS, xPlatfR) =
            if ``type`` = DEP then
                let dep = RawDep.FromRawStopL d.stbStop.Value
                (dep.locX, dep.dTimeS, dep.dTimeR, dep.dTZOffset, dep.dCncl, dep.dPlatfS, dep.dPlatfR)
            else
                let arr = RawArr.FromRawStopL d.stbStop.Value
                (arr.locX, arr.aTimeS, arr.aTimeR, arr.aTZOffset, arr.aCncl, arr.aPlatfS, arr.aPlatfR)

        let stop =
            Common.getElementAtSome locX ctx.common.locations
            |> U2StationStop.FromSomeU3StationStopLocation

        let w =
            ctx.profile.parseWhen ctx d.date.Value xTimeS xTimeR xTZOffset xCncl

        let plt =
            ctx.profile.parsePlatform ctx xPlatfS xPlatfR xCncl

        let filter (s: Client.StopOver) =
            match s.passBy with
            | Some (passBy) -> not passBy
            | None -> true

        let stopovers =
            ctx.profile.parseStopovers ctx d.stopL d.date.Value
            |> Option.map (fun st -> st |> Array.filter filter)

        let remarks =
            if ctx.opt.remarks then
                Common.msgLToRemarks ctx d.msgL
                |> Option.defaultValue Array.empty
                |> Some
            else
                None

        { Default.Alternative with
              tripId = d.jid
              stop = stop
              ``when`` = w.``when``
              plannedWhen = w.plannedWhen
              prognosedWhen = w.prognosedWhen
              delay = w.delay
              platform = plt.platform
              plannedPlatform = plt.plannedPlatform
              prognosedPlatform = plt.prognosedPlatform
              direction = Some d.dirTxt.Value
              provenance = None
              line = Common.getElementAt d.prodX ctx.common.lines
              cancelled = d.stbStop.Value.dCncl
              nextStopovers = stopovers
              remarks = remarks }

    let parseDeparture (ctx: Context) (d: Raw.RawJny) : Client.Alternative = parseDepartureArrival DEP ctx d

    let parseArrival (ctx: Context) (d: Raw.RawJny) : Client.Alternative = parseDepartureArrival ARR ctx d
