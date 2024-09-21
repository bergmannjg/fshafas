namespace FsHafas.Profiles.SvvConfig

module internal Products =

    open FsHafas.Client

    let products: ProductType[] =
        [| { id = "bahn-s-bahn"
             mode = Train
             name = "Bahn & S-Bahn"
             short = "S/Zug"
             bitmasks = [| 1; 2 |]
             ``default`` = true }
           { id = "u-bahn"
             mode = Train
             name = "U-Bahn"
             short = "U"
             bitmasks = [| 4 |]
             ``default`` = true }
           { id = "strassenbahn"
             mode = Train
             name = "Strassenbahn"
             short = "Str"
             bitmasks = [| 16 |]
             ``default`` = true }
           { id = "fernbus"
             mode = Bus
             name = "Fernbus"
             short = "Bus"
             bitmasks = [| 32 |]
             ``default`` = true }
           { id = "regionalbus"
             mode = Bus
             name = "Regionalbus"
             short = "Bus"
             bitmasks = [| 64 |]
             ``default`` = true }
           { id = "stadtbus"
             mode = Bus
             name = "Stadtbus"
             short = "Bus"
             bitmasks = [| 128 |]
             ``default`` = true }
           { id = "seilbahn-zahnradbahn"
             mode = Gondola
             name = "Seil-/Zahnradbahn"
             short = "Seil-/Zahnradbahn"
             bitmasks = [| 256 |]
             ``default`` = true }
           { id = "schiff"
             mode = Watercraft
             name = "Schiff"
             short = "F"
             bitmasks = [| 512 |]
             ``default`` = true } |]
