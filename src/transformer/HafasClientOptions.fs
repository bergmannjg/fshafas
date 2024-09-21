module HafasClientOptions

//
// Hafas domain specfic transformations
//

let prelude =
    """// generated by ts2fable and transformer
namespace HafasClient

open System

open Fable.Core
open Fable.Core.JS

/// <namespacedoc>
///   <summary>FsHafas client types generated from <a href="https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts">TS types</a></summary>
/// </namespacedoc>
type Icon =
    { ``type``: string
      title: string option }

type IndexMap<'s, 'b when 's: comparison>(defaultValue: 'b) =
    [<EmitIndexer>]
    member __.Item
        with get (s: 's): 'b = jsNative
        and set s b = jsNative

    [<Emit("(Object.keys($0))")>]
    member __.Keys: 's [] = jsNative
"""

let postlude =
    """and [<StringEnum>] LocationType = | [<CompiledName "location">] Location

and [<StringEnum>] StationType = | [<CompiledName "station">] Station

and [<StringEnum>] StopType = | [<CompiledName "stop">] Stop

and [<StringEnum>] LineType = | [<CompiledName "line">] Line

and [<StringEnum>] JourneyType = | [<CompiledName "journey">] Journey

and [<StringEnum>] RouteType = | [<CompiledName "route">] Route

and [<StringEnum>] RegionType = | [<CompiledName "region">] Region

and [<StringEnum>] ScheduleType = | [<CompiledName "schedule">] Schedule

and [<StringEnum>] OperatorType = | [<CompiledName "operator">] Operator

and [<StringEnum>] GeometryType = | [<CompiledName "Point">] Point

and [<StringEnum>] FeatureType = | [<CompiledName "Feature">] Feature

and [<StringEnum>] FeatureCollectionType = | [<CompiledName "featureCollection">] FeatureCollection

#if FABLE_JS
and [<Erase>] StationStopLocation =
    | Station of Station
    | Stop of Stop
    | Location of Location

and [<Erase>] StationStop =
    | Station of Station
    | Stop of Stop

and [<Erase>] StopLocation =
    | Stop of Stop
    | Location of Location

and [<Erase>] HintStatusWarning =
    | Hint of Hint
    | Status of Status
    | Warning of Warning
#else
and StationStopLocation =
    | Station of Station
    | Stop of Stop
    | Location of Location

and StationStop =
    | Station of Station
    | Stop of Stop

and StopLocation =
    | Stop of Stop
    | Location of Location

and HintStatusWarning =
    | Hint of Hint
    | Status of Status
    | Warning of Warning
#endif
"""

let transformType str =
    if str = "ReadonlyArray" then "array"
    else if str = "ResizeArray" then "array"
    else str

let escapes = [| "type"; "default"; "when"; "end"; "public"; "to"; "class" |]

let escapeIdent str =
    if Array.contains str escapes then
        "``" + str + "``"
    else
        str

let excludeTypes =
    [| "ReadonlyArray"; "IExports"; "FeatureProperties"; "RealtimeDataUpdatedAt" |]

let memberTypes = [| "Profile"; "HafasClient" |]

let integerTypeVals =
    [| "*", "transferTime"
       "*", "transfers"
       "*", "delay"
       "*", "distance"
       "*", "departureDelay"
       "*", "arrivalDelay"
       "*", "transfers"
       "*", "maxTransfers"
       "*", "maxDuration"
       "*", "duration"
       "*", "results"
       "*", "bitmasks"
       "*", "nr"
       "*", "age"
       "*", "``class``"
       "*", "priority"
       "*", "realtimeDataFrom"
       "*", "categories"
       "*", "realtimeDataUpdatedAt"
       "LoyaltyCard", "discount"
       "Cycle", "min"
       "Cycle", "max"
       "Frame", "t"
       "Edge", "dir"
       "IcoCrd", "x"
       "IcoCrd", "y"
       "RadarOptions", "frames" |]

let case1OfU2TypeVals = [| "DateTime", "float"; "string", "float" |]

let transformTypeVals =
    [| "*", "icon", "Icon option"
       "Location", "``type``", "LocationType"
       "Station", "``type``", "StationType"
       "Stop", "``type``", "StopType"
       "Line", "``type``", "LineType"
       "Journey", "``type``", "JourneyType"
       "Hint", "``type``", "HintType"
       "Status", "``type``", "HintType"
       "Warning", "``type``", "WarningType"
       "Route", "``type``", "RouteType"
       "Region", "``type``", "RegionType"
       "Schedule", "``type``", "ScheduleType"
       "Operator", "``type``", "OperatorType"
       "Geometry", "``type``", "GeometryType"
       "Feature", "``type``", "FeatureType"
       "FeatureCollection", "``type``", "FeatureCollectionType"
       "IcoCrd", "``type``", "string option"
       "Feature", "properties", "StationStopLocation option" |]

let transformTypeDefns =
    [| "ScheduledDays", "IndexMap<string, bool>"
       "Products", "IndexMap<string, bool>"
       "Facilities", "IndexMap<string, string>"
       "Ids", "IndexMap<string, string>" |]

let transformsMemberType (typename: string) (name: string) (arr: (string * string * string) array) =
    let index =
        Array.tryFindIndex (fun (t, s, _) -> (t = "*" || t = typename) && s = name) arr

    if (index.IsSome) then
        let (_, _, transform) = arr.[index.Value]
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

let isIntegerTypeVal (typename: string) (membername: string) =
    integerTypeVals
    |> Array.exists (fun (t, s) -> (t = "*" || t = typename) && s = membername)

let isCase1OfU2TypeVals (case1: string) (case2: string) =
    case1OfU2TypeVals |> Array.exists (fun (t, s) -> t = case1 && s = case2)

let transformsTypeDefn (name: string) = transformsType name transformTypeDefns

let excludesType (name: string) = Array.contains name excludeTypes

let toMemberType (name: string) = Array.contains name memberTypes

let getIntersectionTypes (name: string) =
    if name = "JourneysOptions" then
        [| "JourneysOptionsCommon"; "JourneysOptionsDbProfile" |]
    else
        [||]

let options: Transformer.TransformerOptions =
    { prelude = Some prelude
      postlude = Some postlude
      useRecursiveTypes = true
      escapeIdent = escapeIdent
      transformType = transformType
      excludesType = excludesType
      toMemberType = toMemberType
      isIntegerTypeVal = isIntegerTypeVal
      isCase1OfU2TypeVals = isCase1OfU2TypeVals
      transformsTypeVal = transformsTypeVal
      transformsTypeDefn = transformsTypeDefn
      getIntersectionTypes = getIntersectionTypes }
