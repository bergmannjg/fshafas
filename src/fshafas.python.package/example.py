import asyncio
import sys
from fshafas import HafasClient
from fshafas.profiles import db_profile

async def main(argv) -> int:
    with HafasClient(db_profile) as client:
        journeys = await client.journeys(argv[0], argv[1])
        for j in journeys.journeys:
            for l in j.legs:
                print(l.origin.fields[0].name, l.destination.fields[0].name, l.departure)
    return 0

if __name__ == "__main__":
    if len(sys.argv) == 3:
        asyncio.run(main(sys.argv[1:]))