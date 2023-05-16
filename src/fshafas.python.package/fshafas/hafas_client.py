from __future__ import annotations
from typing import (Optional, List, Any, Tuple, TypeVar,
                    Tuple, Awaitable, Union, Callable, Dict)
from .fable_modules.fable_library.array_ import (map)
from .fable_modules.fable_library.task import TaskCompletionSource
from .fable_modules.fable_library.async_builder import (singleton, Async)
from .fable_modules.fable_library.util import IDisposable
from .fable_modules.fable_library.async_ import (
    start_with_continuations, start_as_task, default_cancellation_token)
from .fable_modules.fable_library.util import (
    create_atom, ignore, structural_hash, string_hash)
from .fable_modules.fs_hafas_python.types_hafas_client import (Profile, IndexMap_2, Station, Stop, Location, StationStopLocation, ProductType, JourneysOptions, Journeys, RefreshJourneyOptions, Journey, StopOver, JourneysFromTripOptions, TripOptions, Trip, DeparturesArrivalsOptions,
                                                               TripsWithRealtimeData, Departures, Arrivals, LocationsOptions, StopOptions, Location, NearByOptions, ReachableFromOptions, Duration, BoundingBox, RadarOptions, Radar, TripsByNameOptions, RemarksOptions, Warning, LinesOptions, Line, ServerOptions, ServerInfo)
from .fable_modules.fs_hafas_python.hafas_async_client import (HafasAsyncClient_productsOfMode, HafasAsyncClient, HafasAsyncClient__AsyncLocations, HafasAsyncClient__AsyncJourneys, HafasAsyncClient__AsyncJourneysFromTrip, HafasAsyncClient__AsyncRefreshJourney,
                                                               HafasAsyncClient__AsyncDepartures, HafasAsyncClient__AsyncArrivals, HafasAsyncClient__AsyncTripsByName, HafasAsyncClient__AsyncNearby, HafasAsyncClient__AsyncReachableFrom, HafasAsyncClient__AsyncRadar, HafasAsyncClient__AsyncStop, HafasAsyncClient__AsyncLines)
from .fable_modules.fs_hafas_python.context import (
    Profile as Profile_1)
from .fable_modules.fs_hafas_python.lib.transformations import (
    Default_LocationsOptions, Default_JourneysOptions, Default_Location, Default_DeparturesArrivalsOptions)

# todo: implement all methods of HafasAsyncClient


def _checkProfileType(profile: Any):
    # check, if profile is from fshafas.fable_modules.fs_hafas_profiles_python classes
    if not isinstance(profile, Profile_1):
        raise TypeError(
            "argument profile: type from fshafas.fable_modules.fs_hafas_profiles_python expected ")


class HafasClient(IDisposable):
    def __init__(self, profile: Profile) -> None:
        _checkProfileType(profile)

        self.profile = profile
        self.asyncClient = HafasAsyncClient(self.profile)

    async def _journeys(self, _from: Union[str, Station, Stop, Location], _to: Union[str, Station, Stop, Location], opt: Optional[JourneysOptions] = None) -> Journeys:
        def generate() -> Async[Journeys]:
            def bind(_arg1: Journeys) -> Async[Journeys]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncJourneys(self.asyncClient, _from, _to, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def journeys(self, _from: Union[str, Station, Stop, Location], _to: Union[str, Station, Stop, Location], opt: Optional[JourneysOptions] = None) -> Journeys:
        if opt is None:
            opt = Default_JourneysOptions

        if (isinstance(_from, str) and not _from.isdigit()):
            from_locs= await self.locations(_from, Default_LocationsOptions)
            if (len(from_locs) > 0 and from_locs[0].fields[0].type == "stop"):
                _from= from_locs[0].fields[0].id

        if (isinstance(_to, str) and not _to.isdigit()):
            to_locs= await self.locations(_to, Default_LocationsOptions)
            if (len(to_locs) > 0 and to_locs[0].fields[0].type == "stop"):
                _to= to_locs[0].fields[0].id

        return await self._journeys(_from, _to, opt)

    async def departures(self, name: Union[str, Station, Stop, Location], opt: Optional[DeparturesArrivalsOptions]=None) -> Departures:
        if (opt is not None and not isinstance(opt, DeparturesArrivalsOptions)):
            raise TypeError(
                "argument opt: type DeparturesArrivalsOptions expected")

        def generate() -> Async[Departures]:
            def bind(_arg1: Departures) -> Async[Departures]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncDepartures(self.asyncClient, name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def arrivals(self, name: Union[str, Station, Stop, Location], opt: Optional[DeparturesArrivalsOptions]=None) -> Arrivals:
        if (opt is not None and not isinstance(opt, DeparturesArrivalsOptions)):
            raise TypeError(
                "argument opt: type DeparturesArrivalsOptions expected")

        def generate() -> Async[Arrivals]:
            def bind(_arg1: Arrivals) -> Async[Arrivals]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncArrivals(self.asyncClient, name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def locations(self, name: str, opt: Optional[LocationsOptions]=None) -> List[StationStopLocation]:
        if opt is None:
            opt = Default_LocationsOptions

        if (not isinstance(name, str)):
            raise TypeError("argument name: type string expected")

        if (opt is not None and not isinstance(opt, LocationsOptions)):
            raise TypeError("argument opt: type LocationsOptions expected")

        def generate() -> Async[List[Any]]:
            def bind(_arg1: List[Any]) -> Async[List[Any]]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncLocations(self.asyncClient, name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def nearby(self, l: Location, opt: Optional[NearByOptions]=None) -> List[StationStopLocation]:
        if (not isinstance(l, Location)):
            raise TypeError("argument l: type Location expected")

        if (opt is not None and not isinstance(opt, NearByOptions)):
            raise TypeError("argument opt: type NearByOptions expected")

        def generate() -> Async[List[Any]]:
            def bind(_arg1: List[Any]) -> Async[List[Any]]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncNearby(self.asyncClient, l, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def radar(self, rect: BoundingBox, opt: Optional[RadarOptions]=None) -> Radar:
        if (not isinstance(rect, BoundingBox)):
            raise TypeError("argument rect: type BoundingBox expected")

        def generate() -> Async[Radar]:
            def bind(_arg1: Radar) -> Async[Radar]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncRadar(self.asyncClient, rect, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def tripsByName(self, name: str, opt: Optional[TripsByNameOptions]=None) -> TripsWithRealtimeData:
        if (not isinstance(name, str)):
            raise TypeError("argument name: type string expected")

        if (opt is not None and not isinstance(opt, TripsByNameOptions)):
            raise TypeError("argument opt: type TripsByNameOptions expected")

        def generate() -> Async[TripsWithRealtimeData]:
            def bind(_arg1: TripsWithRealtimeData) -> Async[TripsWithRealtimeData]:
                return singleton.Return(_arg1)

            return singleton.Bind(HafasAsyncClient__AsyncTripsByName(self.asyncClient, name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    def productsOfMode(self, profile: Profile, mode: str) -> IndexMap_2[str, bool]:
        _checkProfileType(profile)

        return HafasAsyncClient_productsOfMode(profile, mode)

    def Dispose(self) -> None:
        __: HafasClient= self
        __.asyncClient.Dispose()
