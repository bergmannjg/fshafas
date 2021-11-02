import * as fshafas from "./lib/fshafas.web.bundle.js";

// fshafas.setDebug();

interface LocationResult {
    name: string;
    id: string;
    longitude: number;
    latitude: number;
}

const locations = async (profile: 'db' | 'bvg', name: string) => {
    const client = fshafas.createClient(fshafas.getProfile(profile));
    const result = await client.locations(name, { results: 5 });
    let arr: LocationResult[] = [];
    result.forEach(x => {
        console.log(x);
        if (x.type === 'stop') {
            arr.push({ name: x.name, id: x.id, longitude: x.location.longitude, latitude: x.location.latitude })
        }
    });
    return arr;
}

interface JourneyResult {
    nr: string;
    origin: string;
    destination: string;
    plannedDeparture: Date;
    plannedArrival: Date;
}

const journeys = async (profile: 'db' | 'bvg', from: string, to: string) => {
    const client = fshafas.createClient(fshafas.getProfile(profile));
    const locationsFrom = await client.locations(from, { results: 1 });
    const locationsTo = await client.locations(to, { results: 1 });
    const arr: JourneyResult[] = [];
    if (locationsFrom.length > 0 && locationsTo.length > 0) {
        const result = await client.journeys(locationsFrom[0], locationsTo[0], { results: 5 });
        console.log(result);
        result.journeys?.forEach(x => {
            console.log(x);
            if (x.type === 'journey') {
                x.legs.forEach((leg, index) => {
                    arr.push({ nr: (index + 1) + ' von ' + x.legs.length, origin: leg.origin.name, destination: leg.destination.name, plannedDeparture: new Date(leg.plannedDeparture), plannedArrival: new Date(leg.plannedArrival) })
                });
            }
        });
    }
    return arr;
}

const createUrl = (text: string, href: string) => {
    var anchor = document.createElement('a');
    var createAText = document.createTextNode(text);
    anchor.appendChild(createAText);
    anchor.setAttribute('target', '_blank');
    anchor.setAttribute('href', href);
    return anchor;
}

const addRow = (tb: HTMLElement, items: (string | HTMLElement)[]) => {
    var tr = document.createElement("tr");
    tb.appendChild(tr);
    items.forEach(x => {
        var td = document.createElement("td");
        if (typeof x === 'string') td.textContent = x;
        else if (typeof x === 'object') td.appendChild(x);
        tr.appendChild(td);
    });
}

export function displayElement(nameShown: string) {
    const matches = document.querySelectorAll("div.row > div");
    matches.forEach(d => {
        if (d.id.endsWith('box')) (d as HTMLElement).style.display = 'none';
    });
    document.getElementById(nameShown).style.display = 'initial';
}

function removeChilds(element: HTMLElement) {
    while (element.firstChild) {
        element.removeChild(element.firstChild);
    }
}

const createBRouterUrl = (lon: number, lat: number) => {
    return createUrl(lat + ', ' + lon, 'https://brouter.de/brouter-web/#map=16/' + lat + '/' + lon + '/osm-mapnik-german_style');
}

function getProfile() {
    if ((document.getElementById('btnradioDb') as HTMLInputElement).checked) return 'db';
    else if ((document.getElementById('btnradioBvg') as HTMLInputElement).checked) return 'bvg';
    else return 'db';
}

export function lookupLocation(inputLocation: string) {
    var table = document.getElementById("result-locations");
    removeChilds(table);
    const profile = getProfile();
    var elementName = document.getElementById(inputLocation) as HTMLInputElement;
    if (elementName.value.length > 0) {
        locations(profile, elementName.value)
            .then(results => {
                results.forEach(x => {
                    addRow(table, [x.name, x.id, createBRouterUrl(x.longitude, x.latitude)])
                });
            })
            .catch(e => {
                console.log('There has been a problem with your locations operation: ' + e.message);
            });
    }
}

export function lookupJourneys(inputFrom: string, inputTo: string) {
    var table = document.getElementById("result-journeys");
    removeChilds(table);
    const profile = getProfile();
    var elementFrom = document.getElementById(inputFrom) as HTMLInputElement;
    var elementTo = document.getElementById(inputTo) as HTMLInputElement;
    if (elementFrom.value.length > 0 && elementTo.value.length > 0) {
        journeys(profile, elementFrom.value, elementTo.value)
            .then(results => {
                results.forEach(x => {
                    addRow(table, [x.nr, x.origin, x.destination, x.plannedDeparture.toLocaleTimeString(), x.plannedArrival.toLocaleTimeString()])
                });
            })
            .catch(e => {
                console.log('There has been a problem with your journeys operation: ' + e.message);
                console.log('Stacktrace: ' + e.stack);
            });;
    }
}
