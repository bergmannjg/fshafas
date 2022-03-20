# FsHafas Python package

The fshafas Pathon package is build with *build.sh -t Python*.

## Build steps

* dotnet fable-py fshafas.fsproj, compiles F# files to Python,
* python3 -m build, combines the [fshafas](./fshafas) Python module with the compiled files to a Python package.

## Example

* [program.py](../examples/fshafas.fable.python/program.py): Python program using the fshafas Python package.
