namespace FsHafas.Extensions

module internal ConvertEx =

#if FABLE_JS
    open Fable.Core

    [<Emit("Buffer.from($0, 'hex')")>]
    let FromHexString (s: string) = [||]
#else
#if FABLE_PY
    open Fable.Core

    [<Emit("bytes.fromhex($0)")>]
    let FromHexString (s: string) : byte[] = jsNative
#else
    let FromHexString (s: string) = System.Convert.FromHexString s
#endif
#endif
