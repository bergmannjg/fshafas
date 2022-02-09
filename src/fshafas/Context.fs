namespace FsHafas.Endpoint

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

type CommonData =
    { operators: FsHafas.Client.Operator []
      locations: U3<Station, Stop, Location> []
      lines: FsHafas.Client.Line []
      hints: (U3<FsHafas.Client.Hint, FsHafas.Client.Status, FsHafas.Client.Warning> option) []
      polylines: FeatureCollection [] }

type ParsedWhen =
    { ``when``: string option
      plannedWhen: string option
      prognosedWhen: string option
      delay: int option }

type Platform =
    { platform: string option
      plannedPlatform: string option
      prognosedPlatform: string option }

/// A profile is a specific customisation for each endpoint.
/// It parses data from the API differently, add additional information, or enable non-default methods.
type Profile
    (
        locale,
        timezone,
        formatStation,
        transformJourneysQuery,
        parseCommon,
        parseArrival,
        parseDeparture,
        parseHint,
        parsePolyline,
        parseLocations,
        parseLine,
        parseJourney,
        parseJourneyLeg,
        parseMovement,
        parseOperator,
        parsePlatform,
        parseStopover,
        parseStopovers,
        parseTrip,
        parseWhen,
        parseDateTime,
        parseBitmask,
        parseWarning
    ) =
    member val salt: string = "" with get, set
    member val cfg: FsHafas.Raw.Cfg option = None with get, set
    member val baseRequest: FsHafas.Raw.RawRequest option = None with get, set
    member val journeysOutFrwd: bool = false with get, set
    member val departuresGetPasslist: bool = true with get, set
    member val departuresStbFltrEquiv: bool = true with get, set
    member val formatStation: string -> string = formatStation with get, set

    member val transformJourneysQuery: FsHafas.Client.JourneysOptions option
        -> FsHafas.Raw.TripSearchRequest
        -> FsHafas.Raw.TripSearchRequest = transformJourneysQuery with get, set

    member val parseCommon: Context -> FsHafas.Raw.RawCommon -> CommonData = parseCommon with get, set
    member val parseArrival: Context -> FsHafas.Raw.RawJny -> FsHafas.Client.Alternative = parseArrival with get, set

    member val parseDeparture: Context -> FsHafas.Raw.RawJny -> FsHafas.Client.Alternative =
        parseDeparture with get, set

    member val parseHint: Context
        -> FsHafas.Raw.RawRem
        -> U3<FsHafas.Client.Hint, FsHafas.Client.Status, FsHafas.Client.Warning> option = parseHint with get, set

    member val parsePolyline: Context -> FsHafas.Raw.RawPoly -> FsHafas.Client.FeatureCollection =
        parsePolyline with get, set

    member val parseLocations: Context -> FsHafas.Raw.RawLoc [] -> U3<Station, Stop, Location> [] =
        parseLocations with get, set

    member val parseLine: Context -> FsHafas.Raw.RawProd -> FsHafas.Client.Line = parseLine with get, set
    member val parseJourney: Context -> FsHafas.Raw.RawOutCon -> FsHafas.Client.Journey = parseJourney with get, set

    member val parseJourneyLeg: Context -> FsHafas.Raw.RawSec -> string -> FsHafas.Client.Leg =
        parseJourneyLeg with get, set

    member val parseMovement: Context -> FsHafas.Raw.RawJny -> FsHafas.Client.Movement = parseMovement with get, set
    member val parseOperator: Context -> FsHafas.Raw.RawOp -> FsHafas.Client.Operator = parseOperator with get, set

    member val parsePlatform: Context -> string option -> string option -> bool option -> Platform =
        parsePlatform with get, set

    member val parseStopover: Context -> FsHafas.Raw.RawStop -> string -> FsHafas.Client.StopOver =
        parseStopover with get, set

    member val parseStopovers: Context -> FsHafas.Raw.RawStop [] option -> string -> (FsHafas.Client.StopOver [] option) =
        parseStopovers with get, set

    member val parseTrip: Context -> FsHafas.Raw.RawJny -> FsHafas.Client.Trip = parseTrip with get, set

    member val parseWhen: Context -> string -> string option -> string option -> int option -> bool option -> ParsedWhen =
        parseWhen with get, set

    member val parseDateTime: Context -> string -> string option -> int option -> string option =
        parseDateTime with get, set

    member val parseBitmask: Context -> int -> FsHafas.Client.Products = parseBitmask with get, set
    member val parseWarning: Context -> FsHafas.Raw.RawHim -> FsHafas.Client.Warning = parseWarning with get, set

    member val _locale = locale with get, set
    member val _timezone = timezone with get, set
    member val _endpoint = "" with get, set
    member val _products: ProductType [] = [||] with get, set
    member val _trip = None with get, set
    member val _radar = None with get, set
    member val _refreshJourney = None with get, set
    member val _journeysFromTrip = None with get, set
    member val _reachableFrom = None with get, set
    member val _journeysWalkingSpeed = None with get, set
    member val _tripsByName = None with get, set
    member val _remarks = None with get, set
    member val _remarksGetPolyline = None with get, set
    member val _lines = None with get, set

    interface FsHafas.Client.Profile with
        member __.locale = __._locale
        member __.timezone = __._timezone
        member __.endpoint = __._endpoint
        member __.products = __._products
        member __.trip = __._trip
        member __.radar = __._radar
        member __.refreshJourney = __._refreshJourney
        member __.journeysFromTrip = __._journeysFromTrip
        member __.reachableFrom = __._reachableFrom
        member __.journeysWalkingSpeed = __._journeysWalkingSpeed
        member __.tripsByName = __._tripsByName
        member __.remarks = __._remarks
        member __.remarksGetPolyline = __._remarksGetPolyline
        member __.lines = __._lines

and Context =
    { profile: Profile
      opt: Options
      common: CommonData
      res: FsHafas.Raw.RawResult }
