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

# add new fields
sed -i 's/type: .line.;/type: \x27line\x27;\n    matchId?: string;/' index.d.ts
sed -i 's/interface StopOver {/interface StopOver {\n    additionalStop?: boolean;/' index.d.ts

cp index.d.ts ../../src/fshafas.javascript.package/fs-hafas-client/hafas-client.d.ts
cp index.d.ts ../../src/fshafas.profiles.javascript.package/fs-hafas-profiles/hafas-client.d.ts

mkdir node_modules/@types
mkdir node_modules/@types/hafas-client

mv index.d.ts node_modules/@types/hafas-client/index.d.ts

npx ts2fable node_modules/@types/hafas-client/index.d.ts HafasClientTypes.fs

sed -i '/jsNative/d' HafasClientTypes.fs

dotnet run --project ./Transformer.fsproj FsHafas HafasClientTypes.fs ../../src/fshafas/TypesHafasClient.fs

if [ $? -ne 0 ] 
then 
  exit 1 
fi

dotnet fantomas ../../src/fshafas/TypesHafasClient.fs

sed -i 's/U3<Station, Stop, Location>/StationStopLocation/' ../../src/fshafas/TypesHafasClient.fs
sed -i 's/U2<Station, Stop>/StationStop/' ../../src/fshafas/TypesHafasClient.fs
sed -i 's/U2<Stop, Location>/StopLocation/' ../../src/fshafas/TypesHafasClient.fs
sed -i 's/U3<Hint, Status, Warning>/HintStatusWarning/' ../../src/fshafas/TypesHafasClient.fs

dotnet fantomas ../../src/fshafas/TypesHafasClient.fs

rm -f HafasClientTypes.fs

# import raw hafas-client types
if [ "$1" = "remote" ]; then
    wget -q https://raw.githubusercontent.com/bergmannjg/hafas-client/add-types-in-jsdoc/types-raw-api.ts
else
    cp ../../../forks/hafas-client/types-raw-api.ts .
fi
sed -i '/import/d' types-raw-api.ts
sed -i '/generated/d' types-raw-api.ts
sed -i '/any/d' types-raw-api.ts

npx ts2fable ./types-raw-api.ts RawHafasClientTypes.fs
sed -i 's/float/int/' RawHafasClientTypes.fs

dotnet run --project ./Transformer.fsproj RawHafas RawHafasClientTypes.fs ../../src/fshafas/TypesRawHafasClient.fs
dotnet fantomas ../../src/fshafas/TypesRawHafasClient.fs

rm -f types-raw-api.ts
rm -f RawHafasClientTypes.fs

rm -rf node_modules

cd ../..
