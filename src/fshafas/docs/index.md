# Hafas Client in F\#

Ths is a port of the JavaScript [hafas-client library](https://github.com/public-transport/hafas-client) to F#.

The hafas endpoints Db, Bvg and Svv are supported.

The F# library compiles to dotnet and JavaScript (via [Fable](https://github.com/fable-compiler/Fable)).

## Interfaces

The library exposes  3 interfaces:

- a [direct (`raw`) interface](reference/fshafas-api-hafasrawclient.html) to the hafas endpoints,
- a [F# async based interface](reference/fshafas-api-hafasasyncclient.html) corresponding to hafas-client api,
- a [JS promise based interface](reference/fshafas-client-hafasclient.html) corresponding to the TS [Type definitions](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) for hafas-client.

The library compiles via Fable to a webpack module with [this](https://github.com/bergmannjg/fshafas/blob/main/src/fshafas.fable.package/fs-hafas-client/fshafas.bundle.d.ts) TS Type definition.

## Examples

### HafasAsyncClient with F\#

```fsharp
use client = new FsHafas.Api.HafasAsyncClient(FsHafas.Client.ProfileId.Db)
async {
    let! locations = client.AsyncLocations "Hannover" (Some Default.LocationsOptions)

    FsHafas.Printf.Short.Locations locations |> printfn "%s"
}
|> Async.RunSynchronously
```

### HafasClient with F\# (compiles to JavaScript and runs with Node.js)

```fsharp
let client = FsHafas.Api.HafasClient(FsHafas.Client.ProfileId.Db) :> FsHafas.Client.HafasClient

client.locations "Hannover" (Some { Default.LocationsOptions with results = Some 3 })
|> Promise.iter (FsHafas.Printf.Short.Locations >> printfn "%s")
```

### HafasAsyncClient with C\#

```csharp
using (var client = new FsHafas.Api.HafasAsyncClient(FsHafas.Client.ProfileId.Db))
{
    var locations = await HafasAsyncClient.toTask(client.AsyncLocations("Hannover", Default.LocationsOptions));

    Console.WriteLine(FsHafas.Printf.Short.Locations(locations));
}
```

### Using the FsHafas webpack module in JavaScript (example with [hafas-client library](https://github.com/public-transport/hafas-client))

```js
const createClient = require('hafas-client');
const dbProfile = require('hafas-client/p/db');

import { fshafas } from "fs-hafas-client";

const client = choose ? fshafas.createClient('db') : createClient(dbProfile, 'agent');

const locations = () => {
    client.locations('Hannover', { results: 3, linesOfStops: true })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}
```