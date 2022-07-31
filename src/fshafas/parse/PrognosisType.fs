namespace FsHafas.Parser

module internal PrognosisType =

    open FsHafas.Client
    open FsHafas.Endpoint

    let parsePrognosisType (ctx: Context) (p: string option) : PrognosisType option =
        match p with
        | Some "PROGNOSED" -> Some Prognosed
        | Some "CALCULATED" -> Some Calculated
        | _ -> None
