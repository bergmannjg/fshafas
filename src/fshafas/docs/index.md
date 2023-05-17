# Hafas Client in F\#

Ths is a port of the JavaScript [hafas-client library](https://github.com/public-transport/hafas-client) to F#.

The hafas endpoints Db, Bvg and Svv are supported.

The F# library compiles to dotnet and (via [Fable](https://github.com/fable-compiler/Fable)) to [JavaScript](./js) and Python.

## Interfaces

The library exposes 4 interfaces:

- a [direct (`raw`) interface](reference/fshafas-api-hafasrawclient.html) to the hafas endpoints,
- a [F# async based interface](reference/fshafas-api-hafasasyncclient.html) corresponding to hafas-client api,
- a [JS promise based interface](reference/fshafas-client-hafasclient.html) corresponding to the TS [Type definitions](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) for hafas-client,
- an interface to build [profiles](reference/fshafas-api-profile.html) for endpoints.

The library compiles via Fable to a webpack module with [this](https://github.com/bergmannjg/fshafas/blob/main/src/fshafas.javascript.package/fs-hafas-client/fshafas.bundle.d.ts) TS Type definition.

## Examples

### HafasAsyncClient with F\#

```fsharp
use client = new FsHafas.Api.HafasAsyncClient(FsHafas.Profiles.Db.profile)
async {
    let! locations = client.AsyncLocations "Hannover" (Some Default.LocationsOptions)

    FsHafas.Printf.Short.Locations locations |> printfn "%s"
}
|> Async.RunSynchronously
```

### HafasAsyncClient with C\#

```csharp
using (var client = new FsHafas.Api.HafasAsyncClient(FsHafas.Profiles.Db.profile))
{
    var locations = await HafasAsyncClient.toTask(client.AsyncLocations("Hannover", Default.LocationsOptions));

    Console.WriteLine(FsHafas.Printf.Short.Locations(locations));
}
```

### Using the FsHafas package in JavaScript (example with [hafas-client library](https://github.com/public-transport/hafas-client))

```js
const createClient = require('hafas-client');
const dbProfile = require('hafas-client/p/db');

import { fshafas } from 'fs-hafas-client';
import { profiles } from 'fs-hafas-profiles';

const client = choose ? fshafas.createClient(profiles.getProfile('db')) : createClient(dbProfile, 'agent');

const locations = () => {
    client.locations('Hannover', { results: 3, linesOfStops: true })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}
```

### Using the FsHafas python package

```py
import asyncio
import sys
from fshafas import HafasClient
from fshafas.profiles import db_profile 

async def main(argv) -> int:
    with HafasClient(db_profile) as client:
        journeys = await client.journeys(argv[0], argv[1])
        for j in journeys.journeys:
            for l in j.legs:
                print(l.origin.name, l.destination.name, l.departure)
    return 0

if __name__ == "__main__":
    asyncio.run(main(sys.argv[1:]))
```
