from __future__ import annotations
from collections.abc import Callable
import sys
from typing import (Any, TypeVar)
from .arguments import (value_to_arg, flag_to_arg, value2to_arg, value3to_arg, value4to_arg)
from .target_javascript.fable_modules.fable_library.array_ import (iterate, try_pick, sort_by as sort_by_1, filter as filter_1, map, choose as choose_2)
from .target_javascript.fable_modules.fable_library.async_ import catch_async
from .target_javascript.fable_modules.fable_library.async_builder import (singleton, Async)
from .target_javascript.fable_modules.fable_library.date import (add_days as add_days_1, now, create as create_1, year, month, day, hour, add_hours)
from .target_javascript.fable_modules.fable_library.double import parse as parse_1
from .target_javascript.fable_modules.fable_library.int32 import parse as parse_2
from .target_javascript.fable_modules.fable_library.list import (choose as choose_1, of_array, FSharpList, fold, iterate as iterate_1)
from .target_javascript.fable_modules.fable_library.option import (or_else_with, value as value_1, default_arg)
from .target_javascript.fable_modules.fable_library.reflection import (TypeInfo, string_type, float64_type, union_type, int32_type)
from .target_javascript.fable_modules.fable_library.reg_exp import (is_match, create)
from .target_javascript.fable_modules.fable_library.seq2 import (Array_distinct, Array_groupBy)
from .target_javascript.fable_modules.fable_library.string_ import (to_fail, printf, to_console, split, substring, join)
from .target_javascript.fable_modules.fable_library.types import (Array, Union)
from .target_javascript.fable_modules.fable_library.util import (create_atom, ignore, compare, string_hash, equal_arrays, array_hash)
from .target_javascript.fable_modules.fs_hafas_java_script.context import (Profile_reflection, Profile)
from .target_javascript.fable_modules.fs_hafas_java_script.extra_types import (IndexMap_2__ctor_2B594, IndexMap_2, HafasError, HafasError__get_code, Log_set_Debug_Z1FBCCD16)
from .target_javascript.fable_modules.fs_hafas_java_script.hafas_async_client import (HafasAsyncClient_productsOfFilter, HafasAsyncClient_productsOfMode, HafasAsyncClient__AsyncLocations, HafasAsyncClient, HafasAsyncClient__ctor_Z3AB94A1B, HafasAsyncClient__AsyncJourneys, HafasAsyncClient__AsyncBestPrices, HafasAsyncClient__AsyncJourneysFromTrip, HafasAsyncClient__AsyncRefreshJourney, HafasAsyncClient__AsyncDepartures, HafasAsyncClient__AsyncArrivals, HafasAsyncClient__AsyncTrip, HafasAsyncClient__AsyncTripsByName, HafasAsyncClient__AsyncNearby, HafasAsyncClient__AsyncReachableFrom, HafasAsyncClient__AsyncRadar, HafasAsyncClient__AsyncStop, HafasAsyncClient__AsyncRemarks_7D671456, HafasAsyncClient__AsyncLines, HafasAsyncClient__AsyncServerInfo_70DF6D02, HafasAsyncClient_initSerializer)
from .target_javascript.fable_modules.fs_hafas_java_script.lib.transformations import (Default_LocationsOptions, Default_JourneysOptions, Default_Stop, Default_StopOver, Default_JourneysFromTripOptions, Default_Journeys, Default_DeparturesArrivalsOptions, Default_TripsByNameOptions, Default_Location, Default_NearByOptions, Default_ReachableFromOptions, Default_RadarOptions, Default_StopOptions)
from .target_javascript.fable_modules.fs_hafas_java_script.print import (Locations, Journeys, Journey as Journey_1, Alternatives, Trip, Trips, Durations, Movements, U3StationStopLocation, Warnings, Lines)
from .target_javascript.fable_modules.fs_hafas_java_script.types_hafas_client import (ProductType, StationStopLocation, JourneysOptions, LoyaltyCard, Journeys as Journeys_1, Journey, Price, Stop, StopOver, JourneysFromTripOptions, JourneyWithRealtimeData, DeparturesArrivalsOptions, StationStop, Alternative, Departures, Arrivals, TripWithRealtimeData, TripsByNameOptions, TripsWithRealtimeData, Line, Trip as Trip_1, Location, NearByOptions, ReachableFromOptions, DurationsWithRealtimeData, BoundingBox, RadarOptions, Radar, StopOptions, WarningsWithRealtimeData, LinesWithRealtimeData, ServerInfo)
from .target_javascript.fable_modules.fs_hafas_profiles_java_script.bvg.profile import profile as profile_4
from .target_javascript.fable_modules.fs_hafas_profiles_java_script.db.profile import profile as profile_3
from .target_javascript.fable_modules.fs_hafas_profiles_java_script.mobilnrw.profile import profile as profile_5
from .target_javascript.fable_modules.fs_hafas_profiles_java_script.oebb.profile import profile as profile_6
from .target_javascript.fable_modules.fs_hafas_profiles_java_script.rejseplanen.profile import profile as profile_7
from .target_javascript.fable_modules.fs_hafas_profiles_java_script.saarfahrplan.profile import profile as profile_8
from .target_javascript.fable_modules.fs_hafas_profiles_java_script.svv.profile import profile as profile_9

_A = TypeVar("_A")

_B = TypeVar("_B")

_B_ = TypeVar("_B_")

def _expr754() -> TypeInfo:
    return union_type("App.CliArguments", [], CliArguments, lambda: [[("name", string_type)], [("id", string_type)], [("token", string_type)], [("from", string_type), ("to", string_type)], [("from", string_type), ("to", string_type)], [("from", string_type), ("to", string_type), ("options", string_type)], [("tripId", string_type), ("stopover", string_type), ("departure", string_type), ("newToId", string_type)], [("name", string_type)], [("name", string_type)], [("name", string_type)], [("name", string_type)], [("lon", float64_type), ("lat", float64_type)], [("lon", float64_type), ("lat", float64_type)], [("north", float64_type), ("west", float64_type), ("south", float64_type), ("east", float64_type)], [("name", string_type)], [], [("Item", Profile_reflection())], [], []])


