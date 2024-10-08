namespace FsHafas.Profiles.DbConfig

module internal Products =

    open FsHafas.Client

    let products: ProductType[] =
        [| { id = "nationalExpress"
             mode = Train
             name = "InterCityExpress"
             short = "ICE"
             bitmasks = [| 1 |]
             ``default`` = true }
           { id = "national"
             mode = Train
             name = "InterCity & EuroCity"
             short = "IC/EC"
             bitmasks = [| 2 |]
             ``default`` = true }
           { id = "regionalExpress"
             mode = Train
             name = "RegionalExpress & InterRegio"
             short = "RE/IR"
             bitmasks = [| 4 |]
             ``default`` = true }
           { id = "regional"
             mode = Train
             name = "Regio"
             short = "RB"
             bitmasks = [| 8 |]
             ``default`` = true }
           { id = "suburban"
             mode = Train
             name = "S-Bahn"
             short = "S"
             bitmasks = [| 16 |]
             ``default`` = true }
           { id = "bus"
             mode = Bus
             name = "Bus"
             short = "B"
             bitmasks = [| 32 |]
             ``default`` = true }
           { id = "ferry"
             mode = Watercraft
             name = "Ferry"
             short = "F"
             bitmasks = [| 64 |]
             ``default`` = true }
           { id = "subway"
             mode = Train
             name = "U-Bahn"
             short = "U"
             bitmasks = [| 128 |]
             ``default`` = true }
           { id = "tram"
             mode = Train
             name = "Tram"
             short = "T"
             bitmasks = [| 256 |]
             ``default`` = true }
           { id = "taxi"
             mode = Taxi
             name = "Group Taxi"
             short = "Taxi"
             bitmasks = [| 512 |]
             ``default`` = true } |]
