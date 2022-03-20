# FsHafas library

This project is a port of the JavaScript [hafas-client library](https://github.com/public-transport/hafas-client) to F#.

The main correspondences are the generated types:

* [TypesHafasClient](./TypesHafasClient.fs), FsHafas client types generated from this [type definition](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts),
* [TypesRawHafasClient](./TypesRawHafasClient.fs), types of raw hafas api generated from this [type definition](https://github.com/bergmannjg/hafas-client/blob/add-types-in-jsdoc/types-raw-api.ts).

## Compilation to JavaScript

Dependencies:

* google-polyline
* isomorphic-fetch
* luxon
* md5
* slugg

## Compilation to Python

Dependencies:

* [python-slugify](https://github.com/un33k/python-slugify)
* [requests](https://docs.python-requests.org/en/latest/)
* [polyline](https://github.com/frederickjansen/polyline)