namespace FsHafas.Parser

module internal Line =

    open FsHafas

    let private slug (s: string) = Slug.slugify s |> Some

    let private filterProducts (products: Client.Products) =
        let fields = products.Keys

        fields |> Seq.filter (fun kv -> products.[kv])

    let parseLine (ctx: Context) (p: Raw.RawProd) : Client.Line =
        let mutable line = Default.Line

        let name = p.addName |> Option.orElse (Some p.name)

        let (id, fahrtNr, adminCode) =
            match p.prodCtx with
            | Some (prodCtx) ->
                (prodCtx.lineId
                 |> Option.orElse name
                 |> Option.bind slug,
                 prodCtx.num,
                 prodCtx.admin)
            | None -> (None, None, None)

        let product =
            match p.cls with
            | Some cls -> filterProducts (ctx.profile.parseBitmask ctx cls)
            | None -> Seq.empty
            |> Seq.tryHead

        let (productid, mode) =
            match product with
            | Some kv ->
                match ctx.profile.products
                      |> Array.tryFind (fun p -> p.id = kv) with
                | Some (product) -> (Some product.id, Some product.mode)
                | None -> (None, None)
            | None -> (None, None)

        let operator =
            Common.getElementAtSome p.oprX ctx.common.operators

        { line with
              id = id
              fahrtNr = fahrtNr
              adminCode = adminCode
              name = name
              product = productid
              mode = mode
              operator = operator
              ``public`` = Some true }
