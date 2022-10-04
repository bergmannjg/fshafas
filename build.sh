#!/bin/bash

dotnet tool restore
dotnet paket restore
dotnet fake -s run build.fsx $@