namespace FsHafas.Extensions

module internal ArrayEx =

#if FABLE_PY
    open Fable.Core

    // workaround: error in Array.sortBy
    [<Emit("sorted($1, key=$0)")>]
    let sortBy (key: 'a -> 'b) (arr: array<'a>) : array<'a> = jsNative
#else
    let sortBy (key: 'a -> 'b) (arr: array<'a>) : array<'a> = Array.sortBy key arr
#endif
