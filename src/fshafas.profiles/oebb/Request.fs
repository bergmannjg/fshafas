namespace FsHafas.Profiles.OebbConfig

module internal Request =

    open FsHafas.Raw

    let request: RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "OEBB"
              v = "6030600"
              ``type`` = "IPH"
              name = "oebbPROD-ADHOC" }
          ext = None
          ver = "1.41"
          auth =
            { ``type`` = "AID"
              aid = "OWDL4fE4ixNiPBBm" } }
