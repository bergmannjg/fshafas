# FsHafas npm package

The fs-hafas-profiles npm package is build with *build.sh -t JavaScript*.

## Build steps

* dotnet fable fshafas.fable.fsproj -o build, compiles the file Profiles.fs and the F# files from the nuget package to JavaScript,
* webpack --config ./webpack.node.config.js --mode production, bundles the JavaScript files,
* npm pack fs-hafas-profiles/, creates a tarball from the npm package [fs-hafas-profiles](./fs-hafas-profiles).

## Example

* [index.ts](../examples/fshafas.fable.node/index.ts): TypeScript app using the npm package.
