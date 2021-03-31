namespace FsHafas.Parser

module internal DateTime =

    open FsHafas

#if FABLE_COMPILER
    open Fable.Core

#if WEBPACK
    [<ImportAll("luxon")>]
#else
    [<Import("default", from = "luxon")>]
#endif
    [<Emit("luxon.DateTime.fromObject({ year: $1, month: $2, day: $3}).setZone(new luxon.IANAZone($4)).offset")>]
    let OffsetFromObject (year: int) (month: int) (day: int) (zone: string) : int = jsNative
#endif

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

        let dt =
            NodaTime.LocalDateTime(year, month, day, hour, minute, seconds)

        let zdt =
            dt.InZoneLeniently(NodaTime.DateTimeZoneProviders.Tzdb.[zoneId])

        zdt.ToDateTimeOffset()
#endif

    /// expected format 'yyyyMMdd' and 'HHmmss'
    let private ParseString (profile: Profile) (date: string) (time: string) (tzOffset: int option) =
        let year = date.Substring(0, 4) |> int
        let month = date.Substring(4, 2) |> int
        let day = date.Substring(6, 2) |> int
        let hour = time.Substring(0, 2) |> int
        let minute = time.Substring(2, 2) |> int
        let seconds = time.Substring(4, 2) |> int

        match tzOffset with
        | Some tzOffset ->
            System.DateTimeOffset(year, month, day, hour, minute, seconds, System.TimeSpan(tzOffset / 60, 0, 0))
        | None -> GetDateTimeInZone(year, month, day, hour, minute, seconds, profile.timezone)

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

    let ParseIsoString (datetime: string) =
        let year = datetime.Substring(0, 4) |> int
        let month = datetime.Substring(5, 2) |> int
        let day = datetime.Substring(8, 2) |> int
        let hour = datetime.Substring(11, 2) |> int
        let minute = datetime.Substring(14, 2) |> int

        let tzOffset =
            datetime.Substring(20, 2) |> int |> (*) 60

        System.DateTimeOffset(year, month, day, hour, minute, 0, System.TimeSpan(tzOffset / 60, 0, 0))

    let parseDateTimeEx
        (profile: Profile)
        (date: string)
        (time: string option)
        (tzOffset: int option)
        : (System.DateTime option * string option) =
        try
            match time with
            | Some (time) ->
                let daysOffset =
                    if time.Length > 6 then
                        time.Substring(0, 2) |> float
                    else
                        0.0

                let dto =
                    (ParseString profile date (time.Substring(time.Length - 6, 6)) tzOffset)
                        .AddDays(daysOffset)

                (Some dto.DateTime, Some(ToIsoString dto))
            | None -> (None, None)

        with ex ->
            printfn "parseDateTimeEx: %A" ex
            (None, None)

    let parseDateTime (ctx: Context) (date: string) (time: string option) (tzOffset: int option) : string option =
        let (_, s) =
            parseDateTimeEx ctx.profile date time tzOffset

        s
