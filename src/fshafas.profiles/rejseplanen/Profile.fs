namespace FsHafas.Profiles

module Rejseplanen =

    let profile = FsHafas.Api.Profile.defaultProfile ()

    profile._locale <- "da-DK"
    profile._timezone <- "Europe/Copenhagen"
    profile._endpoint <- "https://mobilapps.rejseplanen.dk/bin/iphone.exe"
    profile.salt <- ""
    profile.cfg <- Some { polyEnc = "GPA"; rtMode = None }
    profile.baseRequest <- Some RejseplanenConfig.Request.request
    profile._products <- RejseplanenConfig.Products.products
    profile.departuresGetPasslist <- false
    profile.departuresStbFltrEquiv <- false
    profile._trip <- Some true
    profile._radar <- Some true
    profile._lines <- Some false
    profile._remarks <- Some false
    profile._reachableFrom <- Some false
