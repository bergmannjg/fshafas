namespace FsHafas.Profiles

module Oebb =

    let profile = FsHafas.Api.Profile.defaultProfile ()

    profile._locale <- "de-AT"
    profile._timezone <- "Europe/Vienna"
    profile._endpoint <- "https://fahrplan.oebb.at/bin/mgate.exe"
    profile.salt <- ""
    profile.cfg <- Some { polyEnc = Some "GPA"; rtMode = None }
    profile.baseRequest <- Some OebbConfig.Request.request
    profile._products <- OebbConfig.Products.products
    profile.departuresGetPasslist <- false
    profile.departuresStbFltrEquiv <- false
    profile._trip <- Some true
    profile._radar <- Some true
    profile._lines <- Some false
    profile._remarks <- Some true
    profile._reachableFrom <- Some true
