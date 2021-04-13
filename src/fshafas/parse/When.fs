namespace FsHafas.Parser

module internal When =

    open System

    let defaultWhen =
        { ``when`` = None
          plannedWhen = None
          prognosedWhen = None
          delay = None }

    let parseWhen
        (ctx: Context)
        (date: string)
        (timeS: string option)
        (timeR: string option)
        (tzOffset: int option)
        (cncl: bool option)
        : ParsedWhen =
        let (dtPlanned, strPlanned) =
            FsHafas.Parser.DateTime.parseDateTimeEx ctx.profile date timeS tzOffset

        let (dtPrognosed, strPrognosed) =
            FsHafas.Parser.DateTime.parseDateTimeEx ctx.profile date timeR tzOffset

        let delay =
            match dtPrognosed, dtPlanned with
            | Some (prognosed), Some (planned) -> Some(Convert.ToInt32((prognosed - planned).TotalSeconds))
            | _ -> None

        if (cncl |> Option.exists id) then
            { ``when`` = None
              plannedWhen = strPlanned
              prognosedWhen = strPrognosed
              delay = delay }
        else
            { ``when`` = strPrognosed |> Option.orElse strPlanned
              plannedWhen = strPlanned
              prognosedWhen = None
              delay = delay }
