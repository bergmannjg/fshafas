#!/usr/bin/env bash
# collect profile config from hafas-client

if [ ! -d "./scripts" ]; then
    echo "please run from project directory"
    exit 1
fi

if [ $# -ne 2 ]
  then
    echo "use $0 profile path-to- hafas-client"
    exit 1
fi

PROFILE=$1
PROFILE_LOWERCASE=$(echo ${PROFILE} | tr '[:upper:]' '[:lower:]')
PATH_TO_HAFAS_CLIENT=$2

if [ ! -d ${PATH_TO_HAFAS_CLIENT} ]; then
    echo "directory ${PATH_TO_HAFAS_CLIENT} not found"
    exit 1
fi

if [ ! -d "${PATH_TO_HAFAS_CLIENT}" ]; then
    echo "profile ${PROFILE} not found"
    exit 1
fi

if [ ! -f "${PATH_TO_HAFAS_CLIENT}/products.js" ]; then
    echo "profile ${PROFILE}, products.js not found"
    exit 1
fi

if [ ! -f "${PATH_TO_HAFAS_CLIENT}/base.json" ]; then
    echo "profile ${PROFILE}, base.json not found"
    exit 1
fi

if [ ! -d "src/fshafas.profiles/${PROFILE_LOWERCASE}" ]; then
    cp -r src/fshafas.profiles/bvg/ src/fshafas.profiles/${PROFILE_LOWERCASE}
fi

node scripts/tojson.js ../${PATH_TO_HAFAS_CLIENT}/products.js | \
dotnet fsi scripts/collect-profile-config.fsx --products ${PROFILE} > src/fshafas.profiles/${PROFILE_LOWERCASE}/Products.fs 

dotnet fantomas src/fshafas.profiles/${PROFILE_LOWERCASE}/Products.fs

cat ${PATH_TO_HAFAS_CLIENT}/base.json | \
dotnet fsi scripts/collect-profile-config.fsx --base ${PROFILE} > src/fshafas.profiles/${PROFILE_LOWERCASE}/Request.fs 

dotnet fantomas src/fshafas.profiles/${PROFILE_LOWERCASE}/Request.fs
