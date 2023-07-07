from __future__ import annotations
from typing import (
    Optional,
    List,
    Any,
    Tuple,
    TypeVar,
    Tuple,
    Awaitable,
    Union,
    Callable,
    Dict,
)
from statistics import mean
from inspect import signature
from fshafas import StationStopLocation, Station, Stop, Location, BoundingBox


def to_location(r: StationStopLocation) -> Location:
    s = r.fields[0]
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


def to_locations(sx: List[StationStopLocation]) -> List[Location]:
    """
    convert list of Station | Stop | Location to List of Location
    """
    return list(map(to_location, sx))


def to_bounding_box(
    sx1: List[StationStopLocation], sx2: List[StationStopLocation]
) -> BoundingBox:
    locs_1 = to_locations(sx1)
    locs_2 = to_locations(sx2)
    if len(locs_1) > 0 and len(locs_2) > 0:
        n = max(locs_1[0].latitude, locs_2[0].latitude)
        s = min(locs_1[0].latitude, locs_2[0].latitude)
        w = min(locs_1[0].longitude, locs_2[0].longitude)
        e = max(locs_1[0].longitude, locs_2[0].longitude)
        return BoundingBox(n, w, s, e)
    else:
        return BoundingBox(52.2735877, 8.0596000, 51.7128598, 8.7404385)


def centroid(movements: list[Movement], rect: BoundingBox):
    if len(movements) > 0:
        _x = mean([m.location.latitude for m in movements])
        _y = mean([m.location.longitude for m in movements])
        return (_x, _y)
    else:
        _x = mean([rect.north, rect.south])
        _y = mean([rect.west, rect.east])
        return (_x, _y)
