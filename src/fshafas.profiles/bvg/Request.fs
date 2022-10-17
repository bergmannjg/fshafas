namespace FsHafas.Profiles.BvgConfig

module internal Request =

    open FsHafas.Raw

    let request: RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "BVG"
              v = "6020000"
              ``type`` = "IPA"
              name = "FahrInfo" }
          ext = Some "BVG.1"
          ver = "1.44"
          auth =
            { ``type`` = "AID"
              aid = "YoJ05NartnanEGCj" } }
