namespace FsHafas.Parser

module internal Location =

#if FABLE_COMPILER
    open Fable.Core
#endif

    open FsHafas
    open System.Text.RegularExpressions

    type Lid =
        { L: string option
          O: string option
          X: int option
          Y: int option }

    let private parseLid (lid: string) =
        let tuples =
            lid.Split([| '@' |])
            |> Array.filter (fun s -> s.Length > 3)
            |> Array.map (fun s -> (s.[0], s.Substring(2)))

        let tryFind (c) =
            tuples
            |> Array.tryFind (fun (n, v) -> n = c)
            |> Option.map (fun (n, v) -> v)

        let tryParseInt (s: string) =
            match System.Int32.TryParse s with
            | true, i -> Some i
            | _ -> None

        { L = tryFind 'L'
          O = tryFind 'O'
          X = tryFind 'X' |> Option.bind tryParseInt
          Y = tryFind 'Y' |> Option.bind tryParseInt }

    let private leadingZeros = Regex(@"^0+")

    let private parseLocationPhase1 (ctx: Context) (i: int) (locl: Raw.RawLoc []) =
        let l = locl.[i]
        let lid = parseLid l.lid

        let id =
            match l.extId, lid.L with
            | Some extid, _ -> Some extid
            | _, Some l -> Some l
            | _ -> None
            |> Option.map (fun s -> leadingZeros.Replace(s, ""))

        let (lon, lat) =
            match l.crd with
            | Some (crd) -> (Some(Coordinate.toFloat (crd.x)), Some(Coordinate.toFloat (crd.y)))
            | None ->
                match lid.X, lid.Y with
                | Some X, Some Y -> (Some(Coordinate.toFloat X), Some(Coordinate.toFloat Y))
                | _ -> (None, None)

        let distance =
            l.dist
            |> Option.bind (fun d -> if d > 0 then Some d else None)

        if l.``type`` = "S" then
            let name = Some l.name

            let location =
                lon
                |> Option.map
                    (fun _ ->
                        { Default.Location with
                              id = id
                              longitude = lon
                              latitude = lat })

            let products =
                l.pCls
                |> Option.map (fun pCls -> ctx.profile.parseBitmask ctx pCls)

            let lines =
                if ctx.opt.linesOfStops then
                    l.pRefL
                    |> Common.mapIndexArray ctx.res.common (fun _ -> Some ctx.common.lines)
                    |> Common.toOption
                else
                    None

            (l,
             U3.Case2(
                 { Default.Stop with
                       id = id
                       name = name
                       location = location
                       lines = lines
                       products = products
                       distance = distance }
             ))
        else
            let address =
                if l.``type`` = "A" then
                    Some l.name
                else
                    None

            let name =
                if l.``type`` <> "A" then
                    Some l.name
                else
                    None

            (l,
             U3.Case3(
                 { Default.Location with
                       id = id
                       name = name
                       address = address
                       longitude = lon
                       latitude = lat
                       distance = distance }
             ))

    let private parseLocationPhase2
        (i: int)
        (l: Raw.RawLoc)
        (locations: (Raw.RawLoc * U3<Client.Station, Client.Stop, Client.Location>) [])
        =
        let station =
            match l.mMastLocX with
            | Some mMastLocX ->
                match locations.[mMastLocX] with
                | (_, U3.Case2 stop) ->
                    { Default.Station with
                          id = stop.id
                          name = stop.name
                          location = stop.location
                          products = stop.products
                          lines = stop.lines }
                    |> Some
                | _ -> None
            | None -> None

        match locations.[i] with
        | (_, U3.Case2 stop) -> U3.Case2({ stop with station = station })
        | (_, location) -> location

    /// parse in 2 phases to avoid recursion
    let parseLocations (ctx: Context) (locL: Raw.RawLoc []) =
        let locations =
            locL
            |> Array.mapi (fun i _ -> parseLocationPhase1 ctx i locL)

        locations
        |> Array.mapi (fun i (l, _) -> parseLocationPhase2 i l locations)