class CliArguments(Union):
    def __init__(self, tag: int, *fields: Any) -> None:
        super().__init__()
        self.tag: int = tag or 0
        self.fields: Array[Any] = list(fields)

    @staticmethod
    def cases() -> list[str]:
        return ["Locations", "Stop", "RefreshJourney", "Journeys", "BestPrices", "JourneysWithOptions", "JourneysFromTrip", "Arrivals", "Departures", "Trip", "TripsByName", "Nearby", "ReachableFrom", "Radar", "Lines", "ServerInfo", "Profile", "Debug", "Help"]


CliArguments_reflection = _expr754

def print_help(__unit: None=None) -> str:
    return "\nUSAGE: cli.exe [--help] [--locations <name>] [--stop <id>] [--journeys <from> <to>]\n               [--journeysfromtrip <fromId> <toId> <newToId>]\n               [--departures <name>] [--tripsbyname <name>] [--nearby <lon> <lat>] [--reachablefrom <lon> <lat>]\n               [--radar <north> <west> <south> <east>] [--lines <name>] [--serverinfo] [--profile <db|bvg|svv>] [--debug]\n\nOPTIONS:\n\n    --locations <name>    get locations, e.g. Hannover.\n    --stop <id>           get stop, e.g. 8000152.\n    --journeys <from> <to>\n                          get journeys, e.g. Hannover Berlin.\n    --journeys <from> <to> <options>\n                          get journeys, e.g. Hannover \"Berlin-Spandau\" \"ProductId:national;Transfers:0\".\n    --journeysfromtrip <tripId> <prevStopId> <prevStopDepature> <newToId>\n                          get journeys from  <prevStopId> of trip <tripId> to new target <newToId>.\n    --departures <name>   get Departures, e.g. Hannover.\n    --arrivals <name>     get Arrivals, e.g. Hannover.\n    --trip <tripId>       get Trip for <tripId>.\n    --tripsbyname <name>  get Trips, e.g. ICE1001.\n    --nearby <lon> <lat>  get Nearby, e.g. 13.078028 54.308438.\n    --reachablefrom <lon> <lat>\n                          get ReachableFrom, e.g. 13.078028 54.308438.\n    --radar <north> <west> <south> <east>\n                          get Radar, e.g. 52.039421 8.522777 52.019421 8.542777.\n    --lines <name>        get Lines, e.g. S1, profile svv.\n    --serverinfo          get ServerInfo.\n    --profile <db|bvg|svv>\n                          set Profile.\n    --debug               show debug msgs.\n    --help                display this list of options.\n"


def to_profile(s: str) -> Profile:
    if s == "db":
        return profile_3

    elif s == "bvg":
        return profile_4

    elif s == "mobilnrw":
        return profile_5

    elif s == "oebb":
        return profile_6

    elif s == "rejseplanen":
        return profile_7

    elif s == "saarfahrplan":
        return profile_8

    elif s == "svv":
        return profile_9

    else: 
        return to_fail(printf("%s is out of range"))(s)



def parse(args: FSharpList[str]) -> FSharpList[CliArguments]:
    def chooser(x: CliArguments | None=None, args: Any=args) -> CliArguments | None:
        return x

    def _arrow756(arg: str, args: Any=args) -> CliArguments:
        return CliArguments(16, to_profile(arg))

    def _arrow757(name: str, args: Any=args) -> CliArguments:
        return CliArguments(0, name)

    def _arrow758(token: str, args: Any=args) -> CliArguments:
        return CliArguments(2, token)

    def _arrow759(id: str, args: Any=args) -> CliArguments:
        return CliArguments(1, id)

    def _arrow760(tupled_arg: tuple[str, str], args: Any=args) -> CliArguments:
        return CliArguments(4, tupled_arg[0], tupled_arg[1])

    def _arrow761(tupled_arg_1: tuple[str, str, str], args: Any=args) -> CliArguments:
        return CliArguments(5, tupled_arg_1[0], tupled_arg_1[1], tupled_arg_1[2])

    def if_none_thunk(__unit: None=None, args: Any=args) -> CliArguments | None:
        def _arrow762(tupled_arg_2: tuple[str, str]) -> CliArguments:
            return CliArguments(3, tupled_arg_2[0], tupled_arg_2[1])

        return value2to_arg("--journeys", _arrow762, args)

    def _arrow763(tupled_arg_3: tuple[str, str, str, str], args: Any=args) -> CliArguments:
        return CliArguments(6, tupled_arg_3[0], tupled_arg_3[1], tupled_arg_3[2], tupled_arg_3[3])

    def _arrow764(name_1: str, args: Any=args) -> CliArguments:
        return CliArguments(7, name_1)

    def _arrow765(name_2: str, args: Any=args) -> CliArguments:
        return CliArguments(8, name_2)

    def _arrow766(name_3: str, args: Any=args) -> CliArguments:
        return CliArguments(9, name_3)

    def _arrow767(name_4: str, args: Any=args) -> CliArguments:
        return CliArguments(10, name_4)

    def _arrow768(tupled_arg_4: tuple[str, str], args: Any=args) -> CliArguments:
        return CliArguments(11, parse_1(tupled_arg_4[0]), parse_1(tupled_arg_4[1]))

    def _arrow769(tupled_arg_5: tuple[str, str], args: Any=args) -> CliArguments:
        return CliArguments(12, parse_1(tupled_arg_5[0]), parse_1(tupled_arg_5[1]))

    def _arrow770(tupled_arg_6: tuple[str, str, str, str], args: Any=args) -> CliArguments:
        return CliArguments(13, parse_1(tupled_arg_6[0]), parse_1(tupled_arg_6[1]), parse_1(tupled_arg_6[2]), parse_1(tupled_arg_6[3]))

    def _arrow771(name_5: str, args: Any=args) -> CliArguments:
        return CliArguments(14, name_5)

    return choose_1(chooser, of_array([value_to_arg("--profile", _arrow756, args), flag_to_arg("--debug", CliArguments(17), args), value_to_arg("--locations", _arrow757, args), value_to_arg("--refreshjourney", _arrow758, args), value_to_arg("--stop", _arrow759, args), value2to_arg("--bestprices", _arrow760, args), or_else_with(value3to_arg("--journeys", _arrow761, args), if_none_thunk), value4to_arg("--journeysfromtrip", _arrow763, args), value_to_arg("--arrivals", _arrow764, args), value_to_arg("--departures", _arrow765, args), value_to_arg("--trip", _arrow766, args), value_to_arg("--tripsbyname", _arrow767, args), value2to_arg("--nearby", _arrow768, args), value2to_arg("--reachablefrom", _arrow769, args), value4to_arg("--radar", _arrow770, args), value_to_arg("--lines", _arrow771, args), flag_to_arg("--serverinfo", CliArguments(15), args), flag_to_arg("--help", CliArguments(18), args)]))


