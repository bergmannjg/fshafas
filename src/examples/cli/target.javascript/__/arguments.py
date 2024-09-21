from __future__ import annotations
from collections.abc import Callable
from typing import (Any, TypeVar)
from .target_javascript.fable_modules.fable_library.boolean import try_parse
from .target_javascript.fable_modules.fable_library.list import (try_pick, is_empty, tail, head, FSharpList, windowed, contains)
from .target_javascript.fable_modules.fable_library.option import (default_arg, some)
from .target_javascript.fable_modules.fable_library.types import FSharpRef
from .target_javascript.fable_modules.fable_library.util import string_hash

_A = TypeVar("_A")

def Entry_argValue(key: str, args: FSharpList[str]) -> str | None:
    def chooser(_arg: FSharpList[str], key: Any=key, args: Any=args) -> str | None:
        (pattern_matching_result, key2_1, value_1) = (None, None, None)
        if not is_empty(_arg):
            if not is_empty(tail(_arg)):
                if is_empty(tail(tail(_arg))):
                    if (key == head(_arg)) if (not (head(tail(_arg)).find("-") == 0)) else False:
                        pattern_matching_result = 0
                        key2_1 = head(_arg)
                        value_1 = head(tail(_arg))

                    else: 
                        pattern_matching_result = 1


                else: 
                    pattern_matching_result = 1


            else: 
                pattern_matching_result = 1


        else: 
            pattern_matching_result = 1

        if pattern_matching_result == 0:
            return value_1

        elif pattern_matching_result == 1:
            return None


    return try_pick(chooser, windowed(2, args))


def Entry_tryFlag(flag: str, args: FSharpList[str]) -> bool | None:
    match_value: str | None = Entry_argValue(flag, args)
    if match_value is None:
        class ObjectExpr754:
            @property
            def Equals(self) -> Callable[[str, str], bool]:
                def _arrow753(x: str, y: str) -> bool:
                    return x == y

                return _arrow753

            @property
            def GetHashCode(self) -> Callable[[str], int]:
                return string_hash

        if contains(flag, args, ObjectExpr754()):
            return True

        else: 
            return None


    else: 
        match_value_1: tuple[bool, bool]
        out_arg: bool = False
        def _arrow755(__unit: None=None, flag: Any=flag, args: Any=args) -> bool:
            return out_arg

        def _arrow756(v: bool, flag: Any=flag, args: Any=args) -> None:
            nonlocal out_arg
            out_arg = v

        match_value_1 = (try_parse(match_value, FSharpRef(_arrow755, _arrow756)), out_arg)
        if match_value_1[0]:
            return match_value_1[1]

        else: 
            return None




def Entry_flagEnabled(flag: str, args: FSharpList[str]) -> bool:
    return default_arg(Entry_tryFlag(flag, args), False)


def arg_value2(key: str, args: FSharpList[str]) -> tuple[str, str] | None:
    def chooser(_arg: FSharpList[str], key: Any=key, args: Any=args) -> tuple[str, str] | None:
        (pattern_matching_result, key2_1, value1_1, value2_1) = (None, None, None, None)
        if not is_empty(_arg):
            if not is_empty(tail(_arg)):
                if not is_empty(tail(tail(_arg))):
                    if is_empty(tail(tail(tail(_arg)))):
                        if (key == head(_arg)) if (not (True if (head(tail(_arg)).find("-") == 0) else (head(tail(tail(_arg))).find("-") == 0))) else False:
                            pattern_matching_result = 0
                            key2_1 = head(_arg)
                            value1_1 = head(tail(_arg))
                            value2_1 = head(tail(tail(_arg)))

                        else: 
                            pattern_matching_result = 1


                    else: 
                        pattern_matching_result = 1


                else: 
                    pattern_matching_result = 1


            else: 
                pattern_matching_result = 1


        else: 
            pattern_matching_result = 1

        if pattern_matching_result == 0:
            return (value1_1, value2_1)

        elif pattern_matching_result == 1:
            return None


    return try_pick(chooser, windowed(3, args))


