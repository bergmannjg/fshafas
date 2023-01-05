# Hafas-client Fable bindings

Fable bindings for [hafas-client](https://github.com/public-transport/hafas-client) and [hafas-client-types](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts)

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
