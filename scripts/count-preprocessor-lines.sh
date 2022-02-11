#!/usr/bin/env bash
# count the lines with preprosser symbol FABLE_JS or FABLE_PY defined

if [ ! -d "./scripts" ]; then
    echo "please run from project directory"
    exit 1
fi

if [ $# -eq 0 ] || ([ "$1" != "PY" ] && [ "$1" != "JS" ])
  then
    echo "argument JS|PY expected"
    exit 1
fi

find src/fshafas -name "*.fs"  -not -path "*/Debug/*"  -not -path "*/build/*" \
                 -exec sed -n "/#if FABLE_"$1"/{:a;N;/#e/!ba; p}" {} ';' |\
    grep -v "^#" |\
    wc -l