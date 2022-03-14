#!/usr/bin/env bash
# create f# types from typescript types

if [ ! -d "./scripts" ]; then
    echo "please run from project directory"
    exit 1
fi

if [ ! -d "./src/transformer" ]; then
    echo "cannot access transformer directory"
    exit 1
fi

cd src/transformer

rm -rf node_modules
rm -f HafasClientTypes.fs
rm -f RawHafasClientTypes.fs

mkdir node_modules

# import hafas-client types
if [ "$1" = "remote" ]; then
    wget -q https://raw.githubusercontent.com/DefinitelyTyped/DefinitelyTyped/master/types/hafas-client/index.d.ts
else
    cp ../../../forks/hafas-client/index.d.ts .
fi
cp index.d.ts ../../src/fshafas.fable.package/fs-hafas-client/hafas-client.d.ts
mkdir node_modules/@types
mkdir node_modules/@types/hafas-client

sed -i '/CreateClient.IExports/d' index.d.ts
sed -i 's/: Array/: ReadonlyArray/' index.d.ts
mv index.d.ts node_modules/@types/hafas-client/index.d.ts

npx ts2fable node_modules/@types/hafas-client/index.d.ts HafasClientTypes.fs
sed -i '/CreateClient.IExports/d' HafasClientTypes.fs
sed -i 's/U2<string, float>/string/' HafasClientTypes.fs

# TRANSFORMER_DEBUG=1
dotnet run --project ./Transformer.fsproj FsHafas HafasClientTypes.fs ../../src/fshafas/TypesHafasClient.fs

rm -f HafasClientTypes.fs

# import raw hafas-client types
if [ "$1" = "remote" ]; then
    wget -q https://raw.githubusercontent.com/bergmannjg/hafas-client/add-types-in-jsdoc/types-raw-api.ts
else
    cp ../../../forks/hafas-client/types-raw-api.ts .
fi
sed -i '/createClient/d' types-raw-api.ts
sed -i '/any/d' types-raw-api.ts

npx ts2fable ./types-raw-api.ts RawHafasClientTypes.fs
sed -i 's/float/int/' RawHafasClientTypes.fs

# TRANSFORMER_DEBUG=1
dotnet run --project ./Transformer.fsproj RawHafas RawHafasClientTypes.fs ../../src/fshafas/TypesRawHafasClient.fs

rm -f types-raw-api.ts
rm -f RawHafasClientTypes.fs

rm -rf node_modules

cd ../..
