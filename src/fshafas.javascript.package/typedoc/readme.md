FsHafas-JavaScript is a client for the [HAFAS](https://de.wikipedia.org/wiki/HAFAS) public transport APIs.
It is generated (via [Fable](https://github.com/fable-compiler/Fable>))
by the [fshafas](https://github.com/bergmannjg/fshafas/) F# library.

## Example

JavaScript app using fshafas and hafas-client npm packages (see [HafasClient](./interfaces/HafasClient.html) interface)

```js
import { createClient } from 'hafas-client';
import { profile as oebbProfile } from 'hafas-client/p/oebb/index.js';

import { fshafas } from 'fs-hafas-client';
import { profiles } from 'fs-hafas-profiles';

const args = process.argv.slice(2);

/** @type {HafasClient} */
const client = args[0] === "fshafas" ? fshafas.createClient(profiles.getProfile('db')) : createClient(oebbProfile, 'agent');

const locations = (name) => {
    client.locations(name, { results: 3, linesOfStops: true })
        .then(result => { result.forEach(s => { console.log(s.type, s.id, s.name); }); })
        .catch(console.error);
}

locations(args[1])
```

## Installation

Replace x.y.z with current [release](https://github.com/bergmannjg/fshafas/releases).

```sh
npm install https://github.com/bergmannjg/fshafas/releases/download/x.y.z/fs-hafas-client-x.y.z.tgz
npm install https://github.com/bergmannjg/fshafas/releases/download/x.y.z/fs-hafas-profiles-x.y.z.tgz
npm install hafas-client
```
