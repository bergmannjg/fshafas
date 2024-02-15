# FsHafas Python package

The fshafas Pathon package is build with *build.sh -t Python*.

## Build steps

* dotnet fable fshafas.fsproj --lang Python, compiles F# files from the nuget package to Python,
* python3.10 -m build, combines the [fshafas](./fshafas) Python module with the compiled files to a Python package.

## Example

* [program.py](../examples/fshafas.fable.python/program.py): Python program using the fshafas Python package.
