var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
import * as fshafas from "./lib/fshafas.web.bundle.js";
const locations = (profile, name) => __awaiter(void 0, void 0, void 0, function* () {
    const client = fshafas.createClient(fshafas.getProfile(profile));
    const result = yield client.locations(name, { results: 5 });
    let arr = [];
    result.forEach(x => {
        console.log(x);
        if (x.type === 'stop') {
            arr.push({ name: x.name, id: x.id, longitude: x.location.longitude, latitude: x.location.latitude });
        }
    });
    return arr;
});
const journeys = (profile, from, to) => __awaiter(void 0, void 0, void 0, function* () {
    var _a;
    const client = fshafas.createClient(fshafas.getProfile(profile));
    const locationsFrom = yield client.locations(from, { results: 1 });
    const locationsTo = yield client.locations(to, { results: 1 });
    const arr = [];
    if (locationsFrom.length > 0 && locationsTo.length > 0) {
        const result = yield client.journeys(locationsFrom[0], locationsTo[0], { results: 5 });
        console.log(result);
        (_a = result.journeys) === null || _a === void 0 ? void 0 : _a.forEach(x => {
            console.log(x);
            if (x.type === 'journey') {
                x.legs.forEach((leg, index) => {
                    arr.push({ nr: (index + 1) + ' von ' + x.legs.length, origin: leg.origin.name, destination: leg.destination.name, plannedDeparture: new Date(leg.plannedDeparture), plannedArrival: new Date(leg.plannedArrival) });
                });
            }
        });
    }
    return arr;
});
const createUrl = (text, href) => {
    var anchor = document.createElement('a');
    var createAText = document.createTextNode(text);
    anchor.appendChild(createAText);
    anchor.setAttribute('target', '_blank');
    anchor.setAttribute('href', href);
    return anchor;
};
const addRow = (tb, items) => {
    var tr = document.createElement("tr");
    tb.appendChild(tr);
    items.forEach(x => {
        var td = document.createElement("td");
        if (typeof x === 'string')
            td.textContent = x;
        else if (typeof x === 'object')
            td.appendChild(x);
        tr.appendChild(td);
    });
};
export function displayElement(nameShown) {
    const matches = document.querySelectorAll("div.row > div");
    matches.forEach(d => {
        if (d.id.endsWith('box'))
            d.style.display = 'none';
    });
    document.getElementById(nameShown).style.display = 'initial';
}
function removeChilds(element) {
    while (element.firstChild) {
        element.removeChild(element.firstChild);
    }
}
const createBRouterUrl = (lon, lat) => {
    return createUrl(lat + ', ' + lon, 'https://brouter.de/brouter-web/#map=16/' + lat + '/' + lon + '/osm-mapnik-german_style');
};
function getProfile() {
    if (document.getElementById('btnradioDb').checked)
        return 'db';
    else if (document.getElementById('btnradioBvg').checked)
        return 'bvg';
    else
        return 'db';
}
export function lookupLocation(inputLocation) {
    var table = document.getElementById("result-locations");
    removeChilds(table);
    const profile = getProfile();
    var elementName = document.getElementById(inputLocation);
    if (elementName.value.length > 0) {
        locations(profile, elementName.value)
            .then(results => {
            results.forEach(x => {
                addRow(table, [x.name, x.id, createBRouterUrl(x.longitude, x.latitude)]);
            });
        })
            .catch(e => {
            console.log('There has been a problem with your locations operation: ' + e.message);
        });
    }
}
export function lookupJourneys(inputFrom, inputTo) {
    var table = document.getElementById("result-journeys");
    removeChilds(table);
    const profile = getProfile();
    var elementFrom = document.getElementById(inputFrom);
    var elementTo = document.getElementById(inputTo);
    if (elementFrom.value.length > 0 && elementTo.value.length > 0) {
        journeys(profile, elementFrom.value, elementTo.value)
            .then(results => {
            results.forEach(x => {
                addRow(table, [x.nr, x.origin, x.destination, x.plannedDeparture.toLocaleTimeString(), x.plannedArrival.toLocaleTimeString()]);
            });
        })
            .catch(e => {
            console.log('There has been a problem with your journeys operation: ' + e.message);
            console.log('Stacktrace: ' + e.stack);
        });
        ;
    }
}
