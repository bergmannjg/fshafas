namespace FsHafas.Parser

module internal ProductsBitmask =

    open FsHafas.Client
    open FsHafas.Endpoint

    let parseBitmask (ctx: Context) (bitmask: int) : FsHafas.Client.Products =
        (ctx.profile :> FsHafas.Client.Profile).products
        |> Array.fold
            (fun m p ->
                m.[p.id] <- p.bitmasks |> Array.exists (fun b -> b &&& bitmask <> 0)

                m)
            (Products(false))
