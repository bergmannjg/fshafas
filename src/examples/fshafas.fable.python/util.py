from __future__ import annotations
from typing import (Optional, List, Any, Tuple, TypeVar,
                    Tuple, Awaitable, Union, Callable, Dict)
from statistics import mean
from fshafas.fable_modules.fable_library.array import (map)
from fshafas.fable_modules.fs_hafas_python.types_hafas_client import (Profile, IndexMap_2, Station, Stop, Location, ProductType, JourneysOptions, Journeys, RefreshJourneyOptions, Journey, StopOver, JourneysFromTripOptions, TripOptions, Trip, DeparturesArrivalsOptions,
                                                               Alternative, LocationsOptions, StopOptions, Location, NearByOptions, ReachableFromOptions, Duration, BoundingBox, RadarOptions, Movement, TripsByNameOptions, RemarksOptions, Warning, LinesOptions, Line, ServerOptions, ServerInfo, Log_Print)
from fshafas.fable_modules.fs_hafas_python.lib.transformations import (Default_Location)

def to_name(s: Union[Station, Stop, Location]) -> str:
    """
    get name of Station | Stop | Location
    """
    if isinstance(s, Location):
        return s.name

    elif isinstance(s, Stop):
        return s.name

    elif isinstance(s, Line):
        return s.name

    else:
        return ""


def to_dicts(l: List[Any], transform: Optional[Callable[[str, Any], Any]] = None, flatten: Optional[Callable[[str, Any], List[Tuple[str, Any]]]] = None, new_entries: Optional[List[Tuple[str, Any]]] = None) -> List[Dict]:
    """
    convert list of objs to list of dicts of obj
    """
    dicts = [t.__dict__.copy() for t in l]
    if transform:
        for dict in dicts:
            for key in dict.keys():
                dict[key] = transform(key, dict[key])

    if flatten:
        for dict in dicts:
            entries = []
            for key in dict.keys():
                entries = entries +  flatten(key, dict[key])
            for entry in entries:
                dict[entry[0]] = entry[1]
                    
    if new_entries:
        for dict in dicts:
            for new_entry in new_entries:
                dict[new_entry[0]] = new_entry[1]

    return dicts


def to_location(s: Union[Station, Stop, Location]) -> Location:
    if isinstance(s, Location):
        return s

    elif isinstance(s, Stop):
        location: Location = s.location
        location.id = s.id
        location.name = s.name
        return location

    else:
        return Default_Location


def to_locations(sx: List[Union[Station, Stop, Location]]) -> List[Location]:
    """
    convert list of Station | Stop | Location to List of Location
    """
    # workaround: error in type of map, should accept Optional
    return map(to_location, sx, None)  # pytype: disable=wrong-arg-types

def to_bounding_box(sx1: List[Union[Station, Stop, Location]], sx2: List[Union[Station, Stop, Location]]) -> BoundingBox:
    locs_1 = to_locations(sx1)
    locs_2 = to_locations(sx2)
    if len(locs_1) > 0 and len(locs_2) > 0:
        n = max(locs_1[0].latitude, locs_2[0].latitude)
        s = min(locs_1[0].latitude, locs_2[0].latitude)
        w = min(locs_1[0].longitude, locs_2[0].longitude)
        e = max(locs_1[0].longitude, locs_2[0].longitude)
        return BoundingBox(n, w, s, e )
    else:
        return BoundingBox(52.2735877, 8.0596000,  51.7128598, 8.7404385)
        
def transform_name(k: str, v):
    if k == "origin" or k == "destination":
        return to_name(v)
    else:
        return v
    

def to_dicts_of_legs(journey: Journey, idx):
    return to_dicts(journey.legs, transform=transform_name, new_entries=[("id", idx), ("price", journey.price.amount if journey.price else 0)])

def transform_name_of_line(k: str, v):
    if k == "line" :
        return to_name(v)
    else:
        return v

def flatten_location(k: str, v):
    if k == "location" and isinstance(v, Location):
        return [("longitude", v.longitude),("latitude", v.latitude)]
    else:
        return []

def centroid(movements : list[Movement], rect: BoundingBox):
    if len(movements)> 0:
        _x = mean([m.location.latitude for m in movements])
        _y = mean([m.location.longitude for m in movements])
        return(_x, _y)
    else: 
        _x = mean([rect.north, rect.south])
        _y = mean([rect.west, rect.east])
        return(_x, _y)
