namespace FsHafas.Parser

module internal Journey =

    open FsHafas.Client
    open FsHafas.Endpoint

    let bytes =
        [| 0..7 |]
        |> Array.map (fun x -> 1 <<< (7 - x) |> byte)

    let parseJourney (ctx: Context) (j: FsHafas.Raw.RawOutCon) : Journey =
        let legs =
            j.secL
            |> Array.map (fun l -> ctx.profile.parseJourneyLeg ctx l j.date)

        let remarks =
            if ctx.opt.remarks then
                Common.msgLToRemarks ctx j.msgL
                |> Option.defaultValue Array.empty
                |> Some
            else
                None

        let scheduledDays =
            match ctx.opt.scheduledDays with
            | true -> ctx.profile.parseScheduledDays ctx j.sDays
            | _ -> None

        let cycle =
            match j.freq with
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

        let refreshToken =
            match j.ctxRecon with
            | Some _ -> j.ctxRecon
            | None ->
                match j.recon with
                | Some recon -> recon.ctx
                | None -> None

        { Default.Journey with
            legs = legs
            refreshToken = refreshToken
            remarks = remarks
            scheduledDays = scheduledDays
            cycle = cycle }

    let distanceOfJourney (j: Journey) =
        j.legs
        |> Array.choose (fun l -> l.polyline)
        |> Array.sumBy Polyline.distanceOfFeatureCollection
