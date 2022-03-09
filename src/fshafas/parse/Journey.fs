namespace FsHafas.Parser

module internal Journey =

    open FsHafas.Client
    open FsHafas.Endpoint

    let bytes =
        [| 0 .. 7 |]
        |> Array.map (fun x -> 1 <<< (7 - x) |> byte)

#if FABLE_JS
    open Fable.Core

    [<Emit("Buffer.from($0, 'hex')")>]
    let FromHexString (s: string) = [||]
#else
#if FABLE_PY
    open Fable.Core

    [<Emit("bytes.fromhex($0)")>]
    let FromHexString (s: string) : byte [] = jsNative
#else
    let FromHexString (s: string) = System.Convert.FromHexString s
#endif
#endif

    let private parseScheduledDays (ctx: Context) (sDays: string) (year: int) =

        let mutable dt = System.DateTime(year, 1, 1)

        FromHexString sDays
        |> Seq.fold
            (fun (m: IndexMap<string, bool>) d ->
                bytes
                |> Array.fold
                    (fun (m: IndexMap<string, bool>) b ->
                        m.Item(dt.ToString("yyyy-MM-dd")) <- d &&& b <> 0uy
#if FABLE_PY
                        dt <- FsHafas.Extensions.DateTimeEx.addDays (dt, 1)
#else
                        dt <- dt.AddDays(1.0)
#endif
                        m)
                    m)
            (IndexMap<string, bool>(false))

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
            match ctx.opt.scheduledDays, j.sDays.sDaysB with
            | true, Some sDaysB ->
                parseScheduledDays ctx sDaysB (j.date.Substring(0, 4) |> int)
                |> Some
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

        { Default.Journey with
            legs = legs
            refreshToken = Some j.ctxRecon
            remarks = remarks
            scheduledDays = scheduledDays
            cycle = cycle }

    let distanceOfJourney (j: Journey) =
        j.legs
        |> Array.choose (fun l -> l.polyline)
        |> Array.sumBy Polyline.distanceOfFeatureCollection
