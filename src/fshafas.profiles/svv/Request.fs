namespace FsHafas.Profiles.SvvConfig

module internal Request =

    open FsHafas.Raw

    let request: RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "VAO"
              v = ""
              ``type`` = "WEB"
              name = "webapp" }
          ext = Some "VAO.11"
          ver = "1.39"
          auth =
            { ``type`` = "AID"
              aid = "wf7mcf9bv3nv8g5f" } }
