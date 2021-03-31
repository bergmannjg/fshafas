#!/usr/bin/env bash

if [ ! -d "./scripts" ]; then
    echo "please run from project directory"
    exit 1
fi

if [ "$1" = "lib" ] || [ $# -eq 0 ]; then
    dotnet build src/fshafas/fshafas.fsproj
    status=$?
    if test $status -gt 0; then exit 1; fi
fi

if [ "$1" = "test" ] || [ $# -eq 0 ]; then
    dotnet test src/fshafas.test/fshafastest.fsproj
    status=$?
    if test $status -gt 0; then exit 1; fi
fi

if [ "$1" = "csharp" ] || [ $# -eq 0 ]; then
    dotnet build src/fshafas.csharp/fshafas.csharp.csproj
    status=$?
    if test $status -gt 0; then exit 1; fi
fi

if [ "$1" = "fable" ] || [ $# -eq 0 ]; then
    cd src/fshafas.fable
    npm run build
    status=$?
    cd ../..
    if test $status -gt 0; then exit 1; fi
fi

if [ "$1" = "pack" ]; then
    cd src/fshafas.fable.package
    npm run build
    status=$?
    cd ../..
    if test $status -gt 0; then exit 1; fi
elif [ "$1" = "pack-prod" ] || [ $# -eq 0 ]; then
    cd src/fshafas.fable.package
    npm run build-prod
    status=$?
    cd ../..
    if test $status -gt 0; then exit 1; fi
fi