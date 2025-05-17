#!/usr/bin/env bash
# some fixes in fable_library

sed -i 's/print(\"reg/# print(\"reg/' fshafas/fable_modules/fable_library/reg_exp.py

sed -i 's/print(match/# print(match/' fshafas/fable_modules/fable_library/date.py 

sed -i 's/\.lib\.\.\./../' fshafas/fable_modules/db_vendo_python/db_vendo_async_client.py 
sed -i 's/\.lib\.\.\./../' fshafas/fable_modules/db_vendo_python/format_request.py