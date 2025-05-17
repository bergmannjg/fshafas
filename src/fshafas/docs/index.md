# Hafas Client in F\#

Ths is a port of the JavaScript [hafas-client library](https://github.com/public-transport/hafas-client) to F#.

The hafas endpoints Bvg and Svv are supported.

The [db vendo endpoint](https://github.com/public-transport/db-vendo-client) is supported.

The F# library compiles to dotnet and (via [Fable](https://github.com/fable-compiler/Fable)) to [JavaScript](./js) and [Python](./py).

## Interfaces

The library exposes 4 interfaces:

- F# async based interface [hafas async client](reference/fshafas-api-hafasasyncclient.html),
- F# async based interface [db-vendo async client](reference/dbvendo-api-dbvendoasyncclient.html),
- JS promise based interface [hafas client](reference/fshafas-client-hafasclient.html) corresponding to the TS [Type definitions](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) for hafas-client (using the above async interfaces),
- interface to build [profiles](reference/fshafas-client-profile.html) for endpoints.

The library compiles via Fable to a webpack module with [this](https://github.com/bergmannjg/fshafas/blob/main/src/fshafas.javascript.package/fs-hafas-client/fshafas.bundle.d.ts) TS Type definition.

## Examples

### HafasAsyncClient with F\#

```fsharp
use client = new FsHafas.Api.HafasAsyncClient(FsHafas.Profiles.Oebb.profile)
async {
    let! locations = client.AsyncLocations "Wien" (Some Default.LocationsOptions)

    FsHafas.Printf.Short.Locations locations |> printfn "%s"
}
|> Async.RunSynchronously
```

### DbVendoAsyncClient with F\#

```fsharp
use client = new DbVendo.Api.DbVendoAsyncClient()
async {
    let! locations = client.AsyncLocations "Hannover" (Some Default.LocationsOptions)

    FsHafas.Printf.Short.Locations locations |> printfn "%s"
}
|> Async.RunSynchronously
```

### HafasAsyncClient with C\#

```csharp
using (var client = new FsHafas.Api.HafasAsyncClient(FsHafas.Profiles.Oebb.profile))
{
    var locations = await HafasAsyncClient.toTask(client.AsyncLocations("Hannover", Default.LocationsOptions));

    Console.WriteLine(FsHafas.Printf.Short.Locations(locations));
}
```
