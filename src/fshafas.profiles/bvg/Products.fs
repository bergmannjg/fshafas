namespace FsHafas.Profiles.BvgConfig

module internal Products =

    open FsHafas.Client

    let products: ProductType [] =
        [| { id = "suburban"
             mode = Train
             name = "S-Bahn"
             short = "S"
             bitmasks = [| 1 |]
             ``default`` = true }
           { id = "subway"
             mode = Train
             name = "U-Bahn"
             short = "U"
             bitmasks = [| 2 |]
             ``default`` = true }
           { id = "tram"
             mode = Train
             name = "Tram"
             short = "T"
             bitmasks = [| 4 |]
             ``default`` = true }
           { id = "bus"
             mode = Bus
             name = "Bus"
             short = "B"
             bitmasks = [| 8 |]
             ``default`` = true }
           { id = "ferry"
             mode = Watercraft
             name = "Fähre"
             short = "F"
             bitmasks = [| 16 |]
             ``default`` = true }
           { id = "express"
             mode = Train
             name = "IC/ICE"
             short = "E"
             bitmasks = [| 32 |]
             ``default`` = true }
           { id = "regional"
             mode = Train
             name = "RB/RE"
             short = "R"
             bitmasks = [| 64 |]
             ``default`` = true } |]
