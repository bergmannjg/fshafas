module RunProcess

open System
open System.Diagnostics

let runProc filename args startDir =
    fprintfn stderr $"{filename} {args}"

    let procStartInfo =
        ProcessStartInfo(
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            FileName = filename,
            Arguments = args
        )

    match startDir with
    | Some d -> procStartInfo.WorkingDirectory <- d
    | _ -> ()

    try
        let outputs = System.Collections.Generic.List<string>()
        let errors = System.Collections.Generic.List<string>()
        let outputHandler f (_sender: obj) (args: DataReceivedEventArgs) = f args.Data
        let p = new Process(StartInfo = procStartInfo)
        p.OutputDataReceived.AddHandler(DataReceivedEventHandler(outputHandler outputs.Add))
        p.ErrorDataReceived.AddHandler(DataReceivedEventHandler(outputHandler errors.Add))
        p.Start() |> ignore
        p.BeginOutputReadLine()
        p.BeginErrorReadLine()

        if not (p.WaitForExit(60 * 1000)) then
            fprintfn stderr "error: process not exited after 60 seconds"

        let cleanOut (l: Collections.Generic.List<string>) =
            l
            |> Seq.filter (fun o -> String.IsNullOrEmpty o |> not)
            |> Seq.filter (fun o -> o.Contains "npm notice" |> not)
            |> Seq.filter (fun o -> o.Contains "warning:" |> not)
            |> Seq.toList

        let toError (l: Collections.Generic.List<string>) =
            l |> Seq.filter (fun o -> not (isNull o) && o.Contains "error FS") |> Seq.toList

        cleanOut outputs, List.append (cleanOut errors) (toError outputs)
    with ex ->
        fprintfn stderr "error: %s %s" ex.Message ex.StackTrace
        (Seq.empty |> Seq.toList, [ ex.Message ])
