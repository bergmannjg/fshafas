namespace FsHafas.Parser

module internal ScheduledDays =

    open FsHafas.Client
    open FsHafas.Endpoint
    open FsHafas.Raw

    let private bytes =
        [| 0..7 |]
        |> Array.map (fun x -> 1 <<< (7 - x) |> byte)

    let parseScheduledDays (ctx: Context) (sDays: RawSDays) : ScheduledDays option =
        match ctx.res.fpB, sDays.sDaysB with
        | Some fpB, Some sDaysB when fpB.Length = 8 ->
            let year = fpB.Substring(0, 4) |> int
            let month = fpB.Substring(4, 2) |> int
            let day = fpB.Substring(6, 2) |> int
            let mutable dt = System.DateTime(year, month, day)

            FsHafas.Extensions.ConvertEx.FromHexString sDaysB
            |> fun arr -> // trim array
                match arr |> Array.tryFindIndex (fun b -> b <> 0uy), arr |> Array.tryFindIndexBack (fun b -> b <> 0uy)
                    with
                | Some first, Some last ->
                    if first = 0 && last + 1 = arr.Length then
                        arr
                    else
                        dt <- FsHafas.Extensions.DateTimeEx.addDays (dt, first * 8)
                        arr.[first..last]
                | _ -> [||]
            |> Seq.fold
                (fun (m: IndexMap<string, bool>) d ->
                    bytes
                    |> Array.fold
                        (fun (m: IndexMap<string, bool>) b ->
                            m.Item(dt.ToString("yyyy-MM-dd")) <- d &&& b <> 0uy
                            dt <- FsHafas.Extensions.DateTimeEx.addDays (dt, 1)
                            m)
                        m)
                (IndexMap<string, bool>(false))
            |> Some
        | _ -> None
