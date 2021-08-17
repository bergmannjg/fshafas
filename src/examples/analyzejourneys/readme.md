# Analyze Journeys

Analyze journeydata from the calendar csv export of DB Navigator app.

The program computes the price and distance of the selected journeys.

```txt
USAGE: analyzejourneys.exe [--help] [--file <path>] [--datestart <date>] [--dateend <date>] [--priceinndays <offset>] [--take <count>] [--discount <no|bc25|bc50>] [--debug]

OPTIONS:

    --file <path>         path to csv file.
    --datestart <date>    start date, default 01/01/20
    --dateend <date>      end date, default 08/28/21
    --priceinndays <offset>
                          get price in n days, deafult 1.
    --take <count>        take only count elems, default 1000.
    --discount <no|bc25|bc50>
                          Discount of Bahncard, default BC25.
    --debug               show debug msgs.
    --help                display this list of options.
```