def maybe_array(choose: Callable[[_A], Array[_B]], option: _A | None=None) -> Array[_B]:
    if option is None:
        return []

    else: 
        return choose(value_1(option))



profile: Profile = create_atom(profile_3)

arg: str = profile().locale

to_console(printf("%s"))(arg)

def products_of_id(id: str) -> IndexMap_2[str, bool] | None:
    products_1: IndexMap_2[str, bool] = IndexMap_2__ctor_2B594(False)
    def action(p_1: str, id: Any=id) -> None:
        products_1[p_1]=True

    def filter(p: ProductType, id: Any=id) -> bool:
        return p.id == id

    iterate(action, (Object.keys(HafasAsyncClient_productsOfFilter(profile(), filter))))
    return products_1


def products(__unit: None=None) -> IndexMap_2[str, bool] | None:
    products_1: IndexMap_2[str, bool] = HafasAsyncClient_productsOfMode(profile(), "train")
    def action(p: str) -> None:
        products_1[p]=True

    iterate(action, (Object.keys(HafasAsyncClient_productsOfMode(profile(), "bus"))))
    return products_1


def trains(__unit: None=None) -> IndexMap_2[str, bool] | None:
    return HafasAsyncClient_productsOfMode(profile(), "train")


def get_location(client: HafasAsyncClient, name: str) -> Async[Any | None]:
    def _arrow774(__unit: None=None, client: Any=client, name: Any=name) -> Async[Any | None]:
        def _arrow773(_arg: Array[StationStopLocation]) -> Async[Any | None]:
            def chooser(arg: StationStopLocation) -> Any | None:
                def _arrow772(__unit: None=None, arg: Any=arg) -> Any:
                    u3: StationStopLocation = arg
                    return u3 if (u3.type == "stop") else (u3 if (u3.type == "location") else u3)

                return _arrow772()

            return singleton.Return(try_pick(chooser, _arg))

        return singleton.Return(name) if is_match(create("^\\d+$"), name) else singleton.Bind(HafasAsyncClient__AsyncLocations(client, name, Default_LocationsOptions), _arrow773)

    return singleton.Delay(_arrow774)


def AsyncRunCatched(computation: Async[None]) -> Async[None]:
    def _arrow781(__unit: None=None, computation: Any=computation) -> Async[None]:
        def _arrow780(_arg: Any) -> Async[None]:
            if _arg.tag == 1:
                ext: Exception = _arg.fields[0]
                def _expr776():
                    def _arrow775(__unit: None=None) -> None:
                        arg: str = HafasError__get_code(ext)
                        arg_1: str = str(ext)
                        to_console(printf("hafas error: %s %s"))(arg)(arg_1)

                    _arrow775()
                    return singleton.Zero()

                def _expr778():
                    def _arrow777(__unit: None=None) -> None:
                        arg_2: str = str(ext)
                        to_console(printf("error: %s"))(arg_2)

                    _arrow777()
                    return singleton.Zero()

                def _arrow779(__unit: None=None) -> Async[None]:
                    return singleton.Return(None)

                return singleton.Combine(_expr776() if isinstance(ext, HafasError) else _expr778(), singleton.Delay(_arrow779))

            else: 
                return singleton.Return(_arg.fields[0])


        return singleton.Bind(catch_async(computation), _arrow780)

    return singleton.Delay(_arrow781)


def AsyncRun(computation: Async[None]) -> None:
    def fail(error: Exception, computation: Any=computation) -> None:
        if None:
            arg_1: str = str(error)
            to_console(printf("hafas error, code: %s msg: %s"))(None)(arg_1)

        else: 
            arg_2: str = str(error)
            to_console(printf("error: %s"))(arg_2)


    ignore(None.catch(fail))


def locations(name: str) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow783(__unit: None=None, name: Any=name) -> Async[None]:
            def _arrow782(_arg: Array[StationStopLocation]) -> Async[None]:
                arg: str = Locations(_arg)
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncLocations(client, name, Default_LocationsOptions), _arrow782)

        return AsyncRun(singleton.Delay(_arrow783))


def _expr784() -> TypeInfo:
    return union_type("App.JourneyOption", [], JourneyOption, lambda: [[("id", string_type)], [("nr", int32_type)], [("discount", int32_type), ("class", int32_type)], [("age", int32_type)]])


class JourneyOption(Union):
    def __init__(self, tag: int, *fields: Any) -> None:
        super().__init__()
        self.tag: int = tag or 0
        self.fields: Array[Any] = list(fields)

    @staticmethod
    def cases() -> list[str]:
        return ["Id", "Transfers", "Bahncard", "Age"]


JourneyOption_reflection = _expr784

def parse_journey_options(args: FSharpList[str]) -> FSharpList[JourneyOption]:
    def chooser(x: JourneyOption | None=None, args: Any=args) -> JourneyOption | None:
        return x

    def _arrow786(id: str, args: Any=args) -> JourneyOption:
        return JourneyOption(0, id)

    def _arrow787(nr: str, args: Any=args) -> JourneyOption:
        return JourneyOption(1, parse_2(nr, 511, False, 32))

    def _arrow788(tupled_arg: tuple[str, str], args: Any=args) -> JourneyOption:
        return JourneyOption(2, parse_2(tupled_arg[0], 511, False, 32), parse_2(tupled_arg[1], 511, False, 32))

    def _arrow789(age: str, args: Any=args) -> JourneyOption:
        return JourneyOption(3, parse_2(age, 511, False, 32))

    return choose_1(chooser, of_array([value_to_arg("ProductId", _arrow786, args), value_to_arg("Transfers", _arrow787, args), value2to_arg("Bahncard", _arrow788, args), value_to_arg("Age", _arrow789, args)]))


