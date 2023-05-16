# Command Line Interface

## Run with dotnet

* cd target.dotnet
* **run**: dotnet run --project cli.fsproj -- --help

## Run with node

* cd target.javascript
* npm install
* **build**: dotnet fable cli.fable.javascript.fsproj -o build
* **run**: node Program.js --help

## Run with python

* cd target.python
* pip install -r requirements.txt
* **build**: dotnet fable cli.fable.python.fsproj --lang Python
* **run**: python3.9 program.py --help

## Usage

```txt
USAGE: cli.exe [--help] [--locations <name>] [--stop <id>] [--journeys <from> <to>] 
               [--journeysfromtrip <tripId> <prevStopId> <prevStopDepature> <newToId>]
               [--departures <name>] [--trips <name>] [--nearby <lon> <lat>] [--reachablefrom <lon> <lat>]
               [--radar <north> <west> <south> <east>] [--lines <name>] [--serverinfo] [--profile <db|bvg|svv>] [--debug]

OPTIONS:

    --locations <name>    get locations, e.g. Hannover.
    --stop <id>           get stop, e.g. 8000152.
    --journeys <from> <to>
                          get journeys, e.g. Hannover Berlin.
    --journeysfromtrip <tripId> <prevStopId> <prevStopDepature> <newToId>
                          get journeys from  <prevStopId> of trip <tripId> to new target <newToId>.
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
