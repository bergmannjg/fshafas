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
import { createClient } from 'hafas-client'
import { profile as dbProfile } from 'hafas-client/p/db/index.js'
import { profile as bvgProfile } from 'hafas-client/p/bvg/index.js'
import { profile as mobilnrwProfile } from 'hafas-client/p/mobil-nrw/index.js'
import { profile as oebbProfile } from 'hafas-client/p/oebb/index.js'
import { profile as saarfahrplanProfile } from 'hafas-client/p/saarfahrplan/index.js'
import { profile as rejseplanenProfile } from 'hafas-client/p/rejseplanen/index.js'
import { profile as svvProfile } from 'hafas-client/p/svv/index.js'
import { profile as sncbProfile } from 'hafas-client/p/sncb/index.js'
import geolib from 'geolib'

var myArgs = process.argv.slice(2);

let client = createClient(dbProfile, 'agent');

let journeys_from = { type: 'stop', id: '8002549' };
let journeys_to = '8000107';

switch (myArgs[1]) {
    case 'bvg':
        client = createClient(bvgProfile, 'agent');
        break;
    case 'mobilnrw':
        client = createClient(mobilnrwProfile, 'agent');
        break;
    case 'saarfahrplan':
        client = createClient(saarfahrplanProfile, 'agent');
        journeys_from = { type: 'stop', id: '8000323' };
        journeys_to = '8000189';
        break;
    case 'oebb':
        client = createClient(oebbProfile, 'agent');
        journeys_from = { type: 'stop', id: '1290401' };
        journeys_to = '8100108';
        break;
    case 'rejseplanen':
        client = createClient(rejseplanenProfile, 'agent');
        journeys_from = { type: 'stop', id: '8600626' };
        journeys_to = '8024313';
        break;
    case 'svv':
        client = createClient(svvProfile, 'agent');
        break;
}

const locations = () => {
    const options = { results: 3, linesOfStops: true };
    console.log(JSON.stringify(options));
    client.locations('Hamburg Hbf', options)
        .then(result => { console.log(JSON.stringify(result)); })
        .catch(console.error);
}

const journeys = () => {
    const now = new Date();
    const dt = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 8, 20);
    const options = { results: 2, stopovers: true, polylines: true, scheduledDays: false, remarks: false, departure: dt.toISOString() };
    console.log(JSON.stringify(options));
    client.journeys(journeys_from, journeys_to, options)
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
    "geolib": "^3.3.1",
    "hafas-client": "^5.26.1"
  }
}
EOF

  npm install

else
  cd test-fixtures
fi

PATH2FIXTURES="../src/fshafas.test/fixtures"

METHODS=('db:locations' 'db:journeys' 'oebb:journeys' 'saarfahrplan:journeys' 'rejseplanen:journeys' 'db:journeysFromTrip' 'db:trip' 'db:departures' 'db:nearby' 'db:reachableFrom' 'db:radar' 'svv:remarks' 'svv:lines' 'db:serverInfo')

for PROFILE_METHOD in "${METHODS[@]}"; do
  readarray -d : -t strarr < <(printf '%s' "$PROFILE_METHOD")
  PROFILE=${strarr[0]}
  METHOD=${strarr[1]}

  if [ ! -f "${PATH2FIXTURES}/${PROFILE}-${METHOD}-response.json" ]; then
    DEBUG=hafas-client node index.js ${METHOD} ${PROFILE} &> x.txt

    if [ ${METHOD} = "trip" ]; then
            # cause of journeys request
        	sed -i '1,2d' x.txt 
    fi
    if [ ${METHOD} = "journeysFromTrip" ]; then
            # cause of journeys request
        	sed -i '1,2d' x.txt 
    fi

    if test "$(wc -l < x.txt)" -eq 4
    then
      head -n 1 x.txt > ${PATH2FIXTURES}/${PROFILE}-${METHOD}-options.json

      head -n 2 x.txt | tail -1 > ${PATH2FIXTURES}/${PROFILE}-${METHOD}-raw-request.json

      head -n 3 x.txt | tail -1 > ${PATH2FIXTURES}/${PROFILE}-${METHOD}-raw-response.json

      head -n 4 x.txt | tail -1 > ${PATH2FIXTURES}/${PROFILE}-${METHOD}-response.json
    else
        echo "unexpected output: $( wc -l < x.txt) lines"
        cat x.txt
    fi
  else
    echo "skip ${PROFILE} ${METHOD}"
  fi
done
