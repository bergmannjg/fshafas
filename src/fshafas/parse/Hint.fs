namespace FsHafas.Parser

module internal Hint =

    open FsHafas.Client
    open FsHafas.Endpoint

    let defaultHint: Hint =
        { ``type`` = HintType.Hint
          code = None
          summary = None
          text = ""
          tripId = None }

    let defaultStatus: Status =
        { ``type`` = HintType.Status
          code = None
          summary = None
          text = ""
          tripId = None }

    let parseHint (ctx: Context) (h: FsHafas.Raw.RawRem) : HintStatusWarning option =
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
            HintStatusWarning.Status
                ({ defaultStatus with
                    text = text
                    summary = h.txtS |> trim
                    code = code })
            |> Some
        else if "AI".Contains h.``type`` then
            HintStatusWarning.Hint
                ({ defaultHint with
                    text = text
                    code = code })
            |> Some
        else if "DURNYQP".Contains h.``type`` then
            HintStatusWarning.Status
                ({ defaultStatus with
                    text = text
                    code = code })
            |> Some
        else
            None
