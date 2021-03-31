namespace FsHafas.Parser

module internal Operator =

    open FsHafas

    let private slug (s: string) = Slug.slugify s

    let defaultOperator: Client.Operator =
        { ``type`` = Some "operator"
          id = ""
          name = "" }

    let parseOperator (ctx: Context) (a: Raw.RawOp): Client.Operator =
        let name = a.name.Trim()

        { defaultOperator with
              name = name
              id = slug name }
