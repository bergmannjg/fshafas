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
const sbbProfile = require('hafas-client/p/sbb')
const geolib = require('geolib');

const { fshafas } = require('fs-hafas-client')

const fsclient = fshafas.createClient('db')
const jsclient = createClient(dbProfile, 'agent')
const bvgClient = createClient(bvgProfile, 'agent')
const svvClient = createClient(svvProfile, 'agent')

var myArgs = process.argv.slice(2);

const client = myArgs.indexOf("--fshafas") > 0 ? fsclient : jsclient;

const locations = () => {
    const options = { results: 3, linesOfStops: true };
    console.log(JSON.stringify(options));
    client.locations('Hannover', options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

const journeys = () => {
    const options = { results: 1, stopovers: false, scheduledDays: false, remarks: false };
    console.log(JSON.stringify(options));
    client.journeys({ type: 'stop', id: '8002549' }, '8000261', options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(err =>
            console.error(err));
}

const journeysFromTrip = async () => {
    const hamburgHbf = '8002549'
    const münchenHbf = '8000261'
    const kölnHbf = '8000207'

    const departure = new Date();
    departure.setHours(departure.getHours() - 4);
    const journeysResult = await client.journeys(hamburgHbf, münchenHbf, { departure: departure, results: 1, stopovers: true, transfers: 0 })
    const journey = journeysResult.journeys.find(j => !j.legs[0].canceled);

    const leg = journey.legs.find(l => l.line.product === 'nationalExpress')
    const previousStopovers = leg.stopovers.filter(st => st.departure && new Date(st.departure) < Date.now());

    if (previousStopovers.length > 0) {
        const previousStopover = previousStopovers[previousStopovers.length - 1];
        const options = { stopovers: true };
        console.log(JSON.stringify(options));
        const journeys = await client.journeysFromTrip(leg.tripId, previousStopover, kölnHbf, options);
        console.log(JSON.stringify(journeys));
    }
}

const trip = () => {
    client.journeys('8011160', '8010338', {
        results: 1
    })
        .then(result => {
            const leg = result.journeys[0].legs[0];
            const options = {};
            console.log(JSON.stringify(options));
            client.trip(leg.tripId, leg.line.name, options)
                .then(result => { console.log(JSON.stringify(result)); })
                .catch(console.error);
        })
        .catch(console.error);
}

const departures = () => {
    const options = { duration: 20, linesOfStops: true };
    console.log(JSON.stringify(options));
    client.departures({ type: 'stop', id: '8010338' }, options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

const nearby = () => {
    const options = { distance: 400, linesOfStops: false };
    console.log(JSON.stringify(options));
    client.nearby({
        type: 'location',
        latitude: 54.308438,
        longitude: 13.078028
    }, options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error)
}

const reachableFrom = () => {
    const options = { maxDuration: 10 };
    console.log(JSON.stringify(options));
    client.reachableFrom({
        type: 'location',
        address: 'unused',
        latitude: 54.308438,
        longitude: 13.078028
    }, options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error)
}

const radar = () => {
    const [southwest, northeast] = geolib.getBoundsOfDistance(
        { latitude: 53.553533, longitude: 10.00636 },
        20000
    );

    const options = { results: 20, duration: 600 };
    console.log(JSON.stringify(options));
    client.radar({
        north: northeast.latitude,
        west: southwest.longitude,
        south: southwest.latitude,
        east: northeast.longitude
    }, options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error)
}

const lines = () => {
    const options = { };
    console.log(JSON.stringify(options));
    svvClient.lines('S1', options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

const remarks = () => {
    const options = { };
    console.log(JSON.stringify(options));
    svvClient.remarks(options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}
const serverInfo = () => {
    const options = { };
    console.log(JSON.stringify(options));
    client.serverInfo(options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

switch (myArgs[0]) {
    case 'journeys':
        journeys();
        break;
    case 'journeysFromTrip':
        journeysFromTrip().catch(error => {
            console.error(error.message);
            console.error(error.stack);
        });
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
    "fs-hafas-client": "file:../src/fshafas.fable.package/fs-hafas-client-1.1.0.tgz",
    "google-polyline": "^1.0.3",
    "hafas-client": "^5.24.0",
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

METHODS=('locations' 'journeys' 'journeysFromTrip' 'trip' 'departures' 'nearby' 'reachableFrom' 'radar' 'remarks' 'lines' 'serverInfo')
 
for METHOD in "${METHODS[@]}"; do
  if [ ! -f "${PATH2FIXTURES}/db-${METHOD}-response.json" ]; then
    DEBUG=hafas-client node index.js ${METHOD} &> x.txt

    if [ ${METHOD} = "trip" ]; then
            # cause of journeys request
        	sed -i '1,2d' x.txt 
    fi
    if [ ${METHOD} = "journeysFromTrip" ]; then
            # cause of journeys request
        	sed -i '1,2d' x.txt 
    fi
    head -n 1 x.txt > ${PATH2FIXTURES}/db-${METHOD}-options.json

    head -n 2 x.txt | tail -1 > ${PATH2FIXTURES}/db-${METHOD}-raw-request.json

    head -n 3 x.txt | tail -1 > ${PATH2FIXTURES}/db-${METHOD}-raw-response.json

    head -n 4 x.txt | tail -1 > ${PATH2FIXTURES}/db-${METHOD}-response.json
  else
    echo "skip ${METHOD}"
  fi
done

if [ "$1" = "--fshafas" ]; then
    for METHOD in "${METHODS[@]}"; do
        node index.js ${METHOD} --fshafas
    done
fi