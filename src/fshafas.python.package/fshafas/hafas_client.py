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
from .fable_modules.fs_hafas_api_python.types_hafas_client import (Profile, IndexMap_2, Station, Stop, Location, StationStopLocation, ProductType, JourneysOptions, Journeys, RefreshJourneyOptions, Journey, StopOver, JourneysFromTripOptions, TripOptions, Trip, DeparturesArrivalsOptions,
                                                               TripsWithRealtimeData, Departures, Arrivals, LocationsOptions, StopOptions, Location, NearByOptions, ReachableFromOptions, Duration, BoundingBox, RadarOptions, Radar, TripsByNameOptions, RemarksOptions, Warning, LinesOptions, Line, ServerOptions, ServerInfo)
from .fable_modules.fs_hafas_python.hafas_async_client import (HafasAsyncClient_productsOfMode, HafasAsyncClient)
from .fable_modules.db_vendo_python.db_vendo_async_client import (DbVendoAsyncClient)
from .fable_modules.fs_hafas_python.context import (
    Profile as InternalProfile)
from .fable_modules.fs_hafas_api_python.defaults import (
    LocationsOptions as Default_LocationsOptions, JourneysOptions as Default_JourneysOptions, Location as Default_Location, DeparturesArrivalsOptions as Default_DeparturesArrivalsOptions)

# todo: implement all methods of HafasAsyncClient

def _isDbVendoProfile(profile: Any):
    return profile.endpoint == "https://app.vendo.noncd.db.de"
    
def _checkProfileType(profile: Any):
    # check, if profile is from fshafas.fable_modules.fs_hafas_profiles_python classes
    if _isDbVendoProfile(profile): 
        return
    elif not isinstance(profile, InternalProfile):
        raise TypeError(
            "argument profile: type from fshafas.fable_modules.fs_hafas_profiles_python expected ")

class HafasClient(IDisposable):
    def __init__(self, profile: Profile) -> None:
        _checkProfileType(profile)

        self.profile = profile
        if _isDbVendoProfile(profile):
            self.asyncClient = DbVendoAsyncClient()
        else:
            self.asyncClient = HafasAsyncClient(self.profile)

    async def _journeys(self, _from: Union[str, Station, Stop, Location], _to: Union[str, Station, Stop, Location], opt: Optional[JourneysOptions] = None) -> Journeys:
        def generate() -> Async[Journeys]:
            def bind(_arg1: Journeys) -> Async[Journeys]:
                return singleton.Return(_arg1)

            return singleton.Bind(self.asyncClient.AsyncJourneys(_from, _to, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def journeys(self, _from: Union[str, Station, Stop, Location], _to: Union[str, Station, Stop, Location], opt: Optional[JourneysOptions] = None) -> Journeys:
        """Get journeys between locations."""
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

    async def bestprices(self, _from: str,  _to: str, opt: Optional[JourneysOptions]=None) -> Journeys:
        """Get best prices for journeys."""
        if (opt is not None and not isinstance(opt, JourneysOptions)):
            raise TypeError(
                "argument opt: type JourneysOptions expected")

        if (not _from.isdigit()):
            raise TypeError(
                "argument _from: station id expected")

        if (not _to.isdigit()):
            raise TypeError(
                "argument _to: station id expected")

        def generate() -> Async[Journeys]:
            def bind(_arg1: Journeys) -> Async[Journeys]:
                return singleton.Return(_arg1)

            return singleton.Bind(self.asyncClient.AsyncBestPrices( _from, _to, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def departures(self, name: Union[str, Station, Stop, Location], opt: Optional[DeparturesArrivalsOptions]=None) -> Departures:
        """Query the next departures at a station."""
        if (opt is not None and not isinstance(opt, DeparturesArrivalsOptions)):
            raise TypeError(
                "argument opt: type DeparturesArrivalsOptions expected")

        def generate() -> Async[Departures]:
            def bind(_arg1: Departures) -> Async[Departures]:
                return singleton.Return(_arg1)

            return singleton.Bind(self.asyncClient.AsyncDepartures(name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def arrivals(self, name: Union[str, Station, Stop, Location], opt: Optional[DeparturesArrivalsOptions]=None) -> Arrivals:
        """Query the next arrivals at a station."""
        if (opt is not None and not isinstance(opt, DeparturesArrivalsOptions)):
            raise TypeError(
                "argument opt: type DeparturesArrivalsOptions expected")

        def generate() -> Async[Arrivals]:
            def bind(_arg1: Arrivals) -> Async[Arrivals]:
                return singleton.Return(_arg1)

            return singleton.Bind(self.asyncClient.AsyncArrivals(name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def locations(self, name: str, opt: Optional[LocationsOptions]=None) -> List[StationStopLocation]:
        """Find stations, POIs and addresses."""
        if opt is None:
            opt = Default_LocationsOptions

        if (not isinstance(name, str)):
            raise TypeError("argument name: type string expected")

        if (opt is not None and not isinstance(opt, LocationsOptions)):
            raise TypeError("argument opt: type LocationsOptions expected")

        def generate() -> Async[List[Any]]:
            def bind(_arg1: List[Any]) -> Async[List[Any]]:
                return singleton.Return(_arg1)

            return singleton.Bind(self.asyncClient.AsyncLocations(name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def nearby(self, l: Location, opt: Optional[NearByOptions]=None) -> List[StationStopLocation]:
        """Show stations & POIs around."""
        if (not isinstance(l, Location)):
            raise TypeError("argument l: type Location expected")

        if (opt is not None and not isinstance(opt, NearByOptions)):
            raise TypeError("argument opt: type NearByOptions expected")

        def generate() -> Async[List[Any]]:
            def bind(_arg1: List[Any]) -> Async[List[Any]]:
                return singleton.Return(_arg1)

            return singleton.Bind(self.asyncClient.AsyncNearby(l, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def radar(self, rect: BoundingBox, opt: Optional[RadarOptions]=None) -> Radar:
        """Find all vehicles currently in a certain area."""
        if (not isinstance(rect, BoundingBox)):
            raise TypeError("argument rect: type BoundingBox expected")

        def generate() -> Async[Radar]:
            def bind(_arg1: Radar) -> Async[Radar]:
                return singleton.Return(_arg1)

            return singleton.Bind(self.asyncClient.AsyncRadar(rect, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    async def tripsByName(self, name: str, opt: Optional[TripsByNameOptions]=None) -> TripsWithRealtimeData:
        """ Get all trips matching a name."""
        if (not isinstance(name, str)):
            raise TypeError("argument name: type string expected")

        if (opt is not None and not isinstance(opt, TripsByNameOptions)):
            raise TypeError("argument opt: type TripsByNameOptions expected")

        def generate() -> Async[TripsWithRealtimeData]:
            def bind(_arg1: TripsWithRealtimeData) -> Async[TripsWithRealtimeData]:
                return singleton.Return(_arg1)

            return singleton.Bind(self.asyncClient.AsyncTripsByName(name, opt), bind)

        return await start_as_task(singleton.Delay(generate))

    def productsOfMode(self, profile: Profile, mode: str) -> IndexMap_2[str, bool]:
        _checkProfileType(profile)

        return HafasAsyncClient_productsOfMode(profile, mode)

    def Dispose(self) -> None:
        __: HafasClient= self
        __.asyncClient.Dispose()
