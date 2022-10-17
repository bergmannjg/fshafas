namespace FsHafas.Profiles.RejseplanenConfig

module internal Products =

    open FsHafas.Client

    let products: ProductType [] =
        [| { id = "national-train"
             mode = Train
             name = "InterCity"
             short = "IC"
             bitmasks = [| 1 |]
             ``default`` = true }
           { id = "national-train-2"
             mode = Train
             name = "ICL"
             short = "ICL"
             bitmasks = [| 2 |]
             ``default`` = true }
           { id = "local-train"
             mode = Train
             name = "Regional"
             short = "RE"
             bitmasks = [| 4 |]
             ``default`` = true }
           { id = "o"
             mode = Train
             name = "Ø"
             short = "Ø"
             bitmasks = [| 8 |]
             ``default`` = true }
           { id = "s-tog"
             mode = Train
             name = "S-Tog A/B/Bx/C/E/F/H"
             short = "S"
             bitmasks = [| 16 |]
             ``default`` = true } |]
