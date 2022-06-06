namespace FsHafas.Parser

module internal DateTime =

    open FsHafas.Endpoint

#if FABLE_COMPILER
    open Fable.Core

#if FABLE_PY
    [<Emit("$0.isoformat()")>]
    let isoformat (dt: obj) : string = jsNative
#else
#if !WEBPACK
    [<Import("default", from = "luxon")>]
#endif

    [<Emit("luxon.DateTime.fromObject({ year: $1, month: $2, day: $3}).setZone(new luxon.IANAZone($4)).offset")>]
    let OffsetFromObject (year: int) (month: int) (day: int) (zone: string) : int = jsNative
#endif

#endif

#if FABLE_PY
    // workaround: missing code in DateTime and DateTimeOffset
    open FsHafas.Extensions

    let private parseDateTimeWithOffset
        (
            profile: Profile,
            year,
            month,
            day,
            hour,
            minute,
            seconds,
            tzOffset: int option
        ) =
        try
            let dt = System.DateTime(year, month, day, hour, minute, seconds)

            match tzOffset with
            | Some tzOffset -> DateTimeOffsetEx.DateTimeOffsetEx(dt, Some(tzOffset * 60), None)
            | None -> DateTimeOffsetEx.DateTimeOffsetEx(dt, None, None)
        with
        | ex ->
            printfn "error parseDateTimeWithOffset: %s" ex.Message
            raise (System.Exception(ex.Message))

    let private ToIsoString (dto: DateTimeOffsetEx.DateTimeOffsetEx) =
        let dt = dto.DateTime
        isoformat (dt)

#else

    let private GetDateTimeInZone
        (
            year: int,
            month: int,
            day: int,
            hour: int,
            minute: int,
            seconds: int,
            zoneId: string
        ) =

#if FABLE_COMPILER
        let tzOffset = OffsetFromObject year month day zoneId

        System.DateTimeOffset(year, month, day, hour, minute, seconds, System.TimeSpan(tzOffset / 60, 0, 0))
#else

        let dt = NodaTime.LocalDateTime(year, month, day, hour, minute, seconds)

        let zdt = dt.InZoneLeniently(NodaTime.DateTimeZoneProviders.Tzdb.[zoneId])

        zdt.ToDateTimeOffset()
#endif

    /// expected format 'yyyyMMdd' and 'HHmmss'
    let private parseDateTimeWithOffset
        (
            profile: Profile,
            year,
            month,
            day,
            hour,
            minute,
            seconds,
            tzOffset: int option
        ) =

        match tzOffset with
        | Some tzOffset ->
            System.DateTimeOffset(year, month, day, hour, minute, seconds, System.TimeSpan(tzOffset / 60, 0, 0))
        | None ->
            GetDateTimeInZone(year, month, day, hour, minute, seconds, (profile :> FsHafas.Client.Profile).timezone)

    let private ToIsoString (dt: System.DateTimeOffset) =
        sprintf
            "%04d-%02d-%02dT%02d:%02d:%02d+%02d:00"
            dt.Year
            dt.Month
            dt.Day
            dt.Hour
            dt.Minute
            dt.Second
            ((dt.Offset.TotalMinutes |> int) / 60)
#endif

    let parseDateTimeEx
        (profile: FsHafas.Endpoint.Profile)
        (date: string)
        (time: string option)
        (tzOffset: int option)
        : (System.DateTime option * string option) =
        try
            match time with
            | Some (time) ->
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
                    parseDateTimeWithOffset(profile, year, month, day, hour, minute, seconds, tzOffset)
                        .AddDays(daysOffset)

                (Some dto.DateTime, Some(ToIsoString dto))
            | None -> (None, None)

        with
        | ex ->
            printfn "parseDateTimeEx: %A" ex
            (None, None)

    let parseDateTime (ctx: Context) (date: string) (time: string option) (tzOffset: int option) : string option =
        let (_, s) = parseDateTimeEx ctx.profile date time tzOffset

        s