def apply_journey_option(option: JourneyOption, journeys_options: JourneysOptions) -> JourneysOptions:
    if option.tag == 1:
        return JourneysOptions(journeys_options.departure, journeys_options.arrival, journeys_options.earlier_than, journeys_options.later_than, journeys_options.results, journeys_options.via, journeys_options.stopovers, option.fields[0], journeys_options.transfer_time, journeys_options.accessibility, journeys_options.bike, journeys_options.products, journeys_options.tickets, journeys_options.polylines, journeys_options.sub_stops, journeys_options.entrances, journeys_options.remarks, journeys_options.walking_speed, journeys_options.start_with_walking, journeys_options.language, journeys_options.scheduled_days, journeys_options.when, journeys_options.first_class, journeys_options.age_group, journeys_options.age, journeys_options.loyalty_card, journeys_options.routing_mode)

    elif option.tag == 2:
        return JourneysOptions(journeys_options.departure, journeys_options.arrival, journeys_options.earlier_than, journeys_options.later_than, journeys_options.results, journeys_options.via, journeys_options.stopovers, journeys_options.transfers, journeys_options.transfer_time, journeys_options.accessibility, journeys_options.bike, journeys_options.products, journeys_options.tickets, journeys_options.polylines, journeys_options.sub_stops, journeys_options.entrances, journeys_options.remarks, journeys_options.walking_speed, journeys_options.start_with_walking, journeys_options.language, journeys_options.scheduled_days, journeys_options.when, journeys_options.first_class, journeys_options.age_group, journeys_options.age, LoyaltyCard("Bahncard", option.fields[0], option.fields[1]), journeys_options.routing_mode)

    elif option.tag == 3:
        return JourneysOptions(journeys_options.departure, journeys_options.arrival, journeys_options.earlier_than, journeys_options.later_than, journeys_options.results, journeys_options.via, journeys_options.stopovers, journeys_options.transfers, journeys_options.transfer_time, journeys_options.accessibility, journeys_options.bike, journeys_options.products, journeys_options.tickets, journeys_options.polylines, journeys_options.sub_stops, journeys_options.entrances, journeys_options.remarks, journeys_options.walking_speed, journeys_options.start_with_walking, journeys_options.language, journeys_options.scheduled_days, journeys_options.when, journeys_options.first_class, journeys_options.age_group, option.fields[0], journeys_options.loyalty_card, journeys_options.routing_mode)

    else: 
        return JourneysOptions(journeys_options.departure, journeys_options.arrival, journeys_options.earlier_than, journeys_options.later_than, journeys_options.results, journeys_options.via, journeys_options.stopovers, journeys_options.transfers, journeys_options.transfer_time, journeys_options.accessibility, journeys_options.bike, products_of_id(option.fields[0]), journeys_options.tickets, journeys_options.polylines, journeys_options.sub_stops, journeys_options.entrances, journeys_options.remarks, journeys_options.walking_speed, journeys_options.start_with_walking, journeys_options.language, journeys_options.scheduled_days, journeys_options.when, journeys_options.first_class, journeys_options.age_group, journeys_options.age, journeys_options.loyalty_card, journeys_options.routing_mode)



def apply_journey_options(options: FSharpList[JourneyOption]) -> JourneysOptions:
    def folder(s: JourneysOptions, o: JourneyOption, options: Any=options) -> JourneysOptions:
        return apply_journey_option(o, s)

    return fold(folder, Default_JourneysOptions, options)


def get_journey_options(options: str) -> JourneysOptions:
    l: Array[str] = split(options, [":", ";"])
    if len(l) > 0:
        return apply_journey_options(parse_journey_options(of_array(l)))

    else: 
        return Default_JourneysOptions



def journeys(from_: str, to: str, some_options: str | None=None) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        options_1: JourneysOptions = JourneysOptions(Default_JourneysOptions.departure, Default_JourneysOptions.arrival, Default_JourneysOptions.earlier_than, Default_JourneysOptions.later_than, 4, Default_JourneysOptions.via, None, Default_JourneysOptions.transfers, Default_JourneysOptions.transfer_time, Default_JourneysOptions.accessibility, Default_JourneysOptions.bike, products(), Default_JourneysOptions.tickets, True, Default_JourneysOptions.sub_stops, Default_JourneysOptions.entrances, Default_JourneysOptions.remarks, Default_JourneysOptions.walking_speed, Default_JourneysOptions.start_with_walking, Default_JourneysOptions.language, True, Default_JourneysOptions.when, Default_JourneysOptions.first_class, Default_JourneysOptions.age_group, Default_JourneysOptions.age, Default_JourneysOptions.loyalty_card, "REALTIME") if (some_options is None) else get_journey_options(some_options)
        def _arrow793(__unit: None=None, from_: Any=from_, to: Any=to, some_options: Any=some_options) -> Async[None]:
            def _arrow792(_arg: Any | None=None) -> Async[None]:
                from_loc: Any | None = _arg
                def _arrow791(_arg_1: Any | None=None) -> Async[None]:
                    to_loc: Any | None = _arg_1
                    (pattern_matching_result, from_loc_1, to_loc_1) = (None, None, None)
                    if from_loc is not None:
                        if to_loc is not None:
                            pattern_matching_result = 0
                            from_loc_1 = from_loc
                            to_loc_1 = to_loc

                        else: 
                            pattern_matching_result = 1


                    else: 
                        pattern_matching_result = 1

                    if pattern_matching_result == 0:
                        def _arrow790(_arg_2: Journeys_1) -> Async[None]:
                            arg: str = Journeys(_arg_2)
                            to_console(printf("%s"))(arg)
                            return singleton.Zero()

                        return singleton.Bind(HafasAsyncClient__AsyncJourneys(client, from_loc_1, to_loc_1, options_1), _arrow790)

                    elif pattern_matching_result == 1:
                        return singleton.Zero()


                return singleton.Bind(get_location(client, to), _arrow791)

            return singleton.Bind(get_location(client, from_), _arrow792)

        return AsyncRun(singleton.Delay(_arrow793))


def add_days(dt: Any, h: int) -> Any:
    return add_days_1(dt, h)


