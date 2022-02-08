#!/usr/bin/env bash
# rebuild python

rm -rf fable_modules/

dotnet fable-py cli.fable.python.fsproj 
