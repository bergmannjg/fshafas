namespace FsHafas.Parser

module internal Trip =

    open System
    open FsHafas

    let parseTrip (ctx: Context) (j: Raw.RawJny): Client.Trip =

        match j.stopL with
        | Some stopL when stopL.Length > 1 ->
            let rawSecL: Raw.RawSec =
                { ``type`` = "JNY"
                  icoX = None
                  dep = RawDep.FromRawStopL stopL.[0]
                  arr = RawArr.FromRawStopL stopL.[stopL.Length - 1]
                  jny = Some j
                  resState = None
                  resRecommendation = None }

            let date =
                match j.date with
                | Some date -> date
                | None ->
                    let dt = DateTime.Now
                    sprintf "%04d%02d%02d" dt.Year dt.Month dt.Day

            let leg =
                ctx.profile.parseJourneyLeg ctx rawSecL date

            match leg.tripId with
            | Some tripId -> Trip.FromLeg tripId leg
            | _ -> raise (System.ArgumentException("parseTrip failed"))
        | _ -> raise (System.ArgumentException("parseTrip failed"))
