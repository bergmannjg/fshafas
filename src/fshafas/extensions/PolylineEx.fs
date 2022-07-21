namespace FsHafas.Extensions

module internal PolylineEx =

#if FABLE_COMPILER
    open Fable.Core

    type GooglePolyline = { decode: string -> float [] [] }
#endif

#if FABLE_JS
    [<ImportDefault("google-polyline")>]
    let defaultObject: GooglePolyline = jsNative

    let polylineDecode (xy: string) = defaultObject.decode xy

#else
#if FABLE_PY

    [<ImportAll("polyline")>]
    [<Emit("polyline.decode($1)")>]
    let polylineDecode (_: string) : float [] [] = jsNative

#else
    open PolylinerNet

    let polyliner = Polyliner()

    let polylineDecode (xy: string) =
        polyliner.Decode xy
        |> Seq.map (fun p -> [| p.Latitude; p.Longitude |])
#endif
#endif
