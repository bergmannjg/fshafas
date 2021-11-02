namespace FsHafas.Profiles

module Bvg =

    open FsHafas.Client
    open System.Text.RegularExpressions

#if FABLE_COMPILER
    open Fable.Core
#endif

    let private products: ProductType [] =
        [| { id = "suburban"
             mode = ProductTypeMode.Train
             bitmasks = [| 1 |]
             name = "S-Bahn"
             short = "S"
             ``default`` = true }
           { id = "subway"
             mode = ProductTypeMode.Train
             bitmasks = [| 2 |]
             name = "U-Bahn"
             short = "U"
             ``default`` = true }
           { id = "tram"
             mode = ProductTypeMode.Train
             bitmasks = [| 4 |]
             name = "Tram"
             short = "T"
             ``default`` = true }
           { id = "bus"
             mode = ProductTypeMode.Bus
             bitmasks = [| 8 |]
             name = "Bus"
             short = "B"
             ``default`` = true }
           { id = "ferry"
             mode = ProductTypeMode.Watercraft
             bitmasks = [| 16 |]
             name = "Fähre"
             short = "F"
             ``default`` = true }
           { id = "express"
             mode = ProductTypeMode.Train
             bitmasks = [| 32 |]
             name = "IC/ICE"
             short = "E"
             ``default`` = true }
           { id = "regional"
             mode = ProductTypeMode.Train
             bitmasks = [| 64 |]
             name = "RB/RE"
             short = "R"
             ``default`` = true } |]

    let private IBNR = Regex(@"^\d+$")

    let formatStation (id: string) =
        if IBNR.IsMatch id
           && (id.Length = 7 || id.Length = 9 || id.Length = 12) then
            id
        else
            raise (System.ArgumentException("station id: " + id))

    let private req: FsHafas.Raw.RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "BVG"
              v = "6020000"
              ``type`` = "IPA"
              name = "FahrInfo" }
          ext = "BVG.1"
          ver = "1.21"
          auth =
            { ``type`` = "AID"
              aid = "Mz0YdF9Fgx0Mb9" } }

    let profile = FsHafas.Api.Profile.defaultProfile ()

    profile._locale <- "de-DE"
    profile._timezone <- "Europe/Berlin"
    profile._endpoint <- "https://bvg-apps.hafas.de/bin/mgate.exe"
    profile.salt <- ""
    profile.cfg <- Some { polyEnc = "GPA"; rtMode = None }
    profile.baseRequest <- Some req
    profile._products <- products
    profile._trip <- Some true
    profile._radar <- Some true
    profile._lines <- Some true
    profile._remarks <- Some true
    profile._reachableFrom <- Some true
