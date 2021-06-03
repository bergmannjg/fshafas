// fshafas.setDebug();

const locations = async (profile, name) => {
    const client = fshafas.createClient(profile);
    const result = await client.locations(name, { results: 5 });
    let arr = [];
    result.forEach(x => {
        console.log(x);
        if (x.type === 'stop') {
            arr.push({ name: x.name, id: x.id, longitude: x.location.longitude, latitude: x.location.latitude })
        }
    });
    return arr;
}

const journeys = async (profile, from, to) => {
    const client = fshafas.createClient(profile);
    const locationsFrom = await client.locations(from, { results: 1 });
    const locationsTo = await client.locations(to, { results: 1 });
    const arr = [];
    if (locationsFrom.length > 0 && locationsTo.length > 0) {
        const result = await client.journeys(locationsFrom[0], locationsTo[0], { results: 5 });
        console.log(result);
        result.journeys?.forEach(x => {
            console.log(x);
            if (x.type === 'journey') {
                x.legs.forEach((leg, index) => {
                    arr.push({ nr: (index + 1) + ' von ' + x.legs.length, origin: leg.origin.name, destination: leg.destination.name, plannedDeparture: leg.plannedDeparture })
                });
            }
        });
    }
    return arr;
}

const createUrl = (text, href) => {
    var anchor = document.createElement('a');
    var createAText = document.createTextNode(text);
    anchor.appendChild(createAText);
    anchor.setAttribute('target', '_blank');
    anchor.setAttribute('href', href);
    return anchor;
}

const addRow = (tb, items) => {
    var tr = document.createElement("tr");
    tb.appendChild(tr);
    items.forEach(x => {
        var td = document.createElement("td");
        if (typeof x === 'string') td.textContent = x;
        else td.appendChild(x);
        tr.appendChild(td);
    });
}

function displayElement(nameShown) {
    const matches = document.querySelectorAll("div.row > div");
    matches.forEach(d => {
        if (d.id.endsWith('box')) d.style.display = 'none';
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
}

function getProfile() {
    if (document.getElementById('btnradioDb').checked) return 'db';
    else if (document.getElementById('btnradioBvg').checked) return 'bvg';
    else return 'db';
}

function lookupLocation(inputLocation) {
    const profile = getProfile();
    var elementName = document.getElementById(inputLocation);
    if (elementName.value.length > 0) {
        locations(profile, elementName.value)
            .then(results => {
                var table = document.getElementById("result-locations");
                removeChilds(table);
                results.forEach(x => {
                    addRow(table, [x.name, x.id, createBRouterUrl(x.longitude, x.latitude)])
                });
            })
            .catch(e => {
                console.log('There has been a problem with your locations operation: ' + e.message);
            });
    }
}

function lookupJourneys(inputFrom, inputTo) {
    const profile = getProfile();
    var elementFrom = document.getElementById(inputFrom);
    var elementTo = document.getElementById(inputTo);
    if (elementFrom.value.length > 0 && elementTo.value.length > 0) {
        journeys(profile, elementFrom.value, elementTo.value)
            .then(results => {
                var table = document.getElementById("result-journeys");
                removeChilds(table);
                results.forEach(x => {
                    addRow(table, [x.nr, x.origin, x.destination, (new Date(x.plannedDeparture)).toLocaleTimeString()])
                });
            })
            .catch(e => {
                console.log('There has been a problem with your journeys operation: ' + e.message);
                console.log('Stacktrace: ' + e.stack);
            });;
    }
}
