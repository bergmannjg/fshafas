namespace DbVendo.Parser

module internal Location =

    open FsHafas.Client
    open DbVendo
    open DbVendo.Client

    let parseLocation (response: Raw.Location) : StationStopLocation =
        let location =
            { Default.Location with
                id = response.evaNr
                name = response.name
                latitude = Option.getValue response.position (fun c -> c.latitude)
                longitude = Option.getValue response.position (fun c -> c.longitude) }

        match response.locationType, response.evaNr with
        | None, Some _
        | Some "ST", _ ->
            StationStopLocation.Stop
                { Default.Stop with
                    id = response.evaNr
                    name = response.name
                    location = Some location
                    products = Option.getValue response.products (fun p -> Some(Products.parseProducts p)) }
        | _ -> StationStopLocation.Location location

    let parseLocations (response: Raw.LocationsResponse) : array<StationStopLocation> =
        response |> Array.map parseLocation
