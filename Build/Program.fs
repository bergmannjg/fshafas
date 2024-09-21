module App

open System
open System.IO
open Target

let getVersion (package: string) =
    match File.ReadAllLines "paket.dependencies"
          |> Array.tryFind (fun line -> line.Contains package)
        with
    | Some line ->
        match line.Split '=' |> Array.toList with
        | [ _; version ] -> Some(version.Trim())
        | _ -> None
    | None -> None

let depends (argv: string array) (deps: string list) =
    argv.Length > 0
    && List.exists (fun dep -> String.Compare(argv[0], dep, true) = 0) deps

[<EntryPoint>]
let main argv =
    try
        match getVersion "FsHafas.JavaScript", getVersion "FsHafas.Python", getVersion "Hafas.Client.Bindings" with
        | Some version, Some version_python, Some version_bindings ->
            if Directory.Exists "Build" && Directory.Exists "src" then
                if depends argv [ "Test" ] then
                    buildDotnet ()
                    testDotnet ()
                    buildDotnetNupkg version

                else if depends argv [ "FableBindings" ] then
                    buildFableBindingNupkg version_bindings

                else if depends argv [ "JavaScript" ] then
                    buildDotnet ()
                    testDotnet ()
                    buildJavascriptNupkg version
                    buildJavascript version

                else if depends argv [ "JavaScript.Test" ] then
                    buildDotnet ()
                    testDotnet ()
                    buildDotnetNupkg version
                    buildJavascriptNupkg version
                    buildJavascriptTest ()

                else if depends argv [ "Python" ] then
                    buildDotnet ()
                    testDotnet ()
                    buildPythonNupkg version_python
                    buildPython version_python

                else if depends argv [ "Python.Test" ] then
                    buildDotnet ()
                    testDotnet ()
                    buildDotnetNupkg version
                    buildPythonNupkg version_python
                    buildPythonTest ()
                else
                    printfn "args expected: Test|JavaScript|Python|FableBindings"
            else
                printfn "please run from project directory"
        | _, _, _ -> printfn "version info not found in file paket.dependencies"
    with
    | e -> printfn "error: %s %A" e.Message e.StackTrace

    0
