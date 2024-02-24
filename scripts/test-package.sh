#!/usr/bin/env bash

if [ ! -d "./scripts" ]; then
    echo "please run from project directory"
    exit 1
fi

if [ ! -d "./playground/" ]; then
    echo "directory playground/ not foudn"
    exit 1
fi

if [ "$1" = "" ]; then 
    echo "usage: test-package.sh <version>"
    exit 1
fi

VERSION=$1

if [ ! -f "src/fshafas.javascript.package/fs-hafas-client-${VERSION}.tgz" ]; then
    echo "file src/fshafas.javascript.package/fs-hafas-client-${VERSION}.tgz not found"
    exit 1
fi

if [ ! -f "src/fshafas.profiles.javascript.package/fs-hafas-profiles-${VERSION}.tgz" ]; then
    echo "file src/fshafas.profiles.javascript.package/fs-hafas-profiles-${VERSION}.tgz not found"
    exit 1
fi

cd playground

rm -rf test-${VERSION}

mkdir test-${VERSION}

cd test-${VERSION}

cat << EOF > index.js
import { fshafas } from 'fs-hafas-client';
import { profiles } from 'fs-hafas-profiles';

const client = fshafas.createClient(profiles.getProfile('db'));

async function locations(name) {
    try {
        const result = await client.locations(name, { results: 4 })
        result.forEach(s => {
            console.log(s.type, s.id, s.name);
        });
    } catch (error) {
        throw Error (error);
    }
}

const myArgs = process.argv.slice(2);

switch (myArgs[0]) {
    case 'locations':
        myArgs[1] && locations(myArgs[1]);
        break;
    default:
        console.log('unkown argument: ', myArgs[0]);
}
EOF

cat << EOF > package.json
{
    "name": "test",
    "version": "1.0.0",
    "description": "",
    "main": "index.js",
    "type": "module",
    "dependencies": {
        "fs-hafas-client": "file:../../src/fshafas.javascript.package/fs-hafas-client-${VERSION}.tgz",
        "fs-hafas-profiles": "file:../../src/fshafas.profiles.javascript.package/fs-hafas-profiles-${VERSION}.tgz"
    },
    "author": "",
    "license": "ISC"
}
EOF

npm install

node index.js locations Hannover
