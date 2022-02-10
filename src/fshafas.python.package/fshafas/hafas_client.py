from __future__ import annotations
from typing import (Optional, List, Any, Tuple, TypeVar,
                    Tuple, Awaitable, Union, Callable, Dict)
from .fable_modules.fable_library.array import (map)
from .fable_modules.fable_library.task import TaskCompletionSource
from .fable_modules.fable_library.async_builder import (singleton, Async)
from .fable_modules.fable_library.util import IDisposable
from .fable_modules.fable_library.async_ import (
    start_with_continuations, start_as_task, default_cancellation_token)
from .fable_modules.fable_library.util import (
    create_atom, ignore, structural_hash, string_hash)
from .fable_modules.fable_library.string import (to_fail, printf, to_console)
from .fable_modules.fs_hafas_python.types_hafas_client import (Profile, IndexMap_2, Station, Stop, Location, ProductType, JourneysOptions, Journeys, RefreshJourneyOptions, Journey, StopOver, JourneysFromTripOptions, TripOptions, Trip, DeparturesArrivalsOptions,
                                                               Alternative, LocationsOptions, StopOptions, Location, NearByOptions, ReachableFromOptions, Duration, BoundingBox, RadarOptions, Movement, TripsByNameOptions, RemarksOptions, Warning, LinesOptions, Line, ServerOptions, ServerInfo, Log_Print)
from .fable_modules.fs_hafas_python.hafas_async_client import (HafasAsyncClient_productsOfMode, HafasAsyncClient, HafasAsyncClient__AsyncLocations, HafasAsyncClient__AsyncJourneys, HafasAsyncClient__AsyncJourneysFromTrip, HafasAsyncClient__AsyncRefreshJourney,
                                                               HafasAsyncClient__AsyncDepartures, HafasAsyncClient__AsyncTripsByName, HafasAsyncClient__AsyncNearby, HafasAsyncClient__AsyncReachableFrom, HafasAsyncClient__AsyncRadar, HafasAsyncClient__AsyncStop, HafasAsyncClient__AsyncLines)
from .fable_modules.fs_hafas_python.context import (
    Profile as Profile_1)
from .fable_modules.fs_hafas_python.lib.transformations import (
    Default_LocationsOptions, Default_JourneysOptions, Default_Location)

# todo: implement all methods of HafasAsyncClient


def checkProfileType(profile: Any):
    # check, if profile is from fshafas.fable_modules.fs_hafas_profiles_python classes
    if not isinstance(profile, Profile_1):
        raise TypeError(
            "argument profile: type from fshafas.fable_modules.fs_hafas_profiles_python expected ")


class HafasClient(IDisposable):
    def __init__(self, profile: Profile) -> None:
        checkProfileType(profile)

        self.profile = profile
        self.asyncClient = HafasAsyncClient(self.profile)  # pytype: disable=wrong-arg-types

    async def locations(self, name: str, opt: Optional[LocationsOptions] = None) -> List[Union[Station, Stop, Location]]:
        if (not isinstance(name, str)):
            raise TypeError("argument name: type string expected")

        if (opt is not None and not isinstance(opt, LocationsOptions)):
            raise TypeError("argument opt: type LocationsOptions expected")

        def generate() -> Async[List[Any]]:
            def bind(_arg1: List[Any]) -> Async[List[Any]]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncLocations(self.asyncClient, name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def _journeys(self, _from: Union[str, Station, Stop, Location], _to: Union[str, Station, Stop, Location], opt: Optional[JourneysOptions] = None) -> Journeys:
        def generate() -> Async[Journeys]:
            def bind(_arg1: Journeys) -> Async[Journeys]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncJourneys(self.asyncClient, _from, _to, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def journeys(self, _from: Union[str, Station, Stop, Location], _to: Union[str, Station, Stop, Location], opt: Optional[JourneysOptions] = None) -> Journeys:
        if (isinstance(_from, str) and not _from.isdigit()):
            from_locs = await self.locations(_from, Default_LocationsOptions)
            if (len(from_locs) > 0 and from_locs[0].type == "stop"):
                _from = from_locs[0].id

        if (isinstance(_to, str) and not _to.isdigit()):
            to_locs = await self.locations(_to, Default_LocationsOptions)
            if (len(to_locs) > 0 and to_locs[0].type == "stop"):
                _to = to_locs[0].id

        return await self._journeys(_from, _to, opt)

    async def radar(self, rect: BoundingBox, opt: Optional[RadarOptions] = None) -> List[Movement]:
        if (not isinstance(rect, BoundingBox)):
            raise TypeError("argument rect: type BoundingBox expected")

        def generate() -> Async[List[Movement]]:
            def bind(_arg1: List[Movement]) -> Async[List[Movement]]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncRadar(self.asyncClient, rect, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    def productsOfMode(self, profile: Profile, mode: str) -> IndexMap_2[str, bool]:
        checkProfileType(profile)

        return HafasAsyncClient_productsOfMode(profile, mode)  # pytype: disable=wrong-arg-types

    def Dispose(self) -> None:
        __: HafasClient = self
        __.asyncClient.Dispose()