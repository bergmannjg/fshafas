namespace DbVendo.Client

module internal Option =

    let getD<'a> (opt: 'a option) (d: 'a) : 'a =
        match opt with
        | Some(v) -> v
        | None -> d

    let equals<'a when 'a: equality> (opt: 'a option) (d: 'a) : bool option =
        match opt with
        | Some v -> Some(v = d)
        | None -> None

    let getValue<'a, 'b> (opt: 'a option) (getter: 'a -> 'b option) =
        match opt with
        | Some(value) -> getter value
        | None -> None


    let getValueD<'a, 'b> (opt: 'a option) (getter: 'a -> 'b option) (defaultOpt: 'a) =
        let defaultValue =
            match getter defaultOpt with
            | Some value -> value
            | None -> failwith "Option.getValue: value expected"

        match opt with
        | Some(value) ->
            match getter value with
            | Some result -> result
            | None -> defaultValue
        | None -> defaultValue
