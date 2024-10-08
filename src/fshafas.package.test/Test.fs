module PackageTest

open System.IO
open NUnit.Framework
open RunProcess

let dump (prefix: string) (args: string) (content: seq<string>) =
    let path = "../../../tmp/"

    if Directory.Exists path then
        File.WriteAllLines(path + prefix + args + ".txt", content)
    else
        fprintfn stderr "path: %s not found in %s" path (Directory.GetCurrentDirectory())

let cliDirectory = "../../../../examples/cli"

let callDotnet (args: string) =
    runProc "dotnet" ("bin/Debug/net8.0/cli.dll " + args) (Some(cliDirectory + "/target.dotnet"))

let callJavaScript (args: string) =
    runProc "node" ("Program.js " + args) (Some(cliDirectory + "/target.javascript"))

let callPython (args: string) =
    runProc ("python3.10") ("program.py " + args) (Some(cliDirectory + "/target.python"))

let args () =
    [| "--locations Hannover"
       "--stop 8000152"
       "--journeys 8000152 8000036"
       "--journeysfromtrip 8002549 8000261 8000207"
       "--departures 8000036"
       "--trips \"ICE 1001\""
       "--nearby 13.078028 54.308438"
       "--reachablefrom 13.078028 54.308438"
       "--radar 52.039421 8.522777 52.019421 8.542777" |]

let DotnetEqualsToSource (prefix: string) (args: string) (source: string -> seq<string> * seq<string>) =
    printfn "test: %s" args

    let (netoutputs, neterrors) = callDotnet args
    Assert.That(neterrors |> Seq.isEmpty)

    let (outputs, errors) = source args

    if not (Seq.isEmpty errors) then
        fprintfn stderr "errors: %A" errors

    Assert.That(errors |> Seq.isEmpty)

    dump "net" args netoutputs
    dump prefix args outputs

    Assert.That(netoutputs |> Seq.length > 0)
    Assert.That(outputs |> Seq.length > 0)
    Assert.That(netoutputs |> Seq.length, Is.EqualTo(outputs |> Seq.length))

    let comparer (a: string) (b: string) =
        // ignore realtime data
        if a.Contains "currentLocation" && b.Contains "currentLocation" then
            0
        else
            let compareTo = a.CompareTo b

            if (compareTo <> 0) then
                fprintfn stderr "is distinct a: '%s', b: '%s'" a b

            compareTo

    Assert.That(0, Is.EqualTo((netoutputs, outputs) ||> Seq.compareWith comparer))

[<TestCaseSource(nameof (args))>]
let DotnetEqualsToJavaScript (args: string) =
    DotnetEqualsToSource "js" args callJavaScript

[<TestCaseSource(nameof (args))>]
let DotnetEqualsToPython (args: string) =
    DotnetEqualsToSource "py" args callPython
