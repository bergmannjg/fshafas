namespace DbVendo.Client

module internal Products =

    open FsHafas.Client

    type ProductTypeEx =
        { id: string
          mode: ProductTypeMode
          name: string
          short: string
          dbnav: string
          dbnav_short: string
          bitmasks: array<int>
          ``default``: bool }

    let products: ProductTypeEx[] =
        [| { id = "nationalExpress"
             mode = Train
             name = "InterCityExpress"
             short = "ICE"
             dbnav = "HOCHGESCHWINDIGKEITSZUEGE"
             dbnav_short = "ICE"
             bitmasks = [| 1 |]
             ``default`` = true }
           { id = "national"
             mode = Train
             name = "InterCity & EuroCity"
             short = "IC/EC"
             dbnav = "INTERCITYUNDEUROCITYZUEGE"
             dbnav_short = "IC_EC"
             bitmasks = [| 2 |]
             ``default`` = true }
           { id = "regionalExpress"
             mode = Train
             name = "RegionalExpress & InterRegio"
             short = "RE/IR"
             dbnav = "INTERREGIOUNDSCHNELLZUEGE"
             dbnav_short = "IR"
             bitmasks = [| 4 |]
             ``default`` = true }
           { id = "regional"
             mode = Train
             name = "Regio"
             short = "RB"
             dbnav = "NAHVERKEHRSONSTIGEZUEGE"
             dbnav_short = "RB"
             bitmasks = [| 8 |]
             ``default`` = true }
           { id = "suburban"
             mode = Train
             name = "S-Bahn"
             short = "S"
             dbnav = "SBAHNEN"
             dbnav_short = "SBAHN"
             bitmasks = [| 16 |]
             ``default`` = true }
           { id = "bus"
             mode = Bus
             name = "Bus"
             short = "B"
             dbnav = "BUSSE"
             dbnav_short = "BUS"
             bitmasks = [| 32 |]
             ``default`` = true }
           { id = "ferry"
             mode = Watercraft
             name = "Ferry"
             short = "F"
             dbnav = "SCHIFF"
             dbnav_short = "SCHIFF"
             bitmasks = [| 64 |]
             ``default`` = true }
           { id = "subway"
             mode = Train
             name = "U-Bahn"
             short = "U"
             dbnav = "UBAHN"
             dbnav_short = "UBAHN"
             bitmasks = [| 128 |]
             ``default`` = true }
           { id = "tram"
             mode = Train
             name = "Tram"
             short = "T"
             dbnav = "STRASSENBAHN"
             dbnav_short = "STR"
             bitmasks = [| 256 |]
             ``default`` = true }
           { id = "taxi"
             mode = Taxi
             name = "Group Taxi"
             short = "Taxi"
             dbnav = "ANRUFPFLICHTIGEVERKEHRE"
             dbnav_short = "ANRUFPFLICHTIGEVERKEHRE"
             bitmasks = [| 512 |]
             ``default`` = true } |]

    let parseProducts (productnames: string array) : FsHafas.Client.Products =
        products
        |> Array.fold
            (fun m p ->
                m.[p.id] <- productnames |> Array.exists (fun n -> n = p.dbnav)
                m)
            (Products(false))

    let fllterProducts (filter: FsHafas.Client.Products) : string array =
        let names =
            products
            |> Array.filter (fun p -> filter.Keys |> Array.exists (fun k -> k = p.id && filter[p.id]))
            |> Array.map (fun p -> p.dbnav)

        if names.Length > 0 then names else [| "ALL" |]
