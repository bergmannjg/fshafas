# Hafas Client in F\#

Ths is a port of the JavaScript [hafas-client library](https://github.com/public-transport/hafas-client) to F#,
a client for [HAFAS](https://de.wikipedia.org/wiki/HAFAS) public transport APIs.

The hafas endpoints Db, Bvg and Svv are supported.

The F# library compiles to dotnet and (via [Fable](https://github.com/fable-compiler/Fable)) to JavaScript  and Python.

## Interfaces

The library exposes  3 interfaces:

1) a direct (`raw`) interface to the hafas endpoints,
2) a F# async based interface corresponding to hafas-client api,
3) a JS promise based interface corresponding to the TS [Type definitions](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) for hafas-client.

## Compilation to JavaScript

The JS promise based interface compiles via Fable to a JavaScript library, this library and the original hafas-client can be used almost interchangeably.

The following diagram should commute:

JavaScript/TypeScript | Transformation | F# |
:---------------:|:--------:|:------:|
[hafas-client TS types](https://github.com/DefinitelyTyped/DefinitelyTyped/blob/master/types/hafas-client/index.d.ts) | => [ts2fable](https://github.com/fable-compiler/ts2fable) and [Transformer](./src/transformer) => | hafas-client F# types |
 &#8659;|  | &#8659; implementation |
hafas-client TS types, JS Program | <= [fable](https://github.com/fable-compiler/fable) <= | hafas-client F# types, F# program|

## Compilation to Python

Compilation to Python with **dotnet fable --lang Python** uses fable 4. All currently missing features have a comment **workaround**.

## Documentation

* [docs](https://bergmannjg.github.io/fshafas/) for the fshafas F# library
* [docs](https://bergmannjg.github.io/fshafas/js) for the fshafas JavaScript package
* [docs](https://bergmannjg.github.io/fshafas/py) for the fshafas Python package

## Building

### Requirements

* [dotnet SDK 5](https://dotnet.microsoft.com/download)
* [node.js](https://nodejs.org/en/) for target js
* [python](https://www.python.org/) for target python

Run `./build.sh` or `./build.cmd` at the root folder.

### Targets

* *BuildLib*: compile to dotnet dll,
* *Test*: compile to dotnet dll and run tests,
* *JavaScript*: compile to JavaScript and build npm package
* *Python*: compile to Python and build Python package

### Utility scripts

* [create-types.sh](./scripts/create-types.sh): keep the F# types in sync with the corresponding TS types
* [create-test-fixtures.sh](./scripts/create-test-fixtures.sh): create test fixtures using the JavaScript hafas-client library.

### Packages

There are several packages generated in the build process.

#### NuGet packages

The nuget packages are generated from the [fshafas](src/fshafas) projects and can be used with the dotnet tool.

* fshafas.javascript and fshafas.javascript.profiles nuget packages:
  * the nuget package contains the F# source files and the JavaScrript specific project file
  * *dotnet fable --lang JavaScript* compiles a F# program to a JavaScript program
* fshafas.python and fshafas.python.profiles nuget packages:
  * the nuget package contains the F# source files and the Python specific project file
  * *dotnet fable --lang Python* compiles a F# program to a Python program.

#### npm package

The *fs-hafas-client-x.y.z.tgz* npm package is generated from the [fshafas.javascript.package](src/fshafas.javascript.package) project
and can be used in a JavaScript program.

#### Python package

The *fshafas-x.y.z-py3-none-any.whl* Python package is generated from the [fshafas.python.package](src/fshafas.python.package) project
and can be used in a Python program.

## Using

* [Program.fs](src/examples/cli): F# app running with dotnet, nodejs and Python,
* [program.py](src/examples/fshafas.fable.python/program.py): Python program using the fshafas Python package,
* [notebook.ipynb](src/examples/fshafas.fable.python/notebook.ipynb): Jupyter notebook using the fshafas Python package
* [Program.cs](src/examples/fshafas.csharp/Program.cs): C# program using the F# lib,
* [index.ts](src/examples/fshafas.fable.node/index.ts): TypeScript app using the npm package,
* [FahrplanApp](https://github.com/bergmannjg/FahrplanApp): Android TypeScript app using the npm package,
* [Wep App](src/examples/fshafas.fable.web): a web app using the fshafas javascript bundle.
