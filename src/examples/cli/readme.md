# Command Line Interface

```txt
USAGE: cli.exe [--help] [--locations <name>] [--stop <id>] [--journeys <from> <to>] 
               [--journeysfromtrip <fromId> <toId> <newToId>]
               [--departures <name>] [--trips <name>] [--nearby <lon> <lat>] [--reachablefrom <lon> <lat>]
               [--radar <north> <west> <south> <east>] [--lines <name>] [--serverinfo] [--profile <db|bvg|svv>] [--debug]

OPTIONS:

    --locations <name>    get locations, e.g. Hannover.
    --stop <id>           get stop, e.g. 8000152.
    --journeys <from> <to>
                          get journeys, e.g. Hannover Berlin.
    --journeysfromtrip <fromId> <toId> <newToId>
                          get journeys from trip, e.g. 8002549 8000261 8000207.
    --departures <name>   get Departures, e.g. Hannover.
    --trips <name>        get Trips, e.g. ICE 1001.
    --nearby <lon> <lat>  get Nearby, e.g. 13.078028 54.308438.
    --reachablefrom <lon> <lat>
                          get ReachableFrom, e.g. 13.078028 54.308438.
    --radar <north> <west> <south> <east>
                          get Radar, e.g. 52.039421 8.522777 52.019421 8.542777.
    --lines <name>        get Lines, e.g. S1, profile svv.
    --serverinfo          get ServerInfo.
    --profile <db|bvg|svv>
                          set Profile.
    --debug               show debug msgs.
    --help                display this list of options.
```
