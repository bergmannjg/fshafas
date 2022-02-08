# Hafas Client in F\#

Ths is a port of the JavaScript [hafas-client library](https://github.com/public-transport/hafas-client) to F#.

The hafas endpoints Db, Bvg and Svv are supported.

The F# library compiles to dotnet and (via [Fable](https://github.com/fable-compiler/Fable)) to JavaScript  and Python.

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

## Compilation to Python

Compilation to Python with **dotnet fable-py** is an alpha phase using version 4.0.0-alpha-032. All currently missing features have a comment **workaround**.

Current status when running *src/examples/cli/Program.fs* with fable and fable-py (task *build.sh -t CompareJsPyResult*):

|Interface method|Status|
|---|---|
|locations|ok|
|stop|ok|
|journeys|ok|
|journeysfromtrip|ok|
|departures|ok|
|trips|ok|
|nearby|ok|
|reachablefrom|ok|
|radar|ok|

## Building

Requirements are:

* [dotnet SDK 5](https://dotnet.microsoft.com/download)
* [node.js](https://nodejs.org/en/) for target js
* [python](https://www.python.org/) for target python

Run `./build.sh` or `./build.cmd` at the root folder.

Targets are:

* *BuildLib*: compile to dotnet dll,
* *PublishToLocalFeed*: publish nuget package to local feed, the package can be used in Fable,
* *Test*: compile to dotnet dll and run tests,
* *BuildFableWebpackNode*: compile lib via fable and webpack to a npm package,
* *BuildFableWebpackWeb*: compile lib via fable and webpack to a web target,
* *Python*: compile to Python and publish nuget package and python package to local feeds

Utility scripts:

* [create-types.sh](./scripts/create-types.sh): keep the F# types in sync with the corresponding TS types
* [create-test-fixtures.sh](./scripts/create-test-fixtures.sh): create test fixtures using the JavaScript hafas-client library.

## Using

* [fshafas.fsx](./scripts/fshafas.fsx): F# Interactive example script,
* [Program.fs](src/examples/cli): F# app running with dotnet, nodejs and python,
* [program.py](src/examples/fshafas.fable.python/program.py): python program using the fshafas package,
* [notebook.ipynb](src/examples/fshafas.fable.python/notebook.ipynb): jupyter notebook using the fshafas package
* [Program.cs](src/examples/fshafas.csharp/Program.cs): C# program using the F# lib,
* [FahrplanApp](https://github.com/bergmannjg/FahrplanApp): Android TypeScript app using the npm package,
* [Wep App](src/examples/fshafas.fable.web): a web app using the fshafas javascript bundle.