def best_prices(from_: str, to: str, some_options: str | None=None) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        options: JourneysOptions = JourneysOptions(add_days(now(), 1), Default_JourneysOptions.arrival, Default_JourneysOptions.earlier_than, Default_JourneysOptions.later_than, -1, Default_JourneysOptions.via, None, Default_JourneysOptions.transfers, Default_JourneysOptions.transfer_time, Default_JourneysOptions.accessibility, Default_JourneysOptions.bike, products(), Default_JourneysOptions.tickets, Default_JourneysOptions.polylines, Default_JourneysOptions.sub_stops, Default_JourneysOptions.entrances, Default_JourneysOptions.remarks, Default_JourneysOptions.walking_speed, Default_JourneysOptions.start_with_walking, Default_JourneysOptions.language, Default_JourneysOptions.scheduled_days, Default_JourneysOptions.when, Default_JourneysOptions.first_class, Default_JourneysOptions.age_group, Default_JourneysOptions.age, Default_JourneysOptions.loyalty_card, Default_JourneysOptions.routing_mode)
        def _arrow798(__unit: None=None, from_: Any=from_, to: Any=to, some_options: Any=some_options) -> Async[None]:
            def _arrow797(_arg: Any | None=None) -> Async[None]:
                from_loc: Any | None = _arg
                def _arrow796(_arg_1: Any | None=None) -> Async[None]:
                    to_loc: Any | None = _arg_1
                    (pattern_matching_result, from_loc_1, to_loc_1) = (None, None, None)
                    if from_loc is not None:
                        if to_loc is not None:
                            pattern_matching_result = 0
                            from_loc_1 = from_loc
                            to_loc_1 = to_loc

                        else: 
                            pattern_matching_result = 1


                    else: 
                        pattern_matching_result = 1

                    if pattern_matching_result == 0:
                        def _arrow795(_arg_2: Journeys_1) -> Async[None]:
                            journeys_1: Journeys_1 = _arg_2
                            match_value_1: Array[Journey] | None = journeys_1.journeys
                            if match_value_1 is None:
                                return singleton.Zero()

                            else: 
                                def projection(x_1: Journey) -> Price:
                                    return value_1(x_1.price)

                                def predicate(x: Journey) -> bool:
                                    return x.price is not None

                                class ObjectExpr794:
                                    @property
                                    def Compare(self) -> Callable[[Price, Price], int]:
                                        return compare

                                arg: str = Journeys(Journeys_1(journeys_1.realtime_data_updated_at, journeys_1.earlier_ref, journeys_1.later_ref, sort_by_1(projection, filter_1(predicate, match_value_1), ObjectExpr794())))
                                to_console(printf("%s"))(arg)
                                return singleton.Zero()


                        return singleton.Bind(HafasAsyncClient__AsyncBestPrices(client, from_loc_1, to_loc_1, options), _arrow795)

                    elif pattern_matching_result == 1:
                        return singleton.Zero()


                return singleton.Bind(get_location(client, to), _arrow796)

            return singleton.Bind(get_location(client, from_), _arrow797)

        return AsyncRun(singleton.Delay(_arrow798))


def journeys_from_trip(trip_id: str, stopover: str, departure: str, new_to: str) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile_3) as client:
        def _arrow805(__unit: None=None, trip_id: Any=trip_id, stopover: Any=stopover, departure: Any=departure, new_to: Any=new_to) -> Async[None]:
            def _arrow803(__unit: None=None) -> Async[Journeys_1]:
                def _arrow802(_arg: Any | None=None) -> Async[Journeys_1]:
                    stopover_loc: Any | None = _arg
                    def _arrow801(_arg_1: Any | None=None) -> Async[Journeys_1]:
                        new_to_loc: Any | None = _arg_1
                        def _arrow799(__unit: None=None) -> str | None:
                            id: str = stopover_loc
                            return id

                        def _arrow800(__unit: None=None) -> str | None:
                            stop_1: Stop = stopover_loc
                            return stop_1.id

                        stopover_id: str | None = (_arrow799() if (str(type(stopover_loc)) == "<class \'str\'>") else (_arrow800() if isinstance(stopover_loc, Stop) else None)) if (stopover_loc is not None) else None
                        (pattern_matching_result, new_to_loc_1, stopover_id_1) = (None, None, None)
                        if stopover_id is not None:
                            if new_to_loc is not None:
                                pattern_matching_result = 0
                                new_to_loc_1 = new_to_loc
                                stopover_id_1 = stopover_id

                            else: 
                                pattern_matching_result = 1


                        else: 
                            pattern_matching_result = 1

                        if pattern_matching_result == 0:
                            previous_stopover: StopOver = StopOver(Stop(Default_Stop.type, stopover_id_1, Default_Stop.name, Default_Stop.location, Default_Stop.station, Default_Stop.products, Default_Stop.lines, Default_Stop.is_meta, Default_Stop.reisezentrum_opening_hours, Default_Stop.ids, Default_Stop.load_factor, Default_Stop.entrances, Default_Stop.transit_authority, Default_Stop.distance), departure, Default_StopOver.departure_delay, Default_StopOver.prognosed_departure, Default_StopOver.planned_departure, Default_StopOver.departure_platform, Default_StopOver.prognosed_departure_platform, Default_StopOver.planned_departure_platform, Default_StopOver.arrival, Default_StopOver.arrival_delay, Default_StopOver.prognosed_arrival, Default_StopOver.planned_arrival, Default_StopOver.arrival_platform, Default_StopOver.prognosed_arrival_platform, Default_StopOver.planned_arrival_platform, Default_StopOver.remarks, Default_StopOver.pass_by, Default_StopOver.cancelled, Default_StopOver.departure_prognosis_type, Default_StopOver.arrival_prognosis_type, Default_StopOver.additional)
                            return singleton.ReturnFrom(HafasAsyncClient__AsyncJourneysFromTrip(client, trip_id, previous_stopover, new_to_loc_1, JourneysFromTripOptions(True, Default_JourneysFromTripOptions.transfer_time, Default_JourneysFromTripOptions.accessibility, Default_JourneysFromTripOptions.tickets, Default_JourneysFromTripOptions.polylines, Default_JourneysFromTripOptions.sub_stops, Default_JourneysFromTripOptions.entrances, Default_JourneysFromTripOptions.remarks, Default_JourneysFromTripOptions.products)))

                        elif pattern_matching_result == 1:
                            return singleton.Return(Default_Journeys)


                    return singleton.Bind(get_location(client, new_to), _arrow801)

                return singleton.Bind(get_location(client, stopover), _arrow802)

            def _arrow804(_arg_2: Journeys_1) -> Async[None]:
                arg: str = Journeys(_arg_2)
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(singleton.Delay(_arrow803), _arrow804)

        return AsyncRun(singleton.Delay(_arrow805))


