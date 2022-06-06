module Arguments

open System

open FsHafas.Client

/// see https://github.com/fable-compiler/Fable/blob/main/src/Fable.Cli/Entry.fs
module Entry =

    let allocateArray (n: int) : 'T [] =
        Array.create n (Unchecked.defaultof<'T>)

    // see https://github.com/fable-compiler/Fable/blob/3bc6e82f7aedfc085dfe94ad5b14bf5521984da6/src/fable-library/Array.fs#L898
    // allocates (source.Length - windowSize + 1) instead of (source.Length - windowSize)
    // see https://github.com/dotnet/fsharp/blob/a717e53e756866e6096618dd27afdbf0b7cbd186/src/fsharp/FSharp.Core/array.fs#L775
    let windowedArray (windowSize: int) (source: 'T []) : 'T [] [] =
        if windowSize <= 0 then
            failwith "windowSize must be positive"

        let res =
            FSharp.Core.Operators.max 0 (source.Length - windowSize + 1)
            |> allocateArray

        for i = windowSize to source.Length do
            res.[i - windowSize] <- source.[i - windowSize .. i - 1]

        res

    let windowedList (windowSize: int) (xs: 'T list) : 'T list list =
#if FABLE_COMPILER
        // workaround: allocation error in Array.windowed
        List.toArray xs
        |> windowedArray windowSize
        |> Array.map List.ofArray
        |> List.ofArray
#else
        List.windowed windowSize xs
#endif

    let argValue key (args: string list) =
        args
        |> windowedList 2
        |> List.tryPick (function
            | [ key2; value ] when not (value.StartsWith("-")) && key = key2 -> Some value
            | _ -> None)

    let tryFlag flag (args: string list) =
        match argValue flag args with
        | Some flag ->
            match Boolean.TryParse(flag) with
            | true, flag -> Some flag
            | false, _ -> None
        // Flags can be activated without an explicit value
        | None when List.contains flag args -> Some true
        | None -> None

    let flagEnabled flag (args: string list) =
        tryFlag flag args |> Option.defaultValue false

let private argValue2 key (args: string list) =
    args
    |> Entry.windowedList 3
    |> List.tryPick (function
        | [ key2; value1; value2 ] when
            not (value1.StartsWith("-") || value2.StartsWith("-"))
            && key = key2
            ->
            Some(value1, value2)
        | _ -> None)

let private argValue3 key (args: string list) =
    args
    |> Entry.windowedList 4
    |> List.tryPick (function
        | [ key2; value1; value2; value3 ] when
            not (
                value1.StartsWith("-")
                || value2.StartsWith("-")
                || value3.StartsWith("-")
            )
            && key = key2
            ->
            Some(value1, value2, value3)
        | _ -> None)

let private argValue4 key (args: string list) =
    args
    |> Entry.windowedList 5
    |> List.tryPick (function
        | [ key2; value1; value2; value3; value4 ] when
            not (
                value1.StartsWith("-")
                || value2.StartsWith("-")
                || value3.StartsWith("-")
                || value4.StartsWith("-")
            )
            && key = key2
            ->
            Some(value1, value2, value3, value4)
        | _ -> None)

let valueToArg (key: string) (mk: string -> 'a) (args: string list) =
    match Entry.argValue key args with
    | None -> None
    | Some v -> mk v |> Some

let value2ToArg (key: string) (mk: string * string -> 'a) (args: string list) =
    match argValue2 key args with
    | None -> None
    | Some v -> mk v |> Some

let value3ToArg (key: string) (mk: string * string * string -> 'a) (args: string list) =
    match argValue3 key args with
    | None -> None
    | Some v -> mk v |> Some

let value4ToArg (key: string) (mk: string * string * string * string -> 'a) (args: string list) =
    match argValue4 key args with
    | None -> None
    | Some v -> mk v |> Some

let flagToArg (key: string) (flag: 'a) (args: string list) =
    if Entry.flagEnabled key args then
        flag |> Some
    else
        None
