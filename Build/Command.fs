module Command

open System.IO
open RunProcess

let printOutputs (cmd: string) (netoutputs: List<string>, neterrors: List<string>) =
    if cmd = "test" then
        netoutputs
        |> List.filter (fun s -> s.Contains "Failed")
        |> List.iter (fun s -> printfn $"{s}")

    (netoutputs, neterrors)

let failOnError (netoutputs: List<string>, neterrors: List<string>) =
    if neterrors.Length > 0 then
        raise (System.ArgumentException(neterrors.ToString()))

let delete (path: string) =
    printfn $"delete directory {path}"

    if Directory.Exists path then
        Directory.Delete(path, true)

    if File.Exists path then
        File.Delete path

let mv (_from: string) (_to: string) =
    printfn $"mv '{_from}' '{_to}' "

    if Directory.Exists _from then
        Directory.Move(_from, _to)
    else if File.Exists _from then
        if Directory.Exists _to then
            File.Move(_from, _to + _from, true)
        else
            File.Move(_from, _to, true)
    else
        raise (System.ArgumentException("not found: " + _from))

let dotnet (cmd: string) (args: list<string>) =
    runProc "dotnet" (cmd + " " + String.concat " " args) None
    |> printOutputs cmd
    |> failOnError

let npm (cmd: string) (args: list<string>) =
    runProc "npm" (cmd + " " + String.concat " " args) None
    |> failOnError

let nuget (cmd: string) (args: list<string>) =
    runProc "/usr/local/bin/nuget.exe" (cmd + " " + String.concat " " args) None
    |> failOnError

let python (lib: string) (args: list<string>) =
    runProc "python3.10" ("-m " + lib + " " + String.concat " " args) None
    |> failOnError

let exec (cmd: string) (directory: string) =
    runProc "bash" ("-c " + cmd) (Some directory)
    |> failOnError
