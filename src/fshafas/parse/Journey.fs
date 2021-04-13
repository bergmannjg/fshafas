namespace FsHafas.Parser

module internal Journey =

    open FsHafas.Client

    let bytes =
        [| 0 .. 7 |]
        |> Array.map (fun x -> 1 <<< (7 - x) |> byte)

#if FABLE_COMPILER
    open Fable.Core

    [<Emit("Buffer.from($0, 'hex')")>]
    let FromHexString (s: string) = [||]
#else
    let FromHexString (s: string) = System.Convert.FromHexString s
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
                        dt <- dt.AddDays(1.0)

                        m)
                    m)
            (IndexMap<string, bool>(false))

    let parseJourney (ctx: Context) (j: FsHafas.Raw.RawOutCon) : Journey =
        let legs =
            j.secL
            |> Array.map (fun l -> ctx.profile.parseJourneyLeg ctx l j.date)

        let remarks =
            Common.msgLToRemarks ctx j.msgL
            |> Option.defaultValue Array.empty
            |> Some

        let scheduledDays =
            match j.sDays.sDaysB with
            | Some sDaysB ->
                parseScheduledDays ctx sDaysB (j.date.Substring(0, 4) |> int)
                |> Some
            | None -> None

        let cycle =
            match j.freq with
            | Some freq ->
                match freq.minC, freq.maxC with
                | Some minC, Some maxC ->
                    { min = Some(minC * 60)
                      max = Some(maxC * 60)
                      nr = freq.numC }
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
