from __future__ import annotations
from typing import (Optional, List, Any, Tuple, TypeVar,
                    Tuple, Awaitable, Union, Callable, Dict)
import jsonpickle
import json
from functools import reduce
from statistics import mean
from inspect import signature
from .fable_modules.fable_library.array import (map)
from .fable_modules.fs_hafas_python.types_hafas_client import (Profile, IndexMap_2, Station, Stop, Location, ProductType, JourneysOptions, Journeys, RefreshJourneyOptions, Journey, StopOver, JourneysFromTripOptions, TripOptions, Trip, DeparturesArrivalsOptions,
                                                               Alternative, LocationsOptions, StopOptions, Location, NearByOptions, ReachableFromOptions, Duration, BoundingBox, RadarOptions, Movement, TripsByNameOptions, RemarksOptions, Warning, LinesOptions, Line, ServerOptions, ServerInfo)


def stripNone(data):
    if isinstance(data, dict):
        return {k: stripNone(v) for k, v in data.items() if k is not None and v is not None}
    elif isinstance(data, list):
        return [stripNone(item) for item in data if item is not None]
    elif isinstance(data, tuple):
        return tuple(stripNone(item) for item in data if item is not None)
    elif isinstance(data, set):
        return {stripNone(item) for item in data if item is not None}
    else:
        if hasattr(data, '__dict__'):
            return stripNone(data.__dict__.copy())
        else:
            return data


def simplify(value: Any) -> Any:
    """
    simplify to JSON-able python dicts, strip null values
    """
    return json.loads(json_encode(value, stripNones=True))


def json_encode(value: Any, stripNones: Optional[bool] = True) -> str:
    """
    Return a JSON formatted representation of value, a Python object
    """
    remapped = stripNone(value) if stripNones else value
    return jsonpickle.encode(remapped, unpicklable=False)


def _replace(o: Dict, replacement: Tuple[str, Any]):
    """
    exec replacement in o
    replacement is Tuple[key,replacable]
    replacable may be key, list of keys or callable
    """
    if isinstance(o, Dict):
        key = replacement[0]
        replacable = replacement[1]
        if isinstance(replacable, str):
            o[key] = o[replacable]
        elif isinstance(replacable, List):
            o[key] = reduce(lambda acc, v: acc[v], replacable, o)
        elif isinstance(replacable, Callable):
            sig = signature(replacable)
            if len(sig.parameters) == 0:
                o[key] = replacable()
            elif len(sig.parameters) == 1:
                o[key] = replacable(o)


def replace(o: Union[Dict, List[Dict]], replacements: List[Tuple[str, Any]]) -> Any:
    """
    exec replacements in o, return o
    """
    if isinstance(o, List):
        for d in o:
            for r in replacements:
                _replace(d, r)
    elif isinstance(o, Dict):
        for r in replacements:
            _replace(o, r)

    return o


def to_location(s: Union[Station, Stop, Location]) -> Location:
    if isinstance(s, Location):
        return s

    elif isinstance(s, Stop):
        location: Location = s.location
        location.id = s.id
        location.name = s.name
        return location

    elif isinstance(s, Station):
        location: Location = s.location
        location.id = s.id
        location.name = s.name
        return location

    else:
        return None


def to_locations(sx: List[Union[Station, Stop, Location]]) -> List[Location]:
    """
    convert list of Station | Stop | Location to List of Location
    """
    # workaround: error in type of map, should accept Optional
    return map(to_location, sx, None)  # pytype: disable=wrong-arg-types


def to_name(s: Union[Station, Stop, Location, Line]) -> str:
    """
    get name of Station | Stop | Location | Line
    """
    if isinstance(s, Location):
        return s.name

    elif isinstance(s, Stop):
        return s.name

    elif isinstance(s, Station):
        return s.name

    elif isinstance(s, Line):
        return s.name

    else:
        return ""


def to_bounding_box(sx1: List[Union[Station, Stop, Location]], sx2: List[Union[Station, Stop, Location]]) -> BoundingBox:
    locs_1 = to_locations(sx1)
    locs_2 = to_locations(sx2)
    if len(locs_1) > 0 and len(locs_2) > 0:
        n = max(locs_1[0].latitude, locs_2[0].latitude)
        s = min(locs_1[0].latitude, locs_2[0].latitude)
        w = min(locs_1[0].longitude, locs_2[0].longitude)
        e = max(locs_1[0].longitude, locs_2[0].longitude)
        return BoundingBox(n, w, s, e)
    else:
        return BoundingBox(52.2735877, 8.0596000,  51.7128598, 8.7404385)


def centroid(movements: list[Movement], rect: BoundingBox):
    if len(movements) > 0:
        _x = mean([m.location.latitude for m in movements])
        _y = mean([m.location.longitude for m in movements])
        return(_x, _y)
    else:
        _x = mean([rect.north, rect.south])
        _y = mean([rect.west, rect.east])
        return(_x, _y)
