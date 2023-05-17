## Example

JavaScript app using fs-hafas-client and hafas-client npm packages (see [HafasClient](./interfaces/HafasClient.html) interface)

```js
const createClient = require('hafas-client');
const dbProfile = require('hafas-client/p/db');

import { fshafas } from 'fs-hafas-client';
import { profiles } from 'fs-hafas-profiles';

/** @type {HafasClient} */
const client = choose ? fshafas.createClient(profiles.getProfile('db')) : createClient(dbProfile, 'agent');

const locations = () => {
    client.locations('Hannover', { results: 3, linesOfStops: true })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}
```
