namespace FsHafas.Parser

module internal Line =

    open FsHafas.Client
    open FsHafas.Endpoint

    let private slug (s: string) = Slug.slugify s |> Some

    let private filterProducts (products: FsHafas.Client.Products) =
        let fields = products.Keys

        fields |> Seq.filter (fun kv -> products.[kv])

    let parseLine (ctx: Context) (p: FsHafas.Raw.RawProd) : FsHafas.Client.Line =
        let mutable line = Default.Line

        let name = p.addName |> Option.orElse (p.name)

        let id =
            match p.prodCtx with
            | Some (prodCtx) ->
                prodCtx.lineId
                |> Option.orElse name
                |> Option.bind slug
            | None ->
                match name with
                | Some name -> slug name
                | None -> None

        let (fahrtNr, adminCode, catOut) =
            match p.prodCtx with
            | Some (prodCtx) -> (prodCtx.num, prodCtx.admin, prodCtx.catOut)
            | None -> (None, None, None)

        let productName =
            match catOut with
            | Some catOut when catOut <> "" -> Some(catOut.Trim())
            | _ -> None

        let product =
            match p.cls with
            | Some cls -> filterProducts (ctx.profile.parseBitmask ctx cls)
            | None -> Seq.empty
            |> Seq.tryHead

        let (productid, mode) =
            match product with
            | Some kv ->
                match (ctx.profile :> FsHafas.Client.Profile).products
                      |> Array.tryFind (fun p -> p.id = kv)
                    with
                | Some (product) -> (Some product.id, Some product.mode)
                | None -> (None, None)
            | None -> (None, None)

        let operator = Common.getElementAtSome p.oprX ctx.common.operators

        { line with
            id = id
            fahrtNr = fahrtNr
            adminCode = adminCode
            name = name
            product = productid
            productName = productName
            mode = mode
            operator = operator
            ``public`` = Some true }
