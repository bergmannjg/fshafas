module App

let getProfile (profile: string) =
    match profile.ToLower() with
    | "bvg" -> FsHafas.Profiles.Bvg.profile :> FsHafas.Client.Profile
    | "db" -> FsHafas.Profiles.Db.profile :> FsHafas.Client.Profile
    | "mobilnrw" -> FsHafas.Profiles.MobilNrw.profile :> FsHafas.Client.Profile
    | "oebb" -> FsHafas.Profiles.Oebb.profile :> FsHafas.Client.Profile
    | "rejseplanen" -> FsHafas.Profiles.Rejseplanen.profile :> FsHafas.Client.Profile
    | "saarfahrplan" -> FsHafas.Profiles.SaarFahrplan.profile :> FsHafas.Client.Profile
    | "svv" -> FsHafas.Profiles.Svv.profile :> FsHafas.Client.Profile
    | _ -> raise (System.ArgumentException("profile unkown: " + profile))
