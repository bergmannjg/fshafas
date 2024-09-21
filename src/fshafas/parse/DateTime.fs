namespace FsHafas.Parser

module internal DateTime =

    open FsHafas.Endpoint

    /// expected format 'yyyyMMdd' and 'HHmmss'
    let parseDateTimeEx
        (profile: FsHafas.Endpoint.Profile)
        (date: string)
        (time: string option)
        (tzOffset: int option)
        : (System.DateTime option * string option) =
        try
            match time with
            | Some(time) ->
                let time6, daysOffset =
                    if time.Length > 6 then
                        (time.Substring(time.Length - 6, 6), time.Substring(0, 2) |> float)
                    else
                        (time, 0.0)

                let year = date.Substring(0, 4) |> int
                let month = date.Substring(4, 2) |> int
                let day = date.Substring(6, 2) |> int
                let hour = time6.Substring(0, 2) |> int
                let minute = time6.Substring(2, 2) |> int
                let seconds = time6.Substring(4, 2) |> int

                let dto =
                    FsHafas.Extensions.DateTimeOffsetEx
                        .parseDateTimeWithOffset(profile, year, month, day, hour, minute, seconds, tzOffset)
                        .AddDays(daysOffset)

                (Some dto.DateTime, Some(FsHafas.Extensions.DateTimeOffsetEx.ToIsoString dto))
            | None -> (None, None)

        with ex ->
            printfn "parseDateTimeEx: %A" ex
            (None, None)

    let parseDateTime (ctx: Context) (date: string) (time: string option) (tzOffset: int option) : string option =
        let (_, s) = parseDateTimeEx ctx.profile date time tzOffset

        s
