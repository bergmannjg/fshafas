#!/usr/bin/env bash
# some fixes in fable_library

sed -i 's/print(\"reg/# print(\"reg/' fshafas/fable_modules/fable_library/reg_exp.py

sed -i 's/print(match/# print(match/' fshafas/fable_modules/fable_library/date.py 