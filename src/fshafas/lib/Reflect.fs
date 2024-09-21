namespace FsHafas.Reflect

/// <exclude>Compare</exclude>
module Traverse =

    open System
    open System.Reflection
    open FSharp.Reflection

    type TraverseEvent =
        { onRecordFieldname: int -> string -> Type -> unit
          onCaseInfo: int -> UnionCaseInfo -> unit
          onField: int -> string -> obj -> unit
          onUnkownType: int -> Type -> string -> obj -> unit
          onEmptyArray: int -> string -> unit
          onMapField: int -> string -> Map<string, bool> -> unit }

    let private isBasicType (ty: Type) = ty.IsPrimitive || ty.Name = "String"

    let rec private recordField (evt: TraverseEvent) (depth: int) (o: obj) (info: PropertyInfo) =
        if info.CanRead then
            common evt depth info.Name (info.GetValue(o))

    and private option (evt: TraverseEvent) (depth: int) (name: string) (o: obj) =
        match FSharpValue.GetUnionFields(o, o.GetType()) with
        | case, o ->
            evt.onCaseInfo depth case
            common evt depth name o

    and private map (evt: TraverseEvent) (depth: int) (name: string) (o: obj) =
        let typ = o.GetType()

        if
            typ.GenericTypeArguments.[0].Name = typeof<String>.Name
            && typ.GenericTypeArguments.[1].Name = typeof<Boolean>.Name
        then
            let map = o :?> Map<string, bool>
            evt.onMapField depth name map
        else
            fprintfn stderr "%s" (sprintf "map: unkown type %s" typ.FullName)

    and private indexmap (evt: TraverseEvent) (depth: int) (name: string) (o: obj) =
        let typ = o.GetType()

        if
            typ.GenericTypeArguments.[0].Name = typeof<String>.Name
            && typ.GenericTypeArguments.[1].Name = typeof<Boolean>.Name
        then
            let map = o :?> FsHafas.Client.IndexMap<string, bool>

            map.Keys
            |> Seq.fold (fun (m: Map<_, _>) k -> m.Add(k, map.[k])) Map.empty
            |> evt.onMapField depth name
        else
            fprintfn stderr "%s" (sprintf "indexmap: unkown type %s" typ.FullName)

    and private record (evt: TraverseEvent) (depth: int) (name: string) (o: obj) =
        let typ = o.GetType()

        if name.Length > 0 then
            evt.onRecordFieldname depth name typ

        FSharpType.GetRecordFields(typ) |> Array.iter (recordField evt (depth + 1) o)

    and private union (evt: TraverseEvent) (depth: int) (name: string) (o: obj) =
        let typ = o.GetType()

        match FSharpValue.GetUnionFields(o, typ) with
        | case, v ->
            evt.onCaseInfo depth case

            if v.Length > 0 then
                common evt (depth + 1) name v.[0]
            else
                evt.onField depth name o

    and private common (evt: TraverseEvent) (depth: int) (name: string) (o: obj) =
        if not (isNull o) then
            let typ = o.GetType()

            if isBasicType typ then
                evt.onField depth name o
            else if

                typ.Name = typeof<Option<_>>.Name
            then
                option evt depth name o
            else if

                FSharpType.IsRecord(typ)
            then
                record evt depth name o
            else if

                FSharpType.IsUnion(typ)
            then
                union evt depth name o
            else if

                typ.IsArray && typ.Name = typeof<double[]>.Name
            then
                let arr = o :?> double[]

                arr |> Array.iter (fun e -> common evt depth name e)

                if arr.Length = 0 then
                    evt.onEmptyArray depth name
            else if

                typ.IsArray && typ.Name = typeof<int[]>.Name
            then
                let arr = o :?> int[]

                arr |> Array.iter (fun e -> common evt depth name e)

                if arr.Length = 0 then
                    evt.onEmptyArray depth name
            else if

                typ.IsArray
            then
                let arr = o :?> obj[]

                arr |> Array.iter (fun e -> common evt depth name e)

                if arr.Length = 0 then
                    evt.onEmptyArray depth name
            else if

                typ.Name = "FSharpMap`2"
            then
                map evt depth name o
            else if

                typ.Name = typeof<FsHafas.Client.IndexMap<_, _>>.Name
            then
                indexmap evt depth name o
            else
                evt.onUnkownType depth typ name o
        else
            evt.onField depth name o

    let traverse (evt: TraverseEvent) (o: obj) = common evt 0 "" o

