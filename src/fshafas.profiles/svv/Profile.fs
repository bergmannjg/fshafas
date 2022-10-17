namespace FsHafas.Profiles

module Svv =

    let profile = FsHafas.Api.Profile.defaultProfile ()

    profile._locale <- "at-DE"
    profile._timezone <- "Europe/Vienna"
    profile._endpoint <- "https://fahrplan.salzburg-verkehr.at/bin/mgate.exe"
    profile.salt <- ""
    profile.cfg <- Some { polyEnc = "GPA"; rtMode = None }
    profile.baseRequest <- Some SvvConfig.Request.request
    profile._products <- SvvConfig.Products.products
    profile._trip <- Some true
    profile._lines <- Some true
    profile._remarks <- Some true
    profile._reachableFrom <- Some true
    profile.departuresGetPasslist <- false
    profile.departuresStbFltrEquiv <- false
