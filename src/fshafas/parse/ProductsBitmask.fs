namespace FsHafas.Parser

module internal ProductsBitmask =

    open FsHafas
    open Client

    let parseBitmask (ctx: Context) (bitmask: int) : Client.Products =
        ctx.profile.products
        |> Array.fold
            (fun m p ->
                m.[p.id] <-
                    p.bitmasks
                    |> Array.exists (fun b -> b &&& bitmask <> 0)

                m)
            (Products(false))
