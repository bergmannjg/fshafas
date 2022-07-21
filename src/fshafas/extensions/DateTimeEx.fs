namespace FsHafas.Extensions

module internal DateTimeEx =

#if FABLE_PY
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
#endif

#if FABLE_PY
    // workaround: missing code addDays
    [<Import("timedelta", from = "datetime")>]
    [<Emit("$1+timedelta(days=$2)")>]
    let addDays (dt: System.DateTime, h: int) : System.DateTime = jsNative
#else
    let addDays (dt: System.DateTime, h: int) : System.DateTime = dt.AddDays(h)
#endif
