namespace FsHafas.Profiles.MobilNrwConfig

module internal Request =

    open FsHafas.Raw

    let request: RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "DB-REGIO-NRW"
              v = "6000300"
              ``type`` = "IPH"
              name = "NRW" }
          ext = None
          ver = "1.34"
          auth =
            { ``type`` = "AID"
              aid = "Kdf0LNRWYg5k3499" } }
