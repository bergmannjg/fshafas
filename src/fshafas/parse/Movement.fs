namespace FsHafas.Parser

module internal Movement =

#if FABLE_COMPILER
    open Fable.Core
#endif

    open FsHafas.Client

    let parseMovement (ctx: Context) (m: FsHafas.Raw.RawJny) =

        let line =
            Common.getElementAt m.prodX ctx.common.lines

        let location =
            match m.pos with
            | Some pos ->
                { Default.Location with
                      longitude = Coordinate.toFloat (pos.x) |> Some
                      latitude = Coordinate.toFloat (pos.y) |> Some }
                |> Some
            | None -> None

        let stopovers =
            ctx.profile.parseStopovers ctx m.stopL m.date.Value

        let frames =
            match m.ani with
            | Some (ani) ->
                ani.mSec
                |> Array.mapi<int, FsHafas.Client.Frame option>
                    (fun i ms ->

                        let origin =
                            Common.getElementAt ani.fLocX.[i] ctx.common.locations
                            |> U2StopLocation.FromSomeU3StationStopLocation

                        let destination =
                            Common.getElementAt ani.tLocX.[i] ctx.common.locations
                            |> U2StopLocation.FromSomeU3StationStopLocation

                        match origin, destination with
                        | Some origin, Some destination ->
                            let f : FsHafas.Client.Frame =
                                { origin = origin
                                  destination = destination
                                  t = Some ms }

                            f |> Some
                        | _ -> None)
                |> Array.choose id
                |> Some
            | None -> None

        let polyline =
            match m.ani with
            | Some (ani) ->
                match ani.poly with
                | Some (value) -> Some(ctx.profile.parsePolyline ctx value)
                | None -> None
            | None -> None

        { Default.Movement with
              direction = m.dirTxt
              tripId = Some m.jid
              line = line
              location = location
              nextStopovers = stopovers
              frames = frames
              polyline = polyline }
