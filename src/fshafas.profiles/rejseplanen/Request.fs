namespace FsHafas.Profiles.RejseplanenConfig

module internal Request =

    open FsHafas.Raw

    let request: RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "DK"
              v = ""
              ``type`` = "AND"
              name = "" }
          ext = Some "DK.9"
          ver = "1.43"
          auth =
            { ``type`` = "AID"
              aid = "irkmpm9mdznstenr-android" } }
