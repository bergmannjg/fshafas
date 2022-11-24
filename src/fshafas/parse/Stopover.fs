namespace FsHafas.Parser

module internal Stopover =

    open FsHafas.Client
    open FsHafas.Endpoint

    let defaultStopover: FsHafas.Client.StopOver =
        { stop = None
          departure = None
          departureDelay = None
          prognosedDeparture = None
          plannedDeparture = None
          departurePlatform = None
          prognosedDeparturePlatform = None
          plannedDeparturePlatform = None
          arrival = None
          arrivalDelay = None
          prognosedArrival = None
          plannedArrival = None
          arrivalPlatform = None
          prognosedArrivalPlatform = None
          plannedArrivalPlatform = None
          remarks = None
          passBy = None
          cancelled = None
          departurePrognosisType = None
          arrivalPrognosisType = None }

    let parseStopover (ctx: Context) (st: FsHafas.Raw.RawStop) (date: string) : FsHafas.Client.StopOver =
        let stop =
            Common.getElementAtSome st.locX ctx.common.locations
            |> U2StationStop.FromSomeU3StationStopLocation

        let dep = ctx.profile.parseWhen ctx date st.dTimeS st.dTimeR st.dTZOffset st.dCncl

        let arr = ctx.profile.parseWhen ctx date st.aTimeS st.aTimeR st.aTZOffset st.aCncl

        let matchPlatfS (aPlatfS: string option) (aPltfS: FsHafas.Raw.RawPltf option) =
            match aPlatfS with
            | Some platfS -> Some platfS
            | None ->
                match aPltfS with
                | Some aPltfS -> Some aPltfS.txt
                | _ -> None

        let dPlatfS = matchPlatfS st.dPlatfS st.dPltfS
        let dPlatfR = matchPlatfS st.dPlatfR st.dPltfR

        let depPl = ctx.profile.parsePlatform ctx dPlatfS dPlatfR st.dCncl

        let aPlatfS = matchPlatfS st.aPlatfS st.aPltfS
        let aPlatfR = matchPlatfS st.aPlatfR st.aPltfR

        let arrPl = ctx.profile.parsePlatform ctx aPlatfS aPlatfR st.aCncl

        let passBy =
            match st.dInS, st.aOutS with
            | Some false, Some false -> Some true
            | _ -> None

        { defaultStopover with
            stop = stop
            arrival = arr.``when``
            plannedArrival = arr.plannedWhen
            arrivalDelay = arr.delay
            arrivalPlatform = arrPl.platform
            plannedArrivalPlatform = arrPl.plannedPlatform
            prognosedArrivalPlatform = arrPl.prognosedPlatform
            departure = dep.``when``
            plannedDeparture = dep.plannedWhen
            departureDelay = dep.delay
            departurePlatform = depPl.platform
            plannedDeparturePlatform = depPl.plannedPlatform
            prognosedDeparturePlatform = depPl.prognosedPlatform

            cancelled = st.aCncl |> Option.orElse st.dCncl
            remarks = Common.msgLToRemarks ctx st.msgL
            passBy = passBy
            arrivalPrognosisType = ctx.profile.parsePrognosisType ctx st.aProgType
            departurePrognosisType = ctx.profile.parsePrognosisType ctx st.dProgType }

    let parseStopovers
        (ctx: Context)
        (stopL: FsHafas.Raw.RawStop [] option)
        (date: string)
        : FsHafas.Client.StopOver [] option =
        match stopL with
        | Some (stopL) ->
            stopL
            |> Array.filter (fun s ->
                (Common.getElementAtSome s.locX ctx.common.locations)
                    .IsSome)
            |> Array.map (fun s -> ctx.profile.parseStopover ctx s date)
            |> Some
        | None -> None
