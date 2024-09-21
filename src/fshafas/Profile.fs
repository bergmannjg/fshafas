namespace FsHafas.Api

open FsHafas.Parser

/// <summary>Default profile to build profiles for endpoints.</summary>
module Profile =

    let defaultProfile () =
        FsHafas.Endpoint.Profile(
            "de-DE",
            "Europe/Berlin",
            (fun _ c -> c),
            id,
            id,
            (fun _ q -> q),
            (fun _ q -> q),
            Common.parseCommon,
            ArrivalOrDeparture.parseArrival,
            ArrivalOrDeparture.parseDeparture,
            Hint.parseHint,
            Icon.parseIcon,
            Polyline.parsePolyline,
            Location.parseLocations,
            Line.parseLine,
            Journey.parseJourney,
            JourneyLeg.parseJourneyLeg,
            Movement.parseMovement,
            Operator.parseOperator,
            JourneyLeg.parsePlatform,
            Stopover.parseStopover,
            Stopover.parseStopovers,
            Trip.parseTrip,
            When.parseWhen,
            DateTime.parseDateTime,
            ProductsBitmask.parseBitmask,
            Warning.parseWarning,
            PrognosisType.parsePrognosisType,
            ScheduledDays.parseScheduledDays
        )