def refresh_journey(refresh_token: str) -> None:
    to_console(printf("refreshJourney: %s"))(refresh_token)
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow807(__unit: None=None, refresh_token: Any=refresh_token) -> Async[None]:
            def _arrow806(_arg: JourneyWithRealtimeData) -> Async[None]:
                arg_1: str = Journey_1(0, _arg.journey)
                to_console(printf("%s"))(arg_1)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncRefreshJourney(client, refresh_token, None), _arrow806)

        return AsyncRun(singleton.Delay(_arrow807))


def date_of_current_hour(__unit: None=None) -> Any:
    dt: Any = now()
    return create_1(year(dt), month(dt), day(dt), hour(dt), 0, 0)


def sort_by(key: Callable[[_A], _B], arr: Array[_A]) -> Array[_A]:
    class ObjectExpr808:
        @property
        def Compare(self) -> Callable[[_B_, _B_], int]:
            return compare

    return sort_by_1(key, arr, ObjectExpr808())


def departures(name: str) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        options: DeparturesArrivalsOptions = DeparturesArrivalsOptions(date_of_current_hour(), Default_DeparturesArrivalsOptions.direction, Default_DeparturesArrivalsOptions.line, Default_DeparturesArrivalsOptions.duration, Default_DeparturesArrivalsOptions.results, Default_DeparturesArrivalsOptions.sub_stops, Default_DeparturesArrivalsOptions.entrances, Default_DeparturesArrivalsOptions.lines_of_stops, Default_DeparturesArrivalsOptions.remarks, Default_DeparturesArrivalsOptions.stopovers, Default_DeparturesArrivalsOptions.include_related_stations, Default_DeparturesArrivalsOptions.products, Default_DeparturesArrivalsOptions.language)
        def _arrow812(__unit: None=None, name: Any=name) -> Async[None]:
            def _arrow811(_arg: Any | None=None) -> Async[None]:
                loc: Any | None = _arg
                if loc is not None:
                    loc_1: Any = loc
                    def _arrow810(_arg_1: Departures) -> Async[None]:
                        def key(dep: Alternative) -> str:
                            matchValue: str | None = dep.when
                            matchValue_1: StationStop | None = dep.stop
                            (pattern_matching_result, s_1, w_1) = (None, None, None)
                            if matchValue is not None:
                                if matchValue_1 is not None:
                                    if matchValue_1.type == "stop":
                                        def _arrow809(__unit: None=None, dep: Any=dep) -> bool:
                                            w: str = matchValue
                                            return matchValue_1.name is not None

                                        if _arrow809():
                                            pattern_matching_result = 0
                                            s_1 = matchValue_1
                                            w_1 = matchValue

                                        else: 
                                            pattern_matching_result = 1


                                    else: 
                                        pattern_matching_result = 1


                                else: 
                                    pattern_matching_result = 1


                            else: 
                                pattern_matching_result = 1

                            if pattern_matching_result == 0:
                                return w_1 + value_1(s_1.name)

                            elif pattern_matching_result == 1:
                                return ""


                        arg: str = Alternatives(sort_by(key, _arg_1.departures))
                        to_console(printf("%s"))(arg)
                        return singleton.Zero()

                    return singleton.Bind(HafasAsyncClient__AsyncDepartures(client, loc_1, options), _arrow810)

                else: 
                    return singleton.Zero()


            return singleton.Bind(get_location(client, name), _arrow811)

        return AsyncRun(singleton.Delay(_arrow812))


def arrivals(name: str) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow813(__unit: None=None, name: Any=name) -> Any:
            copy_of_struct: Any = date_of_current_hour()
            return add_hours(copy_of_struct, 1)

        options: DeparturesArrivalsOptions = DeparturesArrivalsOptions(_arrow813(), Default_DeparturesArrivalsOptions.direction, Default_DeparturesArrivalsOptions.line, Default_DeparturesArrivalsOptions.duration, Default_DeparturesArrivalsOptions.results, Default_DeparturesArrivalsOptions.sub_stops, Default_DeparturesArrivalsOptions.entrances, Default_DeparturesArrivalsOptions.lines_of_stops, Default_DeparturesArrivalsOptions.remarks, Default_DeparturesArrivalsOptions.stopovers, Default_DeparturesArrivalsOptions.include_related_stations, Default_DeparturesArrivalsOptions.products, Default_DeparturesArrivalsOptions.language)
        def _arrow817(__unit: None=None, name: Any=name) -> Async[None]:
            def _arrow816(_arg: Any | None=None) -> Async[None]:
                loc: Any | None = _arg
                if loc is not None:
                    loc_1: Any = loc
                    def _arrow815(_arg_1: Arrivals) -> Async[None]:
                        def key(dep: Alternative) -> str:
                            matchValue: str | None = dep.when
                            matchValue_1: StationStop | None = dep.stop
                            (pattern_matching_result, s_1, w_1) = (None, None, None)
                            if matchValue is not None:
                                if matchValue_1 is not None:
                                    if matchValue_1.type == "stop":
                                        def _arrow814(__unit: None=None, dep: Any=dep) -> bool:
                                            w: str = matchValue
                                            return matchValue_1.name is not None

                                        if _arrow814():
                                            pattern_matching_result = 0
                                            s_1 = matchValue_1
                                            w_1 = matchValue

                                        else: 
                                            pattern_matching_result = 1


                                    else: 
                                        pattern_matching_result = 1


                                else: 
                                    pattern_matching_result = 1


                            else: 
                                pattern_matching_result = 1

                            if pattern_matching_result == 0:
                                return w_1 + value_1(s_1.name)

                            elif pattern_matching_result == 1:
                                return ""


                        arg: str = Alternatives(sort_by(key, _arg_1.arrivals))
                        to_console(printf("%s"))(arg)
                        return singleton.Zero()

                    return singleton.Bind(HafasAsyncClient__AsyncArrivals(client, loc_1, options), _arrow815)

                else: 
                    return singleton.Zero()


            return singleton.Bind(get_location(client, name), _arrow816)

        return AsyncRun(singleton.Delay(_arrow817))


