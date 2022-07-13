namespace FsHafas.Parser

module internal Polyline =

    open System
    open FsHafas.Client
    open FsHafas.Endpoint

    let decode (s: string) : float [] [] = [||]

#if FABLE_COMPILER
    open Fable.Core

    type GooglePolyline = { decode: string -> float [] [] }

#if FABLE_JS
    [<ImportDefault("google-polyline")>]
    let defaultObject: GooglePolyline = jsNative
#else

#if FABLE_PY
    [<ImportAll("polyline")>]
    [<Emit("polyline.decode($1)")>]
    let polylinedecode (_: string) : float [] []  = jsNative

    let defaultObject : GooglePolyline = { decode = polylinedecode }
#endif

#endif

#else
    open PolylinerNet

    let polyliner = Polyliner()
#endif

    let round (f: float) = System.Math.Round(f, 5)

    let defaultFeatureCollection: FsHafas.Client.FeatureCollection =
        { ``type`` = Some "FeatureCollection"
          features = Array.empty }

    let private polylineDecode (xy: string) =
#if FABLE_COMPILER
        defaultObject.decode xy
#else
        polyliner.Decode xy
        |> Seq.map (fun p -> [| p.Latitude; p.Longitude |])
#endif

    let private getStop (ctx: Context) (p: FsHafas.Raw.RawPoly) (i: int) =
        match p.ppLocRefL with
        | Some ppLocRefL ->
            match ppLocRefL
                  |> Array.tryFind (fun pLocRefL -> pLocRefL.ppIdx = i)
                with
            | Some pLocRefL ->
                match Common.getElementAt pLocRefL.locX ctx.common.locations with
                | Some (U3.Case2 s) -> s :> obj
                | _ -> obj ()
            | None -> obj ()
        | None -> obj ()

    let parsePolyline (ctx: Context) (poly: FsHafas.Raw.RawPoly) : FsHafas.Client.FeatureCollection =
        let features =
            polylineDecode poly.crdEncYX
            |> Seq.mapi<_, FsHafas.Client.Feature> (fun i p ->
                { ``type`` = Some "Feature"
                  properties = getStop ctx poly i
                  geometry =
                    { ``type`` = Some "Point"
                      coordinates = [| (round p.[1]); (round p.[0]) |] } })
            |> Seq.toArray

        { defaultFeatureCollection with features = features }

    let ``calculate distance`` (p1Latitude, p1Longitude) (p2Latitude, p2Longitude) =
        let r = 6371.0 // km

        let dLat = (p2Latitude - p1Latitude) * Math.PI / 180.0

        let dLon = (p2Longitude - p1Longitude) * Math.PI / 180.0

        let lat1 = p1Latitude * Math.PI / 180.0
        let lat2 = p2Latitude * Math.PI / 180.0

        let a =
            Math.Sin(dLat / 2.0) * Math.Sin(dLat / 2.0)
            + Math.Sin(dLon / 2.0)
              * Math.Sin(dLon / 2.0)
              * Math.Cos(lat1)
              * Math.Cos(lat2)

        let c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a))

        r * c

    let distanceOfFeatureCollection (fc: FsHafas.Client.FeatureCollection) =
        let latLonPoints =
            fc.features
            |> Array.map (fun f -> (f.geometry.coordinates.[1], f.geometry.coordinates.[0]))

        latLonPoints
        |> Array.mapi (fun i _ ->
            if i > 0 then
                let prev = latLonPoints.[i - 1]
                let curr = latLonPoints.[i]
                ``calculate distance`` prev curr
            else
                0.0)
        |> Array.sum
