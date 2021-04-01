#!/usr/bin/env bash

if [ ! -d "./scripts" ]; then
    echo "please run from project directory"
    exit 1
fi

if [ ! -d "./test-fixtures" ]; then
  mkdir test-fixtures
  cd test-fixtures

  cat << EOF > index.js
// created with script 'create-test-fixtures.sh'
const createClient = require('hafas-client')
const dbProfile = require('hafas-client/p/db')
const bvgProfile = require('hafas-client/p/bvg')
const svvProfile = require('hafas-client/p/svv')

const { fshafas } = require('fs-hafas-client')

const fsclient = fshafas.createClient('db')
const jsclient = createClient(dbProfile, 'agent')
const bvgClient = createClient(bvgProfile, 'agent')
const svvClient = createClient(svvProfile, 'agent')

var myArgs = process.argv.slice(2);

const client = myArgs.indexOf("--fshafas") > 0 ? fsclient : jsclient;

const locations = () => {
    client.locations('8010338', { results: 3, linesOfStops: true })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

const journeys = () => {
    client.journeys({ type: 'stop', id: '8011160' }, '8010338', {
        results: 1,
        stopovers: true,
        scheduledDays: true,
        departure: new Date(2021, 2, 29, 13, 40)
    })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

const trip = () => {
    client.journeys('8011160', '8010338', {
        results: 1
    })
        .then(result => {
            const leg = result.journeys[0].legs[0];
            client.trip(leg.tripId, leg.line.name)
                .then(result => { console.log(JSON.stringify(result)); })
                .catch(console.error);
        })
        .catch(console.error);
}

const departures = () => {
    client.departures('8010338', { duration: 20, linesOfStops: true })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

const nearby = () => {
    client.nearby({
        type: 'location',
        latitude: 54.308438,
        longitude: 13.078028
    }, { distance: 400 })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error)
}

const reachableFrom = () => {
    client.reachableFrom({
        type: 'location',
        address: 'unused',
        latitude: 54.308438,
        longitude: 13.078028
    }, {
        maxDuration: 10
    })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error)
}

const radar = () => {
    client.radar({
        north: 54.319438,
        west: 13.077028,
        south: 54.297438,
        east: 13.079028
    }, { results: 5, duration: 300 })
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error)
}

const lines = () => {
    svvClient.lines('S1')
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

const remarks = () => {
    svvClient.remarks()
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}
const serverInfo = () => {
    client.serverInfo()
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

switch (myArgs[0]) {
    case 'journeys':
        journeys();
        break;
    case 'trip':
        trip();
        break;
    case 'departures':
        departures();
        break;
    case 'locations':
        locations();
        break;
    case 'nearby':
        nearby();
        break;
    case 'reachableFrom':
        reachableFrom();
        break;
    case 'radar':
        radar();
        break;
    case 'lines':
        lines();
        break;
    case 'remarks':
        remarks();
        break;
    case 'serverInfo':
        serverInfo();
        break;
    default:
        console.log('unkown argument: ', myArgs[0]);
}
EOF

  cat << EOF > package.json
{
  "name": "test-fixtures",
  "version": "1.0.0",
  "description": "create test-fixtures",
  "main": "index.js",
  "author": "",
  "license": "ISC",
  "dependencies": {
    "fs-hafas-client": "file:../src/fshafas.fable.package/fs-hafas-client-1.0.0.tgz",
    "google-polyline": "^1.0.3",
    "hafas-client": "^5.15.1",
    "isomorphic-fetch": "^2.2.1",
    "md5": "^2.3.0",
    "slugg": "^1.2.1"
  },
  "devDependencies": {
    "@types/hafas-client": "^5.12.0"
  }
}
EOF

  npm install

else
  cd test-fixtures
fi

PATH2FIXTURES="../src/fshafas.test/fixtures"

METHODS=('locations' 'journeys' 'trip' 'departures' 'nearby' 'reachableFrom' 'radar' 'remarks' 'lines' 'serverInfo')
 
for METHOD in "${METHODS[@]}"; do
  if [ ! -f "${PATH2FIXTURES}/db-${METHOD}-response.json" ]; then
    DEBUG=hafas-client node index.js ${METHOD} &> x.txt

    if [ ${METHOD} = "trip" ]; then
            # cause of journeys request
        	sed -i '1,2d' x.txt 
    fi
    head -n 1 x.txt > ${PATH2FIXTURES}/db-${METHOD}-raw-request.json

    head -n 2 x.txt | tail -1 > ${PATH2FIXTURES}/db-${METHOD}-raw-response.json

    head -n 3 x.txt | tail -1 > ${PATH2FIXTURES}/db-${METHOD}-response.json
  else
    echo "skip ${METHOD}"
  fi
done

if [ "$1" = "--fshafas" ]; then
    for METHOD in "${METHODS[@]}"; do
        node index.js ${METHOD} --fshafas
    done
fi