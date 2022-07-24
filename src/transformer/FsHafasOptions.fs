module FsHafasOptions

//
// Hafas domain specfic transformations
//

let prelude =
    """// generated by ts2fable and transformer
namespace FsHafas.Client

open System

#if FABLE_COMPILER
open Fable.Core
open Fable.Core.JS
#endif

/// <namespacedoc>
///   <summary>FsHafas client types generated from <a href="https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts">TS types</a></summary>
/// </namespacedoc>

"""

let transformType str =
    if str = "ReadonlyArray" then "array"
    else if str = "ResizeArray" then "array"
    else str

let escapes =
    [| "type"
       "default"
       "when"
       "end"
       "public"
       "to"
       "class" |]

let escapeIdent str =
    if Array.contains str escapes then
        "``" + str + "``"
    else
        str

let excludeTypes = [| "ReadonlyArray"; "IExports" |]

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

let case1OfU2TypeVals =
    [| "DateTime", "float"
       "string", "float" |]

let transformTypeVals =
    [| "*", "icon", "Icon option"
       "*", "``type``", "string option" |]

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
    case1OfU2TypeVals
    |> Array.exists (fun (t, s) -> t = case1 && s = case2)

let transformsTypeDefn (name: string) = transformsType name transformTypeDefns

let excludesType (name: string) = Array.contains name excludeTypes

let toMemberType (name: string) = Array.contains name memberTypes

let options: Transformer.TransformerOptions =
    { prelude = Some prelude
      postlude = None
      useRecursiveTypes = true
      escapeIdent = escapeIdent
      transformType = transformType
      excludesType = excludesType
      toMemberType = toMemberType
      isIntegerTypeVal = isIntegerTypeVal
      isCase1OfU2TypeVals = isCase1OfU2TypeVals
      transformsTypeVal = transformsTypeVal
      transformsTypeDefn = transformsTypeDefn }