def trip(id: str) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow819(__unit: None=None, id: Any=id) -> Async[None]:
            def _arrow818(_arg: TripWithRealtimeData) -> Async[None]:
                arg: str = Trip(_arg.trip)
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncTrip(client, id, None), _arrow818)

        return AsyncRun(singleton.Delay(_arrow819))


def trips_by_name(name: str) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow824(__unit: None=None, name: Any=name) -> Async[None]:
            def _arrow823(_arg: TripsWithRealtimeData) -> Async[None]:
                trips: TripsWithRealtimeData = _arg
                arg: str = Trips(trips.trips)
                to_console(printf("%s"))(arg)
                def action(tupled_arg_1: tuple[tuple[str | None, str | None, str | None], Array[tuple[str | None, str | None, str | None, str | None]]]) -> None:
                    _arg_2: tuple[str | None, str | None, str | None] = tupled_arg_1[0]
                    o_1: str | None = _arg_2[0]
                    m_1: str | None = _arg_2[2]
                    d_1: str | None = _arg_2[1]
                    def mapping_2(d_3: str, tupled_arg_1: Any=tupled_arg_1) -> str:
                        return substring(d_3, 11, 8)

                    def chooser(x_1: str | None=None, tupled_arg_1: Any=tupled_arg_1) -> str | None:
                        return x_1

                    def mapping_1(tupled_arg_2: tuple[str | None, str | None, str | None, str | None], tupled_arg_1: Any=tupled_arg_1) -> str | None:
                        return tupled_arg_2[3]

                    class ObjectExpr821:
                        @property
                        def Equals(self) -> Callable[[str, str], bool]:
                            def _arrow820(x_2: str, y_1: str) -> bool:
                                return x_2 == y_1

                            return _arrow820

                        @property
                        def GetHashCode(self) -> Callable[[str], int]:
                            return string_hash

                    departures_1: Array[str] = Array_distinct(map(mapping_2, choose_2(chooser, map(mapping_1, tupled_arg_1[1], None), None), None), ObjectExpr821())
                    (pattern_matching_result, d_4, m_2, o_2) = (None, None, None, None)
                    if o_1 is not None:
                        if d_1 is not None:
                            if m_1 is not None:
                                pattern_matching_result = 0
                                d_4 = d_1
                                m_2 = m_1
                                o_2 = o_1

                            else: 
                                pattern_matching_result = 1


                        else: 
                            pattern_matching_result = 1


                    else: 
                        pattern_matching_result = 1

                    if pattern_matching_result == 0:
                        arg_4: str = join(",", departures_1)
                        to_console(printf("%s %s %s %s"))(o_2)(d_4)(m_2)(arg_4)

                    elif pattern_matching_result == 1:
                        pass


                def projection(tupled_arg: tuple[str | None, str | None, str | None, str | None]) -> tuple[str | None, str | None, str | None]:
                    return (tupled_arg[0], tupled_arg[1], tupled_arg[2])

                def mapping(t: Trip_1) -> tuple[str | None, str | None, str | None, str | None]:
                    matchValue: StationStopLocation | None = t.origin
                    matchValue_1: StationStopLocation | None = t.destination
                    matchValue_2: Line | None = t.line
                    (pattern_matching_result_1, destination, line, origin) = (None, None, None, None)
                    if matchValue is not None:
                        if matchValue.type == "stop":
                            if matchValue_1 is not None:
                                if matchValue_1.type == "stop":
                                    if matchValue_2 is not None:
                                        pattern_matching_result_1 = 0
                                        destination = matchValue_1
                                        line = matchValue_2
                                        origin = matchValue

                                    else: 
                                        pattern_matching_result_1 = 1


                                else: 
                                    pattern_matching_result_1 = 1


                            else: 
                                pattern_matching_result_1 = 1


                        else: 
                            pattern_matching_result_1 = 1


                    else: 
                        pattern_matching_result_1 = 1

                    if pattern_matching_result_1 == 0:
                        return (origin.name, destination.name, line.match_id, t.planned_departure)

                    elif pattern_matching_result_1 == 1:
                        return (None, None, None, None)


                class ObjectExpr822:
                    @property
                    def Equals(self) -> Callable[[tuple[str | None, str | None, str | None], tuple[str | None, str | None, str | None]], bool]:
                        return equal_arrays

                    @property
                    def GetHashCode(self) -> Callable[[tuple[str | None, str | None, str | None]], int]:
                        return array_hash

                value: None = iterate(action, Array_groupBy(projection, map(mapping, trips.trips, None), ObjectExpr822()))
                ignore(None)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncTripsByName(client, name, TripsByNameOptions(now(), Default_TripsByNameOptions.from_when, Default_TripsByNameOptions.until_when, Default_TripsByNameOptions.only_currently_running, Default_TripsByNameOptions.products, Default_TripsByNameOptions.currently_stopping_at, Default_TripsByNameOptions.line_name, ["DB Fernverkehr AG"], Default_TripsByNameOptions.additional_filters)), _arrow823)

        return AsyncRun(singleton.Delay(_arrow824))


def nearby(lon: float, lat: float) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow826(__unit: None=None, lon: Any=lon, lat: Any=lat) -> Async[None]:
            def _arrow825(_arg: Array[StationStopLocation]) -> Async[None]:
                arg: str = Locations(_arg)
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncNearby(client, Location(Default_Location.type, Default_Location.id, Default_Location.name, Default_Location.poi, Default_Location.address, lon, lat, Default_Location.altitude, Default_Location.distance), NearByOptions(10, 5000, Default_NearByOptions.poi, Default_NearByOptions.stops, products(), Default_NearByOptions.sub_stops, Default_NearByOptions.entrances, Default_NearByOptions.lines_of_stops, Default_NearByOptions.language)), _arrow825)

        return AsyncRun(singleton.Delay(_arrow826))


