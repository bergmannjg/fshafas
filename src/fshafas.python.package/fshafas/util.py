from __future__ import annotations
from typing import (Optional, List, Any, Tuple, TypeVar,
                    Tuple, Awaitable, Union, Callable, Dict)
import jsonpickle
import json
from functools import reduce
from statistics import mean
from inspect import signature
from .fable_modules.fable_library.array_ import (map)
from .fable_modules.fs_hafas_api_python.types_hafas_client import (Profile, IndexMap_2, Station, Stop, Location, ProductType, JourneysOptions, Journeys, RefreshJourneyOptions, Journey, StopOver, JourneysFromTripOptions, TripOptions, Trip, DeparturesArrivalsOptions, StationStopLocation,
                                                               Alternative, LocationsOptions, StopOptions, Location, NearByOptions, ReachableFromOptions, Duration, BoundingBox, RadarOptions, Movement, TripsByNameOptions, RemarksOptions, Warning, LinesOptions, Line, ServerOptions, ServerInfo)
from .fable_modules.fs_hafas_api_python.extra_types import (Log_set_Debug_Z1FBCCD16)

def enable_logging() -> Any:
    Log_set_Debug_Z1FBCCD16(True)
