namespace FsHafas.Parser

module internal Hint =

    open FsHafas
    open FsHafas.Client

#if FABLE_COMPILER
    open Fable.Core
#endif

    let defaultHint: Client.Hint =
        { ``type`` = Some "hint"
          code = None
          summary = None
          text = ""
          tripId = None }

    let defaultStatus: Client.Status =
        { ``type`` = Some "status"
          code = None
          summary = None
          text = ""
          tripId = None }

    let parseHint (ctx: Context) (h: Raw.RawRem): U3<Client.Hint,Client.Status,Client.Warning> option =
        let text =
            match h.txtN with
            | Some txtN -> txtN.Trim()
            | None -> ""

        let code =
            if h.code <> "" then
                Some h.code
            else
                None

        let trim (s: string option) = s |> Option.map (fun s -> s.Trim())

        if h.``type`` = "M" then
            U3<Client.Hint,Client.Status,Client.Warning>.Case2(
                { defaultStatus with
                      text = text
                      summary = h.txtS |> trim
                      code = code }
            )
            |> Some
        else if "AI".Contains h.``type`` then
            U3<Client.Hint,Client.Status,Client.Warning>.Case1(
                { defaultHint with
                      text = text
                      code = code }
            )
            |> Some
        else if "DURNYQP".Contains h.``type`` then
            U3<Client.Hint,Client.Status,Client.Warning>.Case2(
                { defaultStatus with
                      text = text
                      code = code }
            )
            |> Some
        else
            None
