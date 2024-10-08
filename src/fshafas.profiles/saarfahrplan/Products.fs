namespace FsHafas.Profiles.SaarFahrplanConfig

module internal Products =

    open FsHafas.Client

    let products: ProductType[] =
        [| { id = "nationalExpress"
             mode = Train
             name = "Hochgeschwindigkeitszug"
             short = "ICE"
             bitmasks = [| 8192 |]
             ``default`` = true }
           { id = "national"
             mode = Train
             name = "InterCity & EuroCity"
             short = "IC/EC"
             bitmasks = [| 4096 |]
             ``default`` = true }
           { id = "interregional"
             mode = Train
             name = "InterRegio"
             short = "IR"
             bitmasks = [| 2048 |]
             ``default`` = true }
           { id = "regional"
             mode = Train
             name = "Regionalzug"
             short = "RB ?"
             bitmasks = [| 1024 |]
             ``default`` = true }
           { id = "suburban"
             mode = Train
             name = "S-Bahn"
             short = "S-Bahn"
             bitmasks = [| 512 |]
             ``default`` = true }
           { id = "subway"
             mode = Train
             name = "U-Bahn"
             short = "U"
             bitmasks = [| 256 |]
             ``default`` = true }
           { id = "saarbahn"
             mode = Train
             name = "Saarbahn"
             short = "S"
             bitmasks = [| 128 |]
             ``default`` = true }
           { id = "bus"
             mode = Bus
             name = "Bus"
             short = "Bus"
             bitmasks = [| 64 |]
             ``default`` = true }
           { id = "watercraft"
             mode = Watercraft
             name = "Schiff"
             short = "Schiff"
             bitmasks = [| 32 |]
             ``default`` = true }
           { id = "onCall"
             mode = Bus
             name = "Anruf-Sammel-Taxi"
             short = "AST"
             bitmasks = [| 16 |]
             ``default`` = true }
           { id = "school-bus"
             mode = Bus
             name = "Schulbus"
             short = "Schulbus"
             bitmasks = [| 8 |]
             ``default`` = true } |]
