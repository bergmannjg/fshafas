namespace FsHafas.Parser

module internal Location =

    open System.Text.RegularExpressions

    open FsHafas.Client
    open FsHafas.Endpoint

    type Lid =
        { L: string option
          O: string option
          X: int option
          Y: int option }

    let private parseLid (lid: string option) =
        match lid with
        | Some lid ->
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
        | None ->
            { L = None
              O = None
              X = None
              Y = None }

    let private removeLeadingZeros (s: string) = Regex.Replace(s, "^0+", "")

    let private parseLocationPhase1 (ctx: Context) (i: int) (locl: FsHafas.Raw.RawLoc []) =
        let l = locl.[i]
        let lid = parseLid l.lid

        let id =
            match l.extId, lid.L with
            | Some extid, _ -> Some extid
            | _, Some l -> Some l
            | _ -> None
            |> Option.map removeLeadingZeros

        let (lon, lat) =
            match l.crd with
            | Some (crd) when crd.x > 0 && crd.y > 0 ->
                (Some(Coordinate.toFloat (crd.x)), Some(Coordinate.toFloat (crd.y)))
            | _ ->
                match lid.X, lid.Y with
                | Some X, Some Y -> (Some(Coordinate.toFloat X), Some(Coordinate.toFloat Y))
                | _ -> (None, None)

        let distance =
            l.dist
            |> Option.bind (fun d -> if d > 0 then Some d else None)

        match l.``type`` with
        | Some ``type`` ->
            if ``type`` = "S" then
                let name = Some l.name

                let location =
                    lon
                    |> Option.map (fun _ ->
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

                if l.isMainMast.IsSome && l.isMainMast.Value then
                    (l,
                     U3.Case1(
                         { Default.Station with
                             id = id
                             name = name
                             location = location
                             lines = lines
                             products = products
                             distance = distance }
                     ))
                else
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
                    if ``type`` = "A" then
                        Some l.name
                    else
                        None

                let name =
                    if ``type`` <> "A" then
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
        | None ->
            (l,
             U3.Case3(
                 { Default.Location with
                     id = id
                     name = Some l.name
                     longitude = lon
                     latitude = lat
                     distance = distance }
             ))

    let private parseLocationPhase2
        (i: int)
        (l: FsHafas.Raw.RawLoc)
        (locations: (FsHafas.Raw.RawLoc * U3<FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location>) [])
        (commonLocations: U3<FsHafas.Client.Station, FsHafas.Client.Stop, FsHafas.Client.Location> [])
        =
        let station =
            match l.mMastLocX with
            | Some mMastLocX when mMastLocX < locations.Length ->
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
            | Some mMastLocX when mMastLocX < commonLocations.Length ->
                match commonLocations.[mMastLocX] with
                | U3.Case2 stop ->
                    { Default.Station with
                        id = stop.id
                        name = stop.name
                        location = stop.location
                        products = stop.products
                        lines = stop.lines }
                    |> Some
                | _ -> None
            | _ -> None

        match locations.[i] with
        | (_, U3.Case2 stop) -> U3.Case2({ stop with station = station })
        | (_, location) -> location

    /// parse in 2 phases to avoid recursion
    let parseLocations (ctx: Context) (locL: FsHafas.Raw.RawLoc []) =
        let locations =
            locL
            |> Array.mapi (fun i _ -> parseLocationPhase1 ctx i locL)

        locations
        |> Array.mapi (fun i (l, _) -> parseLocationPhase2 i l locations ctx.common.locations)
        |> Array.filter (fun u3 ->
            match u3 with
            | U3.Case3 l when l.latitude = None || l.longitude = None -> false
            | _ -> true)
