module Program

open System.Diagnostics

let checkEnv (s: string) =
    try
        System.Environment.GetEnvironmentVariable s = "1"
    with _ -> false

[<EntryPoint>]
let main argv =

    if checkEnv "TRANSFORMER_DEBUG" then
        use p =
            System.Diagnostics.Process.GetCurrentProcess()

        printf "procees  %d, press any key..." p.Id
        System.Console.ReadKey() |> ignore

    match argv with
    | [| target; fromFile; toFile |] ->
        let options : Transformer.TransformerOptions =
            match target with
            | "FsHafas" -> FsHafasOptions.options
            | "RawHafas" -> RawHafasOptions.options
            | _ -> failwith "unknown target"

        Transformer.transform fromFile toFile options
    | _ -> failwith "arguments expected: target fromFile toFile"

    0 // return an integer exit code
