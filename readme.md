# Hafas Client in F\#

Ths is a port of the JavaScript [hafas-client library](https://github.com/public-transport/hafas-client) to F#.

The hafas endpoints Db, Bvg and Svv are supported.

The F# library compiles to dotnet and JavaScript (via [Fable](https://github.com/fable-compiler/Fable)).

## Interfaces

The library exposes  3 interfaces:

1) a direct (`raw`) interface to the hafas endpoints,
2) a F# async based interface corresponding to hafas-client api,
3) a JS promise based interface corresponding to the TS [Type definitions](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) for hafas-client.

The library compiles via Fable to a webpack module with [this](src/fshafas.fable.package/fs-hafas-client/fshafas.bundle.d.ts) TS Type definition.

## Compilation to JavaScript

The JS promise based interface compiles via Fable to a JavaScript library, this library and the original hafas-client can be used almost interchangeably.

The following diagram should commute:

JavaScript/TypeScript | Transformation | F# |
:---------------:|:--------:|:------:|
[hafas-client TS types](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) | => [ts2fable](https://github.com/fable-compiler/ts2fable) and [Transformer](./src/transformer) => | hafas-client F# types |
 &#8659;|  | &#8659; implementation |
hafas-client TS types, JS Program | <= [fable](https://github.com/fable-compiler/fable) <= | hafas-client F# types, F# program|

## Building

Requirements are:

* [dotnet SDK 5](https://dotnet.microsoft.com/download)
* [node.js](https://nodejs.org/en/)

Run `./build.sh` or `./build.cmd` at the root folder.

Targets are:

* *BuildLib*: compile to dotnet dll,
* *Test*: compile to dotnet dll and run tests,
* *BuildCSharp*: compile example C# program using the fshafas lib,
* *BuildFableApp*: compile F# program via Fable to JavaScript,
* *BuildNpmPack*: compile lib via fable and webpack to a npm package.

Utility scripts:

* [create-types.sh](./scripts/create-types.sh): keep the F# types in sync with the corresponding TS types
* [create-test-fixtures.sh](./scripts/create-test-fixtures.sh): create test fixtures using the JavaScript hafas-client library.

## Using

* [fshafas.fsx](./scripts/fshafas.fsx): F# Interactive example script,
* [App.fs](src/fshafas.fable/App.fs): F# app running with nodejs,
* [Program.cs](src/fshafas.csharp/Program.cs): C# program using the F# lib,
* [FahrplanApp](https://github.com/bergmannjg/FahrplanApp): Android TypeScript app using the npm package.
