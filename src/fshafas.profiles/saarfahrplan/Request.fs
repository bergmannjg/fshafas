namespace FsHafas.Profiles.SaarFahrplanConfig

module internal Request =

    open FsHafas.Raw

    let request: RawRequest =
        { lang = "de"
          svcReqL = [||]
          client =
            { id = "ZPS-SAAR"
              v = "1000070"
              ``type`` = "AND"
              name = "Saarfahrplan" }
          ext = None
          ver = "1.40"
          auth =
            { ``type`` = "AID"
              aid = "51XfsVqgbdA6oXzHrx75jhlocRg6Xe" } }
