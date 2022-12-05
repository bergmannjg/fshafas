module RunProcess

open System
open System.Diagnostics

let runProc filename args startDir =
    let timer = Stopwatch.StartNew()

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
        let exited = p.WaitForExit(10 * 1000)

        if not exited then
            p.Start() |> ignore
            p.BeginOutputReadLine()
            p.BeginErrorReadLine()
            let exited = p.WaitForExit(10 * 1000)
            if not exited then errors.Add("timeout")

        timer.Stop()
        printfn "Finished %s after %A milliseconds" filename timer.ElapsedMilliseconds

        let cleanOut l =
            l
            |> Seq.filter (fun o -> String.IsNullOrEmpty o |> not)
            |> Seq.filter (fun o -> o.Contains "ExperimentalWarning" |> not)
            |> Seq.filter (fun o -> o.Contains "node --trace-warnings" |> not)

        cleanOut outputs, cleanOut errors
    with
    | ex ->
        fprintfn stderr "error: %s" ex.Message
        (Seq.empty, [ ex.Message ])
