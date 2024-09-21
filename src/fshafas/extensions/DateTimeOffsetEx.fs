namespace FsHafas.Extensions

module internal DateTimeOffsetEx =

#if FABLE_COMPILER
    open Fable.Core
#endif

#if FABLE_JS
    [<Import("DateTime", from = "luxon")>]
#endif
#if FABLE_JS
    [<Emit("DateTime.fromObject({ year: $1, month: $2, day: $3})")>]
    let OffsetFromObject (year: int) (month: int) (day: int) : obj = jsNative
#endif

#if FABLE_JS
    [<Import("IANAZone", from = "luxon")>]
#endif
#if FABLE_JS
    [<Emit("$1.setZone(new IANAZone($2)).offset")>]
    let SetZone (dt: obj) (zone: string) : int = jsNative
#endif

#if FABLE_PY
    [<ImportAll("datetime")>]

    [<Import("tz", from = "dateutil")>]
    [<Emit("tz_1.gettz($1)")>]
    let private gettz (tz: string) : obj = jsNative

    [<Import("tz", from = "dateutil")>]
    [<Emit("tz_1.tzoffset(None, $1)")>]
    let private mktzoffset (seconds: int) : obj = jsNative

    [<Emit("datetime.datetime($0, $1, $2, $3, $4, $5, tzinfo=$6)")>]
    let private mkDateTime
        (year: int)
        (month: int)
        (day: int)
        (hour: int)
        (minute: int)
        (second: int)
        (tzinfo: obj)
        : System.DateTime =
        jsNative

    [<Emit("print($0)")>]
    let print (_: obj) : unit = jsNative

    type DateTimeOffsetEx(dt: System.DateTime, tzoffsetArg: int option, tzArg: string option) =
        let getDateTime () =
            match tzoffsetArg, tzArg with
            | Some(tzoffsetArg), _ ->
                mkDateTime
                    (DateTimeEx.year dt)
                    (DateTimeEx.month dt)
                    (DateTimeEx.day dt)
                    (DateTimeEx.hour dt)
                    (DateTimeEx.minute dt)
                    (DateTimeEx.second dt)
                    (mktzoffset tzoffsetArg)
            | _, Some(tz) ->
                mkDateTime
                    (DateTimeEx.year dt)
                    (DateTimeEx.month dt)
                    (DateTimeEx.day dt)
                    (DateTimeEx.hour dt)
                    (DateTimeEx.minute dt)
                    (DateTimeEx.second dt)
                    (gettz tz)
            | _ ->
                mkDateTime
                    (DateTimeEx.year dt)
                    (DateTimeEx.month dt)
                    (DateTimeEx.day dt)
                    (DateTimeEx.hour dt)
                    (DateTimeEx.minute dt)
                    (DateTimeEx.second dt)
                    (gettz "Europe/Berlin")

        member __.DateTime = getDateTime ()

        member __.AddDays(days: float) =
            DateTimeOffsetEx(DateTimeEx.addDays (dt, days |> int), tzoffsetArg, tzArg)

#endif

#if FABLE_PY
    let parseDateTimeWithOffset
        (profile: FsHafas.Endpoint.Profile, year, month, day, hour, minute, seconds, tzOffset: int option)
        : DateTimeOffsetEx =
        try
            let dt = System.DateTime(year, month, day, hour, minute, seconds)

            match tzOffset with
            | Some tzOffset -> DateTimeOffsetEx(dt, Some(tzOffset * 60), None)
            | None -> DateTimeOffsetEx(dt, None, None)
        with ex ->
            printfn "error parseDateTimeWithOffset: %s" ex.Message
            raise (System.Exception(ex.Message))
#else
    let parseDateTimeWithOffset
        (profile: FsHafas.Endpoint.Profile, year, month, day, hour, minute, seconds, tzOffset: int option)
        : System.DateTimeOffset =

        let GetDateTimeInZone (year: int, month: int, day: int, hour: int, minute: int, seconds: int, zoneId: string) =

#if FABLE_JS
            let tzOffset = SetZone (OffsetFromObject year month day) zoneId

            System.DateTimeOffset(year, month, day, hour, minute, seconds, System.TimeSpan(tzOffset / 60, 0, 0))
#else

            let dt = NodaTime.LocalDateTime(year, month, day, hour, minute, seconds)

            let zdt = dt.InZoneLeniently(NodaTime.DateTimeZoneProviders.Tzdb.[zoneId])

            zdt.ToDateTimeOffset()

#endif

        match tzOffset with
        | Some tzOffset ->
            System.DateTimeOffset(year, month, day, hour, minute, seconds, System.TimeSpan(tzOffset / 60, 0, 0))
        | None ->
            GetDateTimeInZone(year, month, day, hour, minute, seconds, (profile :> FsHafas.Client.Profile).timezone)
#endif

#if FABLE_PY
    [<Emit("$0.isoformat()")>]
    let private isoformat (dt: obj) : string = jsNative

    let ToIsoString (dto: DateTimeOffsetEx) =
        let dt = dto.DateTime
        isoformat (dt)
#else
    let ToIsoString (dt: System.DateTimeOffset) =
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
