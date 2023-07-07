from __future__ import annotations
import sys
import asyncio
import requests
import traceback
from typing import Any, List, Optional, Tuple, TypeVar, Callable, Awaitable
from fshafas.profiles import db_profile
from fshafas import (
    HafasClient,
    printLocations,
    printJourneys,
    printTrips,
    printAlternatives,
    enable_logging,
    Default_LocationsOptions,
    Default_JourneysOptions,
    Default_TripsByNameOptions,
    Default_DeparturesArrivalsOptions,
)

# example program for HafasClient


async def main(argv: List[str]) -> int:
    try:
        if "--log" in argv:
            enable_logging()

        if len(argv) >= 2 and argv[0].startswith("--locations"):
            with HafasClient(db_profile) as client:
                stops = await client.locations(argv[1], Default_LocationsOptions)
                print(printLocations(stops))

        if len(argv) >= 3 and argv[0].startswith("--journeys"):
            with HafasClient(db_profile) as client:
                journeys = await client.journeys(
                    argv[1], argv[2], Default_JourneysOptions
                )
                print(printJourneys(journeys))

        if len(argv) >= 3 and argv[0].startswith("--bestprices"):
            with HafasClient(db_profile) as client:
                journeys = await client.bestprices(
                    argv[1], argv[2], Default_JourneysOptions
                )
                print(printJourneys(journeys))

        if len(argv) >= 2 and argv[0] == "--departures":
            with HafasClient(db_profile) as client:
                departures = await client.departures(
                    argv[1], Default_DeparturesArrivalsOptions
                )
                print(printAlternatives(departures.departures))

        if len(argv) >= 2 and argv[0] == "--arrivals":
            with HafasClient(db_profile) as client:
                arrivals = await client.arrivals(
                    argv[1], Default_DeparturesArrivalsOptions
                )
                print(printAlternatives(arrivals.arrivals))

        if len(argv) >= 2 and argv[0] == "--tripsByName":
            with HafasClient(db_profile) as client:
                trips = await client.tripsByName(argv[1], Default_TripsByNameOptions)
                print(printTrips(trips.trips))

        if len(argv) == 0 or argv[0] == "--help":
            print(
                "arguments: --locations name (i.e. Hannover) | --journeys from to (i.e. 8000152 8000036)"
            )
    except Exception as e:
        print("error: ", str(e))
        traceback.print_exc()

    return 0


if __name__ == "__main__":
    asyncio.run(main(sys.argv[1:]))