def arg_value3(key: str, args: FSharpList[str]) -> tuple[str, str, str] | None:
    def chooser(_arg: FSharpList[str], key: Any=key, args: Any=args) -> tuple[str, str, str] | None:
        (pattern_matching_result, key2_1, value1_1, value2_1, value3_1) = (None, None, None, None, None)
        if not is_empty(_arg):
            if not is_empty(tail(_arg)):
                if not is_empty(tail(tail(_arg))):
                    if not is_empty(tail(tail(tail(_arg)))):
                        if is_empty(tail(tail(tail(tail(_arg))))):
                            if (key == head(_arg)) if (not (True if (True if (head(tail(_arg)).find("-") == 0) else (head(tail(tail(_arg))).find("-") == 0)) else (head(tail(tail(tail(_arg)))).find("-") == 0))) else False:
                                pattern_matching_result = 0
                                key2_1 = head(_arg)
                                value1_1 = head(tail(_arg))
                                value2_1 = head(tail(tail(_arg)))
                                value3_1 = head(tail(tail(tail(_arg))))

                            else: 
                                pattern_matching_result = 1


                        else: 
                            pattern_matching_result = 1


                    else: 
                        pattern_matching_result = 1


                else: 
                    pattern_matching_result = 1


            else: 
                pattern_matching_result = 1


        else: 
            pattern_matching_result = 1

        if pattern_matching_result == 0:
            return (value1_1, value2_1, value3_1)

        elif pattern_matching_result == 1:
            return None


    return try_pick(chooser, windowed(4, args))


def arg_value4(key: str, args: FSharpList[str]) -> tuple[str, str, str, str] | None:
    def chooser(_arg: FSharpList[str], key: Any=key, args: Any=args) -> tuple[str, str, str, str] | None:
        (pattern_matching_result, key2_1, value1_1, value2_1, value3_1, value4_1) = (None, None, None, None, None, None)
        if not is_empty(_arg):
            if not is_empty(tail(_arg)):
                if not is_empty(tail(tail(_arg))):
                    if not is_empty(tail(tail(tail(_arg)))):
                        if not is_empty(tail(tail(tail(tail(_arg))))):
                            if is_empty(tail(tail(tail(tail(tail(_arg)))))):
                                if (key == head(_arg)) if (not (True if (True if (True if (head(tail(_arg)).find("-") == 0) else (head(tail(tail(_arg))).find("-") == 0)) else (head(tail(tail(tail(_arg)))).find("-") == 0)) else (head(tail(tail(tail(tail(_arg))))).find("-") == 0))) else False:
                                    pattern_matching_result = 0
                                    key2_1 = head(_arg)
                                    value1_1 = head(tail(_arg))
                                    value2_1 = head(tail(tail(_arg)))
                                    value3_1 = head(tail(tail(tail(_arg))))
                                    value4_1 = head(tail(tail(tail(tail(_arg)))))

                                else: 
                                    pattern_matching_result = 1


                            else: 
                                pattern_matching_result = 1


                        else: 
                            pattern_matching_result = 1


                    else: 
                        pattern_matching_result = 1


                else: 
                    pattern_matching_result = 1


            else: 
                pattern_matching_result = 1


        else: 
            pattern_matching_result = 1

        if pattern_matching_result == 0:
            return (value1_1, value2_1, value3_1, value4_1)

        elif pattern_matching_result == 1:
            return None


    return try_pick(chooser, windowed(5, args))


def value_to_arg(key: str, mk: Callable[[str], _A], args: FSharpList[str]) -> _A | None:
    match_value: str | None = Entry_argValue(key, args)
    if match_value is not None:
        return some(mk(match_value))

    else: 
        return None



def value2to_arg(key: str, mk: Callable[[tuple[str, str]], _A], args: FSharpList[str]) -> _A | None:
    match_value: tuple[str, str] | None = arg_value2(key, args)
    if match_value is not None:
        return some(mk(match_value))

    else: 
        return None



def value3to_arg(key: str, mk: Callable[[tuple[str, str, str]], _A], args: FSharpList[str]) -> _A | None:
    match_value: tuple[str, str, str] | None = arg_value3(key, args)
    if match_value is not None:
        return some(mk(match_value))

    else: 
        return None



def value4to_arg(key: str, mk: Callable[[tuple[str, str, str, str]], _A], args: FSharpList[str]) -> _A | None:
    match_value: tuple[str, str, str, str] | None = arg_value4(key, args)
    if match_value is not None:
        return some(mk(match_value))

    else: 
        return None



def flag_to_arg(key: str, flag: _A, args: FSharpList[str]) -> _A | None:
    if Entry_flagEnabled(key, args):
        return some(flag)

    else: 
        return None



__all__ = ["Entry_argValue", "Entry_tryFlag", "Entry_flagEnabled", "arg_value2", "arg_value3", "arg_value4", "value_to_arg", "value2to_arg", "value3to_arg", "value4to_arg", "flag_to_arg"]

