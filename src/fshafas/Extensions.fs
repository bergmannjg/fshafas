namespace FsHafas.Extensions

/// temporary extensions to add code for target python

#if FABLE_PY

module internal ArrayEx =

    open Fable.Core

    // workaround: error in Arrayy.sortBy
    [<Emit("sorted($1, key=$0)")>]
    let sortBy (key: 'a -> 'b) (arr: array<'a>) : array<'a> = jsNative

module internal DateTimeEx =

    open Fable.Core

    // workaround: missing code year
    [<Emit("$0.year")>]
    let year (dt: obj) : int = jsNative

    // workaround: missing code month
    [<Emit("$0.month")>]
    let month (dt: obj) : int = jsNative

    // workaround: missing code day
    [<Emit("$0.day")>]
    let day (dt: obj) : int = jsNative

    // workaround: missing code hour
    [<Emit("$0.hour")>]
    let hour (dt: obj) : int = jsNative

    // workaround: missing code minute
    [<Emit("$0.minute")>]
    let minute (dt: obj) : int = jsNative

    // workaround: missing code second
    [<Emit("$0.second")>]
    let second (dt: obj) : int = jsNative

    // workaround: error in DateTime.ToString
    let formatDate (dt: System.DateTime) (pattern: string) : string =
        if "yyyyMMdd" = pattern then
            sprintf "%04d%02d%02d" (year dt) (month dt) (day dt)
        else
            raise (System.NotImplementedException("nyi"))

    // workaround: error in DateTime.ToString if minute = 0
    let formatTime (dt: System.DateTime) (pattern: string) : string =
        if "HHmm" = pattern then
            sprintf "%02d%02d" (hour dt) (minute dt)
        else
            raise (System.NotImplementedException("nyi"))

    // workaround: missing code addHours
    [<Import("timedelta", from = "datetime")>]
    [<Emit("$1+timedelta(hours=$2)")>]
    let addHours (dt: System.DateTime, h: int) : System.DateTime = jsNative

    // workaround: missing code addDays
    [<Import("timedelta", from = "datetime")>]
    [<Emit("$1+timedelta(days=$2)")>]
    let addDays (dt: System.DateTime, h: int) : System.DateTime = jsNative

module internal DateTimeOffsetEx =

    open Fable.Core

    [<ImportAll("datetime")>]

    [<Import("tz", from = "dateutil")>]
    [<Emit("tz_1.gettz($1)")>]
    let gettz (tz: string) : obj = jsNative

    [<Import("tz", from = "dateutil")>]
    [<Emit("tz_1.tzoffset(None, $1)")>]
    let mktzoffset (seconds: int) : obj = jsNative

    [<Emit("datetime.datetime($0, $1, $2, $3, $4, $5, tzinfo=$6)")>]
    let mkDateTime
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
            | Some (tzoffsetArg), _ ->
                mkDateTime
                    (DateTimeEx.year dt)
                    (DateTimeEx.month dt)
                    (DateTimeEx.day dt)
                    (DateTimeEx.hour dt)
                    (DateTimeEx.minute dt)
                    (DateTimeEx.second dt)
                    (mktzoffset tzoffsetArg)
            | _, Some (tz) ->
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
