namespace FsHafas.Parser

module internal Icon =

    open FsHafas.Client
    open FsHafas.Endpoint

    let parseIcon (ctx: Context) (i: FsHafas.Raw.RawIco) : Icon option =
        match i.res with
        | Some res when res <> "Empty" ->
            Some
                { ``type`` = res
                  title = i.text |> Option.orElse i.txt |> Option.orElse i.txtS }
        | _ -> None
