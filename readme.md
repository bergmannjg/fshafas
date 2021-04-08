# Hafas Client in F\#

Ths is a port of the JavaScript [hafas-client library](https://github.com/public-transport/hafas-client) to F#.

The hafas endpoints Db, Bvg and Svv are supported.

The F# library compiles to dotnet and JavaScript (via [Fable](https://github.com/fable-compiler/Fable)).

## Interfaces

The library exposes  3 interfaces:

1) a direct (`raw`) interface to the hafas endpoints,
2) a F# async based interface corresponding to hafas-client api,
3) a JS promise based interface corresponding to the TS [Type definitions](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) for hafas-client.

## Compilation to JavaScript

The JS promise based interface compiles via Fable to a JavaScript library, this library and the original hafas-client can be used almost interchangeably.

Differences:

* *String Literal Unions* are compiled via ts2fable to F# Discriminated Unions and these are compiled via Fable to Jvascript classes. The mapping *.toString().toLowerCase()* can eliminate the differences.

The following diagram should commute modulo the aforementioned differences:

JavaScript/TypeScript | Transformation | F# |
:---------------:|:--------:|:------:|
[hafas-client TS types](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) | => [ts2fable](https://github.com/fable-compiler/ts2fable) and [Transformer](./src/transformer) => | hafas-client F# types |
 &#8659;|  | &#8659; implementation |
hafas-client TS types, JS Program | <= [fable](https://github.com/fable-compiler/fable) <= | hafas-client F# types, F# program|

## Building

Requirements are:

* [dotnet SDK 5](https://dotnet.microsoft.com/download)
* [node.js](https://nodejs.org/en/)

Run `./scripts/build.sh` at the root folder.

Targets are:

* *lib*: compile to dotnet dll,
* *test*: compile to dotnet dll and run tests,
* *csharp*: compile example C# program using the fshafas lib,
* *fable*: compile F# program via Fable to JavaScript,
* *pack*: compile lib via fable and webpack to a npm package.

Utility scripts:

* [create-types.sh](./scripts/create-types.sh): keep the F# types in sync with the corresponding TS types
* [create-test-fixtures.sh](./scripts/create-test-fixtures.sh): create test fixtures using the JavaScript hafas-client library.

## Using

* [fshafas.fsx](./scripts/fshafas.fsx): F# Interactive example script,
* [App.fs](src/fshafas.fable/App.fs): F# app running with nodejs,
* [Program.cs](src/fshafas.csharp/Program.cs): C# program using the F# lib,
* [FahrplanApp](https://github.com/bergmannjg/FahrplanApp): Android TypeScript app using the npm package.
