namespace FsHafas.Profiles

module Db =

    /// dummy profile to use db vendo client via FsHafas.Client.HafasClient api.
    let profile: FsHafas.Client.Profile =
        { new FsHafas.Client.Profile with
            member _.locale = ""
            member _.timezone = ""
            member _.endpoint = FsHafas.Api.Profile.dbEndpoint
            member _.products = [||]
            member _.trip = None
            member _.radar = None
            member _.refreshJourney = None
            member _.reachableFrom = None
            member _.journeysWalkingSpeed = None
            member _.tripsByName = None
            member _.remarks = None
            member _.journeysFromTrip = None
            member _.remarksGetPolyline = None
            member _.lines = None }
