namespace FsHafas.Parser

module internal Trip =

    open System
    open FsHafas.Client
    open FsHafas.Endpoint

#if FABLE_PY
    open FsHafas.Extensions
#endif

    let parseTrip (ctx: Context) (j: FsHafas.Raw.RawJny) : FsHafas.Client.Trip =

        match j.stopL with
        | Some stopL when stopL.Length > 1 ->
            let rawSecL: FsHafas.Raw.RawSec =
                { ``type`` = "JNY"
                  icoX = None
                  dep = RawDep.FromRawStopL stopL.[0]
                  arr = RawArr.FromRawStopL stopL.[stopL.Length - 1]
                  jny = Some j
                  resState = None
                  resRecommendation = None
                  gis = None }

            let date =
                match j.date with
                | Some date -> date
                | None ->
                    let dt = DateTime.Now
#if FABLE_PY
                    sprintf "%04d%02d%02d" (DateTimeEx.year dt) (DateTimeEx.month dt) (DateTimeEx.day dt)
#else
                    sprintf "%04d%02d%02d" dt.Year dt.Month dt.Day
#endif
            let leg = ctx.profile.parseJourneyLeg ctx rawSecL date

            let scheduledDays =
                match j.sDaysL with
                | Some sDaysL when sDaysL.Length > 0 -> ctx.profile.parseScheduledDays ctx sDaysL.[0] // todo: parse array
                | _ -> None

            match leg.tripId with
            | Some tripId -> { ToTrip.FromLeg tripId leg with scheduledDays = scheduledDays }
            | _ -> raise (System.ArgumentException("parseTrip failed"))
        | _ -> raise (System.ArgumentException("parseTrip failed"))
