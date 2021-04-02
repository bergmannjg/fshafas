namespace FsHafas.Parser

module internal Common =

#if FABLE_COMPILER
    open Fable.Core
#endif

    open FsHafas

    let private updateOperators (ctx: Context) (ops: Client.Operator []) =
        { ctx with
              common = { ctx.common with operators = ops } }

    let private updateLines (ctx: Context) (lines: Client.Line []) =
        { ctx with
              common = { ctx.common with lines = lines } }

    let parseCommon (ctx: Context) (c: Raw.RawCommon) =
        let ctx1 =
            c.opL
            |> Array.map (fun op -> ctx.profile.parseOperator ctx op)
            |> updateOperators ctx

        let ctx2 =
            c.prodL
            |> Array.map (fun p -> ctx1.profile.parseLine ctx1 p)
            |> updateLines ctx

        let hints =
            c.remL
            |> Array.map (fun p -> ctx2.profile.parseHint ctx2 p)

        let locations = ctx2.profile.parseLocations ctx2 c.locL

        { operators = ctx2.common.operators
          locations = locations
          lines = ctx2.common.lines
          hints = hints }

    let getElementAt<'a> (index: int) (arr: 'a []) =
        if index < arr.Length then
            Some arr.[index]
        else
            None

    let getElementAtSome<'a> (index: int option) (arr: 'a []) =
        match index with
        | Some index when index < arr.Length -> Some arr.[index]
        | _ -> None

    let getArray<'a> (common: Raw.RawCommon option) (getter: Raw.RawCommon -> 'a [] option) =
        match common with
        | Some common ->
            match getter common with
            | Some arr -> arr
            | None -> Array.empty
        | None -> Array.empty

    /// map index array to elements of array from RawCommon 
    let mapIndexArray<'a>
        (common: Raw.RawCommon option)
        (getTargetArray: Raw.RawCommon -> 'a [] option)
        (indexArr: int [] option)
        =
        let elements = getArray common getTargetArray

        match indexArr with
        | Some arr ->
            arr
            |> Array.map (fun i -> getElementAt i elements)
            |> Array.choose id
        | None -> Array.empty

    /// map array with index field to elements of target array
    let mapArray<'a, 'b>
        (targetArray: 'b [])
        (getIndex: 'a -> int option)
        (sourceArray: 'a [] option)
        =
        let elements = targetArray

        match sourceArray with
        | Some arr ->
            arr
            |> Array.map (fun elt -> getElementAtSome (getIndex elt) elements)
            |> Array.choose id
        | None -> Array.empty

    let toOption<'a> (arr: 'a []) =
        if arr.Length > 0 then
            Some arr
        else
            None

    let msgLToRemarks (ctx: Context) (msgL: Raw.RawMsg [] option) =
        msgL
        |> mapArray ctx.common.hints (fun x -> x.remX)
        |> Array.choose id
        |> toOption
