# FsHafas npm package

The fs-hafas-client npm package is build with *build.sh -t BuildFableNpmPack*.

## Build steps

* dotnet fable fshafas.fable.fsproj, compile F# files to JavaScript,
* webpack --config ./webpack.node.config.js, bundle the JavaScript files,
* npm pack fs-hafas-client, create a tarball from the npm package [fs-hafas-client](./fs-hafas-client).

## Example

* [index.ts](../examples/fshafas.fable.node/index.ts): TypeScript app using the npm package.