/// <exclude>Compare</exclude>
module Compare =

    open System
    open System.Reflection
    open FSharp.Reflection

    type CompareEvent =
        { onTypesDifferent: string -> obj -> Type -> obj -> Type -> unit
          onValuesDifferent: string -> obj -> obj -> unit }

    let private isBasicType (ty: Type) = ty.IsPrimitive || ty.Name = "String"

    let rec private recordField (evt: CompareEvent) (depth: int) (o1: obj) (o2: obj) (info: PropertyInfo) =
        if info.CanRead then
            common evt depth info.Name (info.GetValue(o1)) (info.GetValue(o2))

    and private option (evt: CompareEvent) (depth: int) (name: string) (o1: obj) (o2: obj) =
        match FSharpValue.GetUnionFields(o1, o1.GetType()) with
        | case1, v1 ->
            match FSharpValue.GetUnionFields(o2, o2.GetType()) with
            | case2, v2 ->
                if case1.Name <> case2.Name then
                    evt.onValuesDifferent name o1 o2
                else
                    common evt depth name v1 v2

    and private map (evt: CompareEvent) (depth: int) (name: string) (o1: obj) (o2: obj) =
        let typ = o1.GetType()

        if
            typ.GenericTypeArguments.[0].Name = typeof<String>.Name
            && typ.GenericTypeArguments.[1].Name = typeof<Boolean>.Name
        then
            let map1 = o1 :?> Map<string, bool>
            let map2 = o2 :?> Map<string, bool>

            if map1.Count <> map2.Count then
                evt.onValuesDifferent name o1 o2
            else
                map2
                |> Seq.iter2
                    (fun kv1 kv2 ->
                        if kv1 <> kv2 then
                            evt.onValuesDifferent name o1 o2)
                    map1
        else
            fprintfn stderr "%s" (sprintf "map: unkown type %s" typ.FullName)

    and private indexmap (evt: CompareEvent) (depth: int) (name: string) (o1: obj) (o2: obj) =
        let typ = o1.GetType()

        if
            typ.GenericTypeArguments.[0].Name = typeof<String>.Name
            && typ.GenericTypeArguments.[1].Name = typeof<Boolean>.Name
        then
            let map1 = o1 :?> FsHafas.Client.IndexMap<string, bool>

            let map2 = o2 :?> FsHafas.Client.IndexMap<string, bool>

            if map1.Keys.Length <> map2.Keys.Length then
                evt.onValuesDifferent name o1 o2
            else
                map2.Keys
                |> Seq.iter2
                    (fun kv1 kv2 ->
                        if kv1 <> kv2 || map1.[kv1] <> map2.[kv2] then
                            evt.onValuesDifferent name ((kv1, map1.[kv1]) :> obj) ((kv2, map2.[kv2]) :> obj))
                    map1.Keys
        else
            fprintfn stderr "%s" (sprintf "indexmap: unkown type %s" typ.FullName)

    and private record (evt: CompareEvent) (depth: int) (name: string) (o1: obj) (o2: obj) =
        let typ = o1.GetType()

        FSharpType.GetRecordFields(typ)
        |> Array.iter (recordField evt (depth + 1) o1 o2)

    and private union (evt: CompareEvent) (depth: int) (name: string) (o1: obj) (o2: obj) =
        let typ = o1.GetType()

        match FSharpValue.GetUnionFields(o1, typ) with
        | case1, v1 ->
            match FSharpValue.GetUnionFields(o2, typ) with
            | case2, v2 ->
                if case1.Name <> case2.Name then
                    evt.onValuesDifferent name o1 o2
                else if v1.Length > 0 && v2.Length > 0 then
                    common evt (depth + 1) name v1.[0] v2.[0]
                else if v1.Length <> v2.Length then
                    evt.onValuesDifferent name o1 o2

    and private common (evt: CompareEvent) (depth: int) (name: string) (o1: obj) (o2: obj) =
        if not (isNull o1) && not (isNull o2) then
            let typ1 = o1.GetType()
            let typ2 = o2.GetType()

            if typ1 <> typ2 then
                evt.onTypesDifferent name o1 typ1 o2 typ2
            else if

                isBasicType typ1
            then
                if o1 <> o2 then
                    evt.onValuesDifferent name o1 o2
            else if

                typ1.Name = typeof<Option<_>>.Name
            then
                option evt depth name o1 o2
            else if

                FSharpType.IsRecord(typ1)
            then
                record evt depth name o1 o2
            else if

                FSharpType.IsUnion(typ1)
            then
                union evt depth name o1 o2
            else if

                typ1.IsArray && typ1.Name = typeof<double[]>.Name
            then
                let arr1 = o1 :?> double[]
                let arr2 = o2 :?> double[]

                if arr1.Length = arr2.Length then
                    arr2 |> Array.iter2 (fun e1 e2 -> common evt depth name e1 e2) arr1
                else
                    evt.onValuesDifferent name o1 o2
            else if

                typ1.IsArray && typ1.Name = typeof<int[]>.Name
            then
                let arr1 = o1 :?> int[]
                let arr2 = o2 :?> int[]

                if arr1.Length = arr2.Length then
                    arr2 |> Array.iter2 (fun e1 e2 -> common evt depth name e1 e2) arr1
                else
                    evt.onValuesDifferent name o1 o2
            else if

                typ1.IsArray
            then
                let arr1 = o1 :?> obj[]
                let arr2 = o2 :?> obj[]

                if arr1.Length = arr2.Length then
                    arr2 |> Array.iter2 (fun e1 e2 -> common evt depth name e1 e2) arr1
                else
                    evt.onValuesDifferent name o1 o2
            else if

                typ1.Name = "FSharpMap`2"
            then
                map evt depth name o1 o2
            else if

                typ1.Name = typeof<FsHafas.Client.IndexMap<_, _>>.Name
            then
                indexmap evt depth name o1 o2
            else
                fprintfn stderr "%s" (sprintf "common: unkown type %s" typ1.FullName)
        else if o1 <> o2 then
            evt.onValuesDifferent name o1 o2

    let compare (evt: CompareEvent) (o1: obj) (o2: obj) = common evt 0 "" o1 o2
