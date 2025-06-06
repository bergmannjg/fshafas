import type { HafasClient, Journey, Leg, Trip, ScheduledDays, TripWithRealtimeData } from 'fs-hafas-client/hafas-client.js';
import { fshafas } from 'fs-hafas-client';
import { profiles } from 'fs-hafas-profiles';
import { createClient as hafasClient } from 'hafas-client';
import { profile as oebbProfile } from 'hafas-client/p/oebb/index.js';
import { createClient as dbVendoClient } from 'db-vendo-client';
import { profile as dbnavProfile } from 'db-vendo-client/p/dbnav/index.js'

const myArgs = process.argv.slice(2);

const client: HafasClient = myArgs.indexOf("--hafas") > 0 ? hafasClient(oebbProfile, 'agent')
    : myArgs.indexOf("--dbvendo") > 0 ? dbVendoClient(dbnavProfile, 'agent')
        : fshafas.createClient(profiles.getProfile('db'));

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

// retrieve tripsByName
async function tripsByName(name: string) {
    try {
        if (client.tripsByName) {
            const result = await client.tripsByName(name, {})
            result.trips
                .forEach((t: Trip) => {
                    console.log(t.origin?.name, t.destination?.name, t.line?.name);
                });
        }
    } catch (error) {
        console.error(error);
    }
}

async function trip(id: string) {
    try {
        if (client.trip) {
            const { trip }: TripWithRealtimeData = await client.trip(id, {})
            console.log(trip.origin?.name, trip.destination?.name, trip.line?.name);
        }
    } catch (error) {
        console.error(error);
    }
}

const printScheduledDays = (nr: number, scheduledDays: ScheduledDays) => {
    const keyValues = Object.entries(scheduledDays).filter((kv: [string, boolean], key: number, map: [string, boolean][]) => kv[1]);
    if (keyValues.length > 0) console.log(nr, 'scheduledDays:', keyValues[0][0], keyValues[keyValues.length - 1][0]);
}

// retrieve journeys
async function journeys(from: string, to: string) {
    try {
        const fromStops = parseInt(from) > 0 ? [from] : await client.locations(from, { results: 1 });
        const toStops = parseInt(to) > 0 ? [to] : await client.locations(to, { results: 1 });
        if (fromStops.length > 0 && toStops.length > 0) {
            const result = await client.journeys(fromStops[0], toStops[0], { results: 4, scheduledDays: true, transfers: 0 })
            result.journeys?.forEach((j: Journey, nr: number) => {
                if (j.scheduledDays) {
                    printScheduledDays(nr, j.scheduledDays);
                }
                j.legs.forEach((l: Leg) => {
                    console.log(nr, l.origin?.name, l.destination?.name, l.plannedDeparture, l.line?.name);
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
    case 'tripsByName':
        myArgs[1] && tripsByName(myArgs[1]);
        break;
    case 'trip':
        myArgs[1] && trip(myArgs[1]);
        break;
    case 'journeys':
        myArgs[1] && myArgs[2] && journeys(myArgs[1], myArgs[2]);
        break;
    default:
        console.log('unkown argument: ', myArgs[0]);
}
