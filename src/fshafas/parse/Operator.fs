namespace FsHafas.Parser

module internal Operator =

    open FsHafas.Client
    open FsHafas.Endpoint

    let private slug (s: string) = Slug.slugify s

    let defaultOperator: FsHafas.Client.Operator =
        { ``type`` = Some "operator"
          id = ""
          name = "" }

    let parseOperator (ctx: Context) (a: FsHafas.Raw.RawOp) : FsHafas.Client.Operator =
        let name = a.name.Trim()

        { defaultOperator with
            name = name
            id = slug name }
