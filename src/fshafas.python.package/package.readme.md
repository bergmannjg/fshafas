# FsHafas

Python client for [HAFAS](https://de.wikipedia.org/wiki/HAFAS) public transport APIs.

The python package is generated from [Hafas Client in F#](https://github.com/bergmannjg/fshafas/tree/1.5.1) with [fable-py](https://www.nuget.org/packages/fable-py/), a tool that translates F# source files to Python (currently a prerelease).

Related:

* [hafas-client](https://github.com/public-transport/hafas-client) JavaScript client for HAFAS public transport APIs,
* [FPTF](https://github.com/public-transport/friendly-public-transport-format) Friendly Public Transport Format,
* [@types/hafas-client](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/743f4c8d5d8af49dfb635a31a8720f7bde5b823f/types/hafas-client/index.d.ts) Type definitions for hafas-client 5.24.0.

## Example

Retrieve journeys:

```py
import asyncio
import sys
from fshafas.fable_modules.fs_hafas_profiles_python.db.profile import profile
from fshafas.hafas_client import HafasClient

async def main(argv) -> int:
    with HafasClient(profile) as client:
        journeys = await client.journeys(argv[0], argv[1])
        for j in journeys.journeys:
            for l in j.legs:
                print(l.origin.name, l.destination.name, l.departure)
    return 0

if __name__ == "__main__":
    asyncio.run(main(sys.argv[1:]))
```

The class `HafasClient` corresponds to the Javascript [HafasClient interface](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/fb785106d6264285d452e2e7efb5c68c0639fbd8/types/hafas-client/index.d.ts#L1006).
