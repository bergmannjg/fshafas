namespace FsHafas.Parser

module internal Polyline =

    open System
    open FsHafas.Client
    open FsHafas.Endpoint

    let decode (s: string) : float [] [] = [||]

    let round (f: float) = System.Math.Round(f, 5)

    let defaultFeatureCollection: FsHafas.Client.FeatureCollection =
        { ``type`` = FeatureCollectionType.FeatureCollection
          features = Array.empty }

    let private getStop (ctx: Context) (p: FsHafas.Raw.RawPoly) (i: int) =
        match p.ppLocRefL with
        | Some ppLocRefL ->
            match ppLocRefL
                  |> Array.tryFind (fun pLocRefL -> pLocRefL.ppIdx = i)
                with
            | Some pLocRefL -> Common.getElementAt pLocRefL.locX ctx.common.locations
            | None -> None
        | None -> None

    let parsePolyline (ctx: Context) (poly: FsHafas.Raw.RawPoly) : FsHafas.Client.FeatureCollection =
        let features =
            FsHafas.Extensions.PolylineEx.polylineDecode poly.crdEncYX
            |> Seq.mapi<_, FsHafas.Client.Feature> (fun i p ->
                { ``type`` = FeatureType.Feature
                  properties = getStop ctx poly i
                  geometry =
                    { ``type`` = GeometryType.Point
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
