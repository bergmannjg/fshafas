namespace FsHafas.Profiles

module SaarFahrplan =

    let profile = FsHafas.Api.Profile.defaultProfile ()

    profile._locale <- "de-DE"
    profile._timezone <- "Europe/Berlin"
    profile._endpoint <- "https://saarfahrplan.de/bin/mgate.exe"
    profile.salt <- "HJtlubisvxiJxss"
    profile.addMicMac <- true
    profile.cfg <- Some { polyEnc = Some "GPA"; rtMode = None }
    profile.baseRequest <- Some SaarFahrplanConfig.Request.request
    profile._products <- SaarFahrplanConfig.Products.products
    profile._trip <- Some true
    profile._radar <- Some true
    profile._lines <- Some true
    profile._remarks <- Some true
    profile._reachableFrom <- Some true
