namespace FsHafas.Parser

module internal ArrivalOrDeparture =

    open FsHafas.Client
    open FsHafas.Endpoint

    let DEP = "DEP"
    let ARR = "ARR"

    let private parseDepartureArrival
        (``type``: string)
        (ctx: Context)
        (d: FsHafas.Raw.RawJny)
        : FsHafas.Client.Alternative =

        let (locX, xTimeS, xTimeR, xTZOffset, xCncl, xPlatfS, xPlatfR, xPltfS, xPltfR) =
            if ``type`` = DEP then
                let dep = RawDep.FromRawStopL d.stbStop.Value

                (dep.locX,
                 dep.dTimeS,
                 dep.dTimeR,
                 dep.dTZOffset,
                 dep.dCncl,
                 dep.dPlatfS,
                 dep.dPlatfR,
                 dep.dPltfS,
                 dep.dPltfR)
            else
                let arr = RawArr.FromRawStopL d.stbStop.Value

                (arr.locX,
                 arr.aTimeS,
                 arr.aTimeR,
                 arr.aTZOffset,
                 arr.aCncl,
                 arr.aPlatfS,
                 arr.aPlatfR,
                 arr.aPltfS,
                 arr.aPltfR)

        let stop =
            Common.getElementAtSome locX ctx.common.locations
            |> U2StationStop.FromSomeU3StationStopLocation

        let w = ctx.profile.parseWhen ctx d.date.Value xTimeS xTimeR xTZOffset xCncl

        let matchPlatfS (aPlatfS: string option) (aPltfS: FsHafas.Raw.RawPltf option) =
            match aPlatfS with
            | Some platfS -> Some platfS
            | None ->
                match aPltfS with
                | Some aPltfS -> Some aPltfS.txt
                | _ -> None

        let platfS = matchPlatfS xPlatfS xPltfS
        let platfR = matchPlatfS xPlatfR xPltfR

        let plt = ctx.profile.parsePlatform ctx platfS platfR xCncl

        let filter (s: FsHafas.Client.StopOver) =
            match s.passBy with
            | Some (passBy) -> not passBy
            | None -> true

        let stopovers =
            ctx.profile.parseStopovers ctx d.stopL d.date.Value
            |> Option.map (fun st -> st |> Array.filter filter)

        let currentTripPosition: Location option =
            match d.pos with
            | Some pos ->
                Some
                    { Default.Location with
                        longitude = Some(float pos.x / 1000000.0)
                        latitude = Some(float pos.y / 1000000.0) }
            | None -> None

        let destination =
            if ``type`` = DEP then
                match d.prodL, ctx.res.common with
                | Some prodL, Some common when prodL.Length > 0 && common.locL.IsSome ->
                    let loc = Common.getElementAtSome prodL.[0].tLocX common.locL.Value

                    ctx.profile.parseLocations ctx [| loc.Value |]
                    |> Array.tryHead
                | _ -> None
            else
                None

        let origin =
            if ``type`` = ARR then
                match d.prodL, ctx.res.common with
                | Some prodL, Some common when prodL.Length > 0 && common.locL.IsSome ->
                    let loc = Common.getElementAtSome prodL.[0].fLocX common.locL.Value

                    ctx.profile.parseLocations ctx [| loc.Value |]
                    |> Array.tryHead
                | _ -> None
            else
                None

        let remarks =
            if ctx.opt.remarks then
                let stopMsgL = d.stbStop |> Option.fold (fun _ s -> s.msgL) None

                Common.msgLToRemarks ctx (Common.appendSomeArray d.msgL stopMsgL)
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
            remarks = remarks
            currentTripPosition = currentTripPosition
            origin = origin
            destination = destination }

    let parseDeparture (ctx: Context) (d: FsHafas.Raw.RawJny) : FsHafas.Client.Alternative =
        parseDepartureArrival DEP ctx d

    let parseArrival (ctx: Context) (d: FsHafas.Raw.RawJny) : FsHafas.Client.Alternative =
        parseDepartureArrival ARR ctx d
