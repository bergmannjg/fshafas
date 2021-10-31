namespace FsHafas.Parser

module internal Warning =

    open FsHafas.Client
    open FsHafas.Endpoint

    let private parseMsgEdge (ctx: Context) (e: FsHafas.Raw.RawHimMsgEdge) : FsHafas.Client.Edge =
        { fromLoc = Common.getElementAtSome e.fLocX ctx.common.locations
          toLoc = Common.getElementAtSome e.tLocX ctx.common.locations
          dir = e.dir
          icoCrd =
              Some
                  { x = e.icoCrd.x
                    y = e.icoCrd.y
                    ``type`` = None }
          icon = None }

    let private parseMsgEvent (ctx: Context) (e: FsHafas.Raw.RawHimMsgEvent) : FsHafas.Client.Event =
        { fromLoc = Common.getElementAtSome e.fLocX ctx.common.locations
          toLoc = Common.getElementAtSome e.tLocX ctx.common.locations
          start = ctx.profile.parseDateTime ctx e.fDate (Some e.fTime) None
          ``end`` = ctx.profile.parseDateTime ctx e.tDate (Some e.tTime) None
          sections = Some Array.empty }

    let private parseDateTime (ctx: Context) (date: string option) (time: string option) =
        match date with
        | Some date -> ctx.profile.parseDateTime ctx date time None
        | _ -> None

    let parseWarning (ctx: Context) (w: FsHafas.Raw.RawHim) : FsHafas.Client.Warning =

        let products =
            w.prod
            |> Option.map (fun pCls -> ctx.profile.parseBitmask ctx pCls)

        let categories =
            w.catRefL
            |> Common.mapIndexArray ctx.res.common (fun c -> c.himMsgCatL)
            |> Array.map (fun c -> c.id)

        let events =
            w.eventRefL
            |> Common.mapIndexArray ctx.res.common (fun c -> c.himMsgEventL)
            |> Array.map (parseMsgEvent ctx)
            |> Common.toOption

        let edges =
            w.edgeRefL
            |> Common.mapIndexArray ctx.res.common (fun c -> c.himMsgEdgeL)
            |> Array.map (parseMsgEdge ctx)
            |> Common.toOption

        let affectedLines =
            w.affProdRefL
            |> Common.mapIndexArray ctx.res.common (fun _ -> Some ctx.common.lines)
            |> Common.toOption

        let brToNewline (s: string option) =
            s |> Option.map (fun s -> s.Replace("<br>", "\n"))

        { Default.Warning with
              id = Some w.hid
              summary = brToNewline w.head
              text = brToNewline w.text
              company = w.comp
              categories = Some categories
              priority = Some w.prio
              validFrom = parseDateTime ctx w.sDate w.sTime
              validUntil = parseDateTime ctx w.eDate w.eTime
              modified = parseDateTime ctx w.lModDate w.lModTime
              products = products
              events = events
              edges = edges
              affectedLines = affectedLines }
