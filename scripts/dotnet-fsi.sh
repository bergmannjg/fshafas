#!/bin/bash

if [ ! -d "./scripts" ]; then
    echo "please run from project directory"
    exit 1
fi

dotnet fsi --fsi-server-input-codepage:28591 --use:scripts/Prelude.fsx
