namespace FsHafas.Profiles

module Svv =

    open FsHafas.Client

    let private products: ProductType [] =
        [| { id = "bahn-s-bahn"
             mode = ProductTypeMode.Train
             bitmasks = [| 1; 2 |]
             name = "Bahn & S-Bahn"
             short = "S/Zug"
             ``default`` = true }
           { id = "u-bahn"
             mode = ProductTypeMode.Train
             bitmasks = [| 4 |]
             name = "U-Bahn"
             short = "U"
             ``default`` = true }
           { id = "strassenbahn"
             mode = ProductTypeMode.Train
             bitmasks = [| 16 |]
             name = "Strassenbahn"
             short = "Str"
             ``default`` = true }
           { id = "fernbus"
             mode = ProductTypeMode.Bus
             bitmasks = [| 32 |]
             name = "Fernbus"
             short = "Bus"
             ``default`` = true }
           { id = "regionalbus"
             mode = ProductTypeMode.Bus
             bitmasks = [| 64 |]
             name = "Regionalbus"
             short = "Bus"
             ``default`` = true }
           { id = "stadtbus"
             mode = ProductTypeMode.Bus
             bitmasks = [| 128 |]
             name = "Stadtbus"
             short = "Bus"
             ``default`` = true }
           { id = "seilbahn-zahnradbahn"
             mode = ProductTypeMode.Gondola
             bitmasks = [| 256 |]
             name = "Seil-/Zahnradbahn"
             short = "Seil-/Zahnradbahn"
             ``default`` = true }
           { id = "schiff"
             mode = ProductTypeMode.Watercraft
             bitmasks = [| 512 |]
             name = "Schiff"
             short = "F"
             ``default`` = true } |]

    let private req: FsHafas.Raw.RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "VAO"
              v = ""
              ``type`` = "WEB"
              name = "webapp" }
          ext = "VAO.11"
          ver = "1.20"
          auth =
            { ``type`` = "AID"
              aid = "wf7mcf9bv3nv8g5f" } }

    let profile = FsHafas.Api.Profile.defaultProfile ()

    profile._locale <- "at-DE"
    profile._timezone <- "Europe/Vienna"
    profile._endpoint <- "https://fahrplan.salzburg-verkehr.at/bin/mgate.exe"
    profile.salt <- ""
    profile.cfg <- Some { polyEnc = "GPA"; rtMode = None }
    profile.baseRequest <- Some req
    profile._products <- products
    profile._trip <- Some true
    profile._lines <- Some true
    profile._remarks <- Some true
    profile._reachableFrom <- Some true
    profile.departuresGetPasslist <- false
    profile.departuresStbFltrEquiv <- false
