namespace FsHafas.Profiles.DbConfig

module internal Request =

    open FsHafas.Raw

    let request: RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "DB"
              v = "21120000"
              ``type`` = "AND"
              name = "DB Navigator" }
          ext = Some "DB.R21.12.a"
          ver = "1.34"
          auth =
            { ``type`` = "AID"
              aid = "n91dB8Z77MLdoR0K" } }
