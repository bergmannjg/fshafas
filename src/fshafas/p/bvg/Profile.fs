namespace FsHafas.Profiles

module Bvg =

    open FsHafas
    open FsHafas.Client
    open System.Text.RegularExpressions

#if FABLE_COMPILER
    open Fable.Core
#endif

    let private products : Client.ProductType [] =
        [| { id = "suburban"
             mode = Client.ProductTypeMode.Train
             bitmasks = [| 1 |]
             name = "S-Bahn"
             short = "S"
             ``default`` = true }
           { id = "subway"
             mode = Client.ProductTypeMode.Train
             bitmasks = [| 2 |]
             name = "U-Bahn"
             short = "U"
             ``default`` = true }
           { id = "tram"
             mode = Client.ProductTypeMode.Train
             bitmasks = [| 4 |]
             name = "Tram"
             short = "T"
             ``default`` = true }
           { id = "bus"
             mode = Client.ProductTypeMode.Bus
             bitmasks = [| 8 |]
             name = "Bus"
             short = "B"
             ``default`` = true }
           { id = "ferry"
             mode = Client.ProductTypeMode.Watercraft
             bitmasks = [| 16 |]
             name = "Fähre"
             short = "F"
             ``default`` = true }
           { id = "express"
             mode = Client.ProductTypeMode.Train
             bitmasks = [| 32 |]
             name = "IC/ICE"
             short = "E"
             ``default`` = true }
           { id = "regional"
             mode = Client.ProductTypeMode.Train
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

    let private req : Raw.RawRequest =
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

    let getProfile () =
        let profile = Api.Parser.defaultProfile

        { profile with
              locale = "de-DE"
              timezone = "Europe/Berlin"
              endpoint = "https://bvg-apps.hafas.de/bin/mgate.exe"
              salt = ""
              cfg = Some { polyEnc = "GPA"; rtMode = None }
              baseRequest = Some req
              products = products
              trip = Some true
              radar = Some true
              lines = Some true
              remarks = Some true
              reachableFrom = Some true }
