namespace FsHafas.Parser

module internal Stopover =

    open FsHafas.Client
    open FsHafas.Endpoint

    let defaultStopover : FsHafas.Client.StopOver =
        { stop = None
          departure = None
          departureDelay = None
          prognosedDeparture = None
          plannedDeparture = None
          departurePlatform = None
          prognosedDeparturePlatform = None
          plannedDeparturePlatform = None
          /// null, if first stopOver of trip
          arrival = None
          arrivalDelay = None
          prognosedArrival = None
          plannedArrival = None
          arrivalPlatform = None
          prognosedArrivalPlatform = None
          plannedArrivalPlatform = None
          remarks = None
          passBy = None
          cancelled = None }

    let parseStopover (ctx: Context) (st: FsHafas.Raw.RawStop) (date: string) : FsHafas.Client.StopOver =
        let stop =
            Common.getElementAt st.locX ctx.common.locations
            |> U2StationStop.FromSomeU3StationStopLocation

        let dep =
            ctx.profile.parseWhen ctx date st.dTimeS st.dTimeR st.dTZOffset st.dCncl

        let arr =
            ctx.profile.parseWhen ctx date st.aTimeS st.aTimeR st.aTZOffset st.aCncl

        let depPl =
            ctx.profile.parsePlatform ctx st.dPlatfS st.dPlatfR st.dCncl

        let arrPl =
            ctx.profile.parsePlatform ctx st.aPlatfS st.aPlatfR st.aCncl

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
              passBy = passBy }

    let parseStopovers (ctx: Context) (stopL: FsHafas.Raw.RawStop [] option) (date: string) : FsHafas.Client.StopOver [] option =
        match stopL with
        | Some (stopL) ->
            stopL
            |> Array.filter
                (fun s ->
                    (Common.getElementAt s.locX ctx.common.locations)
                        .IsSome)
            |> Array.map (fun s -> ctx.profile.parseStopover ctx s date)
            |> Some
        | None -> None
