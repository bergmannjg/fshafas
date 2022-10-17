namespace FsHafas.Profiles

module MobilNrw =

    let profile = FsHafas.Api.Profile.defaultProfile ()

    profile._locale <- "de-DE"
    profile._timezone <- "Europe/Berlin"
    profile._endpoint <- "https://nrw.hafas.de/bin/mgate.exe"
    profile.salt <- ""
    profile.cfg <- Some { polyEnc = "GPA"; rtMode = None }
    profile.baseRequest <- Some MobilNrwConfig.Request.request
    profile._products <- MobilNrwConfig.Products.products
    profile._trip <- Some true
    profile._radar <- Some true
    profile._lines <- Some true
    profile._remarks <- Some true
    profile._reachableFrom <- Some true
