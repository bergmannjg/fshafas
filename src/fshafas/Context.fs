namespace FsHafas

open FsHafas.Client

#if FABLE_COMPILER
open Fable.Core
#endif

type Log() =
    static let mutable debug = false

    static member Debug
        with get () = debug
        and set (v) = debug <- v

    static member Print (msg: string) (o: obj) = if debug then printfn "%s %A" msg o

/// Intersection of Client.XXXOptions used in parsers
type Options =
    { remarks: bool
      stopovers: bool
      polylines: bool
      scheduledDays: bool
      subStops: bool
      entrances: bool
      linesOfStops: bool
      firstClass: bool }

type CommonData =
    { operators: Client.Operator []
      locations: U3<Station, Stop, Location> []
      lines: Client.Line []
      hints: U3<Client.Hint, Client.Status, Client.Warning> [] }

type When =
    { ``when``: string option
      plannedWhen: string option
      prognosedWhen: string option
      delay: int option }

type Platform =
    { platform: string option
      plannedPlatform: string option
      prognosedPlatform: string option }

type ProfileId =
    | Db
    | Bvg
    | Svv

type Profile =
    { locale: string
      timezone: string
      endpoint: string
      salt: string
      cfg: Raw.Cfg option
      baseRequest: Raw.RawRequest option
      products: array<Client.ProductType>
      trip: bool option
      radar: bool option
      refreshJourney: bool option
      reachableFrom: bool option
      journeysWalkingSpeed: bool option
      tripsByName: bool option
      remarks: bool option
      lines: bool option
      journeysOutFrwd: bool
      departuresGetPasslist: bool
      departuresStbFltrEquiv: bool
      formatStation: string -> string
      transformJourneysQuery: Client.JourneysOptions option -> Raw.TripSearchRequest -> Raw.TripSearchRequest
      parseCommon: Context -> Raw.RawCommon -> CommonData
      parseArrival: Context -> Raw.RawJny -> Client.Alternative
      parseDeparture: Context -> Raw.RawJny -> Client.Alternative
      parseHint: Context -> Raw.RawRem -> U3<Client.Hint, Client.Status, Client.Warning> option
      parsePolyline: Context -> Raw.RawPoly -> Client.FeatureCollection
      parseLocations: Context -> Raw.RawLoc [] -> U3<Station, Stop, Location> []
      parseLine: Context -> Raw.RawProd -> Client.Line
      parseJourney: Context -> Raw.RawOutCon -> Client.Journey
      parseJourneyLeg: Context -> Raw.RawSec -> string -> Client.Leg
      parseMovement: Context -> Raw.RawJny -> Client.Movement
      parseOperator: Context -> Raw.RawOp -> Client.Operator
      parsePlatform: Context -> string option -> string option -> bool option -> Platform
      parseStopover: Context -> Raw.RawStop -> string -> Client.StopOver
      parseStopovers: Context -> Raw.RawStop [] option -> string -> (Client.StopOver [] option)
      parseTrip: Context -> Raw.RawJny -> Client.Trip
      parseWhen: Context -> string -> string option -> string option -> int option -> bool option -> When
      parseDateTime: Context -> string -> string option -> int option -> string option
      parseBitmask: Context -> int -> Client.Products
      parseWarning: Context -> Raw.RawHim -> Client.Warning }

and Context =
    { profile: Profile
      opt: Options
      common: CommonData
      res: Raw.RawResult }
