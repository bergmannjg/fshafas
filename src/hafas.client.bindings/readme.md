# Hafas-client Fable bindings

Fable bindings for [hafas-client](https://github.com/public-transport/hafas-client), [db-vendo-client](https://github.com/public-transport/db-vendo-client) and [hafas-client-types](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts)

## Building

```sh
./build.sh FableBindings
```

## Usage

```f#
open Fable.Core
open HafasClient

promise {
    let client = createClient (getProfile ProfileId.Db)
    let! result = client.locations "Hannover" None
    printfn "%A" result
}
|> Promise.start
```
