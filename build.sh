#!/bin/bash

dotnet tool restore
dotnet fake -s run build.fsx $@