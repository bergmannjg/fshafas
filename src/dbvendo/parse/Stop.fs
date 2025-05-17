namespace DbVendo.Parser

module internal Stop =

    open FsHafas.Client
    open DbVendo

    let private findProducts (response: Raw.StopResponse) : Products option =
        let products =
            DbVendo.Client.Products.products
            |> Array.fold
                (fun (m: Products) p ->
                    m.[p.id] <-
                        response.produktGattungen
                        |> Array.exists (fun pg -> pg.produktGattung = p.dbnav)

                    m)
                (Products(false))

        Some products

    let parseStopResponse (stop: U2<string, Stop>) (response: Raw.StopResponse) : StationStopLocation =
        let id =
            match stop with
            | U2.Case1 id -> // simple check, if id is evaNr
                match id.Length = 7, System.Int32.TryParse id with
                | true, (true, _) -> Some id
                | _, _ -> None
            | U2.Case2 stop -> stop.id

        StationStopLocation.Stop
            { Default.Stop with
                name = Some response.haltName
                id = id
                products = findProducts response }