def reachable_from(lon: float, lat: float) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow828(__unit: None=None, lon: Any=lon, lat: Any=lat) -> Async[None]:
            def _arrow827(_arg: DurationsWithRealtimeData) -> Async[None]:
                arg: str = Durations(_arg.reachable)
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncReachableFrom(client, Location(Default_Location.type, Default_Location.id, Default_Location.name, Default_Location.poi, "unused", lon, lat, Default_Location.altitude, Default_Location.distance), ReachableFromOptions(Default_ReachableFromOptions.when, 0, 10, Default_ReachableFromOptions.products, Default_ReachableFromOptions.sub_stops, Default_ReachableFromOptions.entrances, Default_ReachableFromOptions.polylines)), _arrow827)

        return AsyncRun(singleton.Delay(_arrow828))


def radar(n: float, w: float, s: float, e: float) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow830(__unit: None=None, n: Any=n, w: Any=w, s: Any=s, e: Any=e) -> Async[None]:
            def _arrow829(_arg: Radar) -> Async[None]:
                arg: str = Movements(default_arg(_arg.movements, []))
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncRadar(client, BoundingBox(n, w, s, e), RadarOptions(60, 10, trains(), 2400, Default_RadarOptions.sub_stops, Default_RadarOptions.entrances, Default_RadarOptions.polylines, Default_RadarOptions.when)), _arrow829)

        return AsyncRun(singleton.Delay(_arrow830))


def stop(name: str) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow832(__unit: None=None, name: Any=name) -> Async[None]:
            def _arrow831(_arg: StationStopLocation) -> Async[None]:
                arg: str = U3StationStopLocation(0, _arg)
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncStop(client, name, StopOptions(True, Default_StopOptions.sub_stops, Default_StopOptions.entrances, Default_StopOptions.remarks, Default_StopOptions.language)), _arrow831)

        return AsyncRun(singleton.Delay(_arrow832))


def remarks(__unit: None=None) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow834(__unit: None=None) -> Async[None]:
            def _arrow833(_arg: WarningsWithRealtimeData) -> Async[None]:
                arg: str = Warnings(_arg.remarks)
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncRemarks_7D671456(client, None), _arrow833)

        return AsyncRun(singleton.Delay(_arrow834))


def lines(name: str) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow836(__unit: None=None, name: Any=name) -> Async[None]:
            def _arrow835(_arg: LinesWithRealtimeData) -> Async[None]:
                arg: str = Lines(default_arg(_arg.lines, []))
                to_console(printf("%s"))(arg)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncLines(client, name, None), _arrow835)

        return AsyncRun(singleton.Delay(_arrow836))


def server_info(__unit: None=None) -> None:
    with HafasAsyncClient__ctor_Z3AB94A1B(profile()) as client:
        def _arrow838(__unit: None=None) -> Async[None]:
            def _arrow837(_arg: ServerInfo) -> Async[None]:
                server_info_1: ServerInfo = _arg
                to_console(printf("hciVersion: %A, timetableStart: %A, timetableEnd: %A, serverTime: %A"))(server_info_1.hci_version)(server_info_1.timetable_start)(server_info_1.timetable_end)(server_info_1.server_time)
                return singleton.Zero()

            return singleton.Bind(HafasAsyncClient__AsyncServerInfo_70DF6D02(client, None), _arrow837)

        return AsyncRun(singleton.Delay(_arrow838))


def run(arg: CliArguments) -> None:
    if arg.tag == 16:
        profile(arg.fields[0])

    elif arg.tag == 0:
        locations(arg.fields[0])

    elif arg.tag == 2:
        refresh_journey(arg.fields[0])

    elif arg.tag == 3:
        journeys(arg.fields[0], arg.fields[1], None)

    elif arg.tag == 4:
        best_prices(arg.fields[0], arg.fields[1], None)

    elif arg.tag == 5:
        journeys(arg.fields[0], arg.fields[1], arg.fields[2])

    elif arg.tag == 1:
        stop(arg.fields[0])

    elif arg.tag == 6:
        journeys_from_trip(arg.fields[0], arg.fields[1], arg.fields[2], arg.fields[3])

    elif arg.tag == 8:
        departures(arg.fields[0])

    elif arg.tag == 7:
        arrivals(arg.fields[0])

    elif arg.tag == 9:
        trip(arg.fields[0])

    elif arg.tag == 10:
        trips_by_name(arg.fields[0])

    elif arg.tag == 11:
        nearby(arg.fields[0], arg.fields[1])

    elif arg.tag == 12:
        reachable_from(arg.fields[0], arg.fields[1])

    elif arg.tag == 13:
        radar(arg.fields[0], arg.fields[1], arg.fields[2], arg.fields[3])

    elif arg.tag == 14:
        lines(arg.fields[0])

    elif arg.tag == 15:
        server_info()

    elif arg.tag == 18:
        arg_1: str = print_help()
        to_console(printf("%s"))(arg_1)

    else: 
        Log_set_Debug_Z1FBCCD16(True)



def main(argv: Array[str]) -> int:
    try: 
        HafasAsyncClient_initSerializer()
        def _arrow839(arg: CliArguments) -> None:
            run(arg)

        iterate_1(_arrow839, parse(of_array(argv)))

    except Exception as e:
        arg_1: str = str(e)
        arg_2: str = e.stack
        to_console(printf("main error: %s %A"))(arg_1)(arg_2)

    return 0


if __name__ == "__main__":
    main(sys.argv[1:])


__all__ = ["CliArguments_reflection", "print_help", "to_profile", "parse", "maybe_array", "profile", "products_of_id", "products", "trains", "get_location", "AsyncRunCatched", "AsyncRun", "locations", "JourneyOption_reflection", "parse_journey_options", "apply_journey_option", "apply_journey_options", "get_journey_options", "journeys", "add_days", "best_prices", "journeys_from_trip", "refresh_journey", "date_of_current_hour", "sort_by", "departures", "arrivals", "trip", "trips_by_name", "nearby", "reachable_from", "radar", "stop", "remarks", "lines", "server_info", "run"]

