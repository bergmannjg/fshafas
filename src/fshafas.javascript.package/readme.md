# FsHafas npm package

The fs-hafas-client npm package is build with *build.sh -t JavaScript*.

## Build steps

* dotnet fable fshafas.fable.fsproj, compiles the file Lib.fs and the F# files from the nuget package to JavaScript,
* webpack --config ./webpack.node.config.js, bundles the JavaScript files,
* npm pack fs-hafas-client, creates a tarball from the npm package [fs-hafas-client](./fs-hafas-client).

## Example

* [index.ts](../examples/fshafas.fable.node/index.ts): TypeScript app using the npm package.
