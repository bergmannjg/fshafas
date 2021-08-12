namespace FsHafas.Parser

#if FABLE_COMPILER
open Fable.Core
#endif

open FsHafas.Client

/// <exclude>Options</exclude>
type Options =
    { remarks: bool
      stopovers: bool
      polylines: bool
      scheduledDays: bool
      subStops: bool
      entrances: bool
      linesOfStops: bool
      firstClass: bool }

type internal CommonData =
    { operators: FsHafas.Client.Operator []
      locations: U3<Station, Stop, Location> []
      lines: FsHafas.Client.Line []
      hints: (U3<FsHafas.Client.Hint, FsHafas.Client.Status, FsHafas.Client.Warning> option) [] }

type internal ParsedWhen =
    { ``when``: string option
      plannedWhen: string option
      prognosedWhen: string option
      delay: int option }

type internal Platform =
    { platform: string option
      plannedPlatform: string option
      prognosedPlatform: string option }

type internal Profile =
    { locale: string
      timezone: string
      endpoint: string
      salt: string
      cfg: FsHafas.Raw.Cfg option
      baseRequest: FsHafas.Raw.RawRequest option
      products: array<FsHafas.Client.ProductType>
      trip: bool option
      radar: bool option
      refreshJourney: bool option
      journeysFromTrip: bool option
      reachableFrom: bool option
      journeysWalkingSpeed: bool option
      tripsByName: bool option
      remarks: bool option
      lines: bool option
      journeysOutFrwd: bool
      departuresGetPasslist: bool
      departuresStbFltrEquiv: bool
      formatStation: string -> string
      transformJourneysQuery: FsHafas.Client.JourneysOptions option -> FsHafas.Raw.TripSearchRequest -> FsHafas.Raw.TripSearchRequest
      parseCommon: Context -> FsHafas.Raw.RawCommon -> CommonData
      parseArrival: Context -> FsHafas.Raw.RawJny -> FsHafas.Client.Alternative
      parseDeparture: Context -> FsHafas.Raw.RawJny -> FsHafas.Client.Alternative
      parseHint: Context -> FsHafas.Raw.RawRem -> U3<FsHafas.Client.Hint, FsHafas.Client.Status, FsHafas.Client.Warning> option
      parsePolyline: Context -> FsHafas.Raw.RawPoly -> FsHafas.Client.FeatureCollection
      parseLocations: Context -> FsHafas.Raw.RawLoc [] -> U3<Station, Stop, Location> []
      parseLine: Context -> FsHafas.Raw.RawProd -> FsHafas.Client.Line
      parseJourney: Context -> FsHafas.Raw.RawOutCon -> FsHafas.Client.Journey
      parseJourneyLeg: Context -> FsHafas.Raw.RawSec -> string -> FsHafas.Client.Leg
      parseMovement: Context -> FsHafas.Raw.RawJny -> FsHafas.Client.Movement
      parseOperator: Context -> FsHafas.Raw.RawOp -> FsHafas.Client.Operator
      parsePlatform: Context -> string option -> string option -> bool option -> Platform
      parseStopover: Context -> FsHafas.Raw.RawStop -> string -> FsHafas.Client.StopOver
      parseStopovers: Context -> FsHafas.Raw.RawStop [] option -> string -> (FsHafas.Client.StopOver [] option)
      parseTrip: Context -> FsHafas.Raw.RawJny -> FsHafas.Client.Trip
      parseWhen: Context -> string -> string option -> string option -> int option -> bool option -> ParsedWhen
      parseDateTime: Context -> string -> string option -> int option -> string option
      parseBitmask: Context -> int -> FsHafas.Client.Products
      parseWarning: Context -> FsHafas.Raw.RawHim -> FsHafas.Client.Warning }

and internal Context =
    { profile: Profile
      opt: Options
      common: CommonData
      res: FsHafas.Raw.RawResult }
