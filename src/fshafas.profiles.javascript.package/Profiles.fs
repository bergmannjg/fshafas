module App

let getProfile (profile: string) =
    match profile with
    | "db" -> FsHafas.Profiles.Db.profile :> FsHafas.Client.Profile
    | "bvg" -> FsHafas.Profiles.Bvg.profile :> FsHafas.Client.Profile
    | _ -> raise (System.ArgumentException("profile unkown: " + profile))
