# TypeScript app using fshafas, hafas-client and db-vendo-client npm packages

* build: npm install && tsc
* run: node dist/index.js journeys Berlin Hamburg

## Choosing client libraries

All clients have the [interface](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts):

- use hafas-client with arg "--hafas"
- use db-vendo-client with arg "--dbvendo"
- otherwise use fshafas client
