from __future__ import annotations
import sys
import asyncio
import requests
from typing import (Any, List, Optional, Tuple, TypeVar, Callable, Awaitable)
from fshafas.fable_modules.fable_library.array import (map)
from fshafas.fable_modules.fs_hafas_profiles_python.db.profile import profile as db_profile
from fshafas.fable_modules.fs_hafas_python.lib.transformations import (
    Default_LocationsOptions, Default_JourneysOptions, Default_RadarOptions)
from fshafas.fable_modules.fs_hafas_python.print import (
    Locations as printLocations, Journeys as printJourneys, Movements as prinMovements)
from fshafas.fable_modules.fs_hafas_python.types_hafas_client import (
    Station, Stop, Location, BoundingBox)
from fshafas.hafas_client import (HafasClient)
from util import (to_locations)

# example program for HafasClient


async def main(argv: List[str]) -> int:
    try:
        if len(argv) == 2 and argv[0] == "--locations":
            with HafasClient(db_profile) as client:

                stops = await client.locations(argv[1], Default_LocationsOptions)
                locations = to_locations(stops)
                print(printLocations(locations))

        if len(argv) == 3 and argv[0] == "--journeys":
            with HafasClient(db_profile) as client:
                journeys = await client.journeys(argv[1], argv[2], Default_JourneysOptions)
                print(printJourneys(journeys))

        if len(argv) == 1 and argv[0] == "--radar":
            rect = BoundingBox(52.2735877, 8.0596000,  51.7128598, 8.7404385)
            with HafasClient(db_profile) as client:
                opt = Default_RadarOptions
                opt.duration = 2400
                opt.frames = 10
                opt.products = client.productsOfMode(db_profile, "train")
                movements = await client.radar(rect, opt)
                print(prinMovements(movements))

        if len(argv) == 0 or argv[0] == "--help":
            print(
                "arguments: --locations name (i.e. Hannover) | --journeys from to (i.e. 8000152 8000036)")
    except Exception as e:
        arg10: str = str(e)
        print(arg10)

    return 0

if __name__ == "__main__":
    asyncio.run(main(sys.argv[1:]))
