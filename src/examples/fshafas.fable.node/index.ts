import { fshafas } from 'fs-hafas-client';
import { Journey, Leg } from 'fs-hafas-client/hafas-client';
import { profiles } from 'fs-hafas-profiles';

import createClient from 'hafas-client';
import dbProfile from 'hafas-client/p/db/index.js';

const myArgs = process.argv.slice(2);

const client = myArgs.indexOf("--hafas") > 0 ? createClient(dbProfile, 'agent') : fshafas.createClient(profiles.getProfile('db'));

if (myArgs.indexOf("--debug") > 0) {
    fshafas.setDebug();
}

// retrieve locations
async function locations(name: string) {
    try {
        const result = await client.locations(name, { results: 4 })
        result.forEach(s => {
            console.log(s.type, s.id, s.name);
        });
    } catch (error) {
        console.error(error);
    }
}

// retrieve journeys
async function journeys(from: string, to: string) {
    try {
        const fromStops = parseInt(from) > 0 ? [from] : await client.locations(from, { results: 1 });
        const toStops = parseInt(to) > 0 ? [to] : await client.locations(to, { results: 1 });
        if (fromStops.length > 0 && toStops.length > 0) {
            const result = await client.journeys(fromStops[0], toStops[0], { results: 4 })
            result.journeys?.forEach((j: Journey, nr: number) => {
                j.legs.forEach((l: Leg) => {
                    console.log(nr, l.origin?.name, l.destination?.name, l.departure);
                });
            });
        }
    } catch (error: any) {
        if (error.isHafasError) {
            console.error('hafas error:', error);
        } else {
            console.error('error:', error);
        }
    }
}

switch (myArgs[0]) {
    case 'locations':
        myArgs[1] && locations(myArgs[1]);
        break;
    case 'journeys':
        myArgs[1] && myArgs[2] && journeys(myArgs[1], myArgs[2]);
        break;
    default:
        console.log('unkown argument: ', myArgs[0]);
}
