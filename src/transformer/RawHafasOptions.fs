module RawHafasOptions

//
// Hafas domain specfic transformations
//

let prelude =
    """// generated by ts2fable and transformer
namespace FsHafas.Raw

open FsHafas.Client

/// <namespacedoc>
///   <summary>Types of raw hafas api generated from <a href="https://github.com/bergmannjg/hafas-client/blob/add-types-in-jsdoc/types-raw-api.ts">TS types</a></summary>
/// </namespacedoc>
"""

let postlude =
    """
"""

let transformType str =
    if str = "ReadonlyArray" then "array"
    else if str = "ResizeArray" then "array"
    else str

let escapeIdent str =
    if str = "type" then "``" + str + "``"
    else if str = "match" then "``" + str + "``"
    else if str = "default" then "``" + str + "``"
    else if str = "when" then "``" + str + "``"
    else if str = "public" then "``" + str + "``"
    else if str = "to" then "``" + str + "``"
    else str

let excludeTypes = [| "ReadonlyArray"; "IExports"; "HafasClient" |]

let transformTypeVals =
    [| "r", "int option"
       "req",
       "U14<LocMatchRequest, TripSearchRequest, JourneyDetailsRequest, StationBoardRequest, ReconstructionRequest, JourneyMatchRequest, LocGeoPosRequest, LocGeoReachRequest, LocDetailsRequest, JourneyGeoPosRequest, HimSearchRequest, LineMatchRequest, ServerInfoRequest, SearchOnTripRequest>" |]

let transformTypeDefns = [||]

let transformsMemberType (typename: string) (name: string) (arr: (string * string) array) =
    let index = Array.tryFindIndex (fun (s, _) -> s = name) arr

    if (index.IsSome) then
        let (_, transform) = arr.[index.Value]
        Some transform
    else
        None

let transformsType (name: string) (arr: (string * string) array) =
    let index = Array.tryFindIndex (fun (s, _) -> s = name) arr

    if (index.IsSome) then
        let (_, transform) = arr.[index.Value]
        Some transform
    else
        None

let transformsTypeVal (typename: string) (membername: string) =
    transformsMemberType typename membername transformTypeVals

let transformsTypeDefn (name: string) = transformsType name transformTypeDefns

let excludesType (name: string) =
    Array.exists (fun s -> s = name) excludeTypes

let options: Transformer.TransformerOptions =
    { prelude = Some prelude
      postlude = Some postlude
      useRecursiveTypes = true
      escapeIdent = escapeIdent
      transformType = transformType
      excludesType = excludesType
      toMemberType = fun _ -> false
      isIntegerTypeVal = fun _ _ -> false
      isCase1OfU2TypeVals = fun _ _ -> false
      transformsTypeVal = transformsTypeVal
      transformsTypeDefn = transformsTypeDefn
      getIntersectionTypes = fun _ -> [||] }
