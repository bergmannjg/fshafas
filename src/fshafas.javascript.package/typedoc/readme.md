FsHafas-JavaScript is a client for the [HAFAS](https://de.wikipedia.org/wiki/HAFAS) public transport APIs.
It is generated (via [Fable](https://github.com/fable-compiler/Fable>))
by the [fshafas](https://github.com/bergmannjg/fshafas/) F# library.

## Example

JavaScript app using fshafas and hafas-client npm packages (see [HafasClient](./interfaces/HafasClient.html) interface)

```js
import { createClient } from 'hafas-client';
import { profile as dbProfile } from 'hafas-client/p/db/index.js';

import { fshafas } from 'fs-hafas-client';
import { profiles } from 'fs-hafas-profiles';

const args = process.argv.slice(2);

/** @type {HafasClient} */
const client = args[0] === "fshafas" ? fshafas.createClient(profiles.getProfile('db')) : createClient(dbProfile, 'agent');

const locations = (name) => {
    client.locations(name, { results: 3, linesOfStops: true })
        .then(result => { result.forEach(s => { console.log(s.type, s.id, s.name); }); })
        .catch(console.error);
}

locations(args[1])
```

## Installation

```sh
npm install https://github.com/bergmannjg/fshafas/releases/download/2.3.0/fs-hafas-client-2.3.0.tgz
npm install https://github.com/bergmannjg/fshafas/releases/download/2.3.0/fs-hafas-profiles-2.3.0.tgz
npm install hafas-client
```
