namespace FsHafas.Client

#if !FABLE_COMPILER

type U2<'a, 'b> =
    | Case1 of 'a
    | Case2 of 'b

type U3<'a, 'b, 'c> =
    | Case1 of 'a
    | Case2 of 'b
    | Case3 of 'c


type U4<'a, 'b, 'c, 'd> =
    | Case1 of 'a
    | Case2 of 'b
    | Case3 of 'c
    | Case4 of 'd

type U5<'a, 'b, 'c, 'd, 'e> =
    | Case1 of 'a
    | Case2 of 'b
    | Case3 of 'c
    | Case4 of 'd
    | Case5 of 'e

#endif

#if FABLE_COMPILER
open Fable.Core

type U2<'a, 'b> = Fable.Core.U2<'a, 'b>

type U3<'a, 'b, 'c> = Fable.Core.U3<'a, 'b, 'c>

type U4<'a, 'b, 'c, 'd> = Fable.Core.U4<'a, 'b, 'c, 'd>

type U5<'a, 'b, 'c, 'd, 'e> = Fable.Core.U5<'a, 'b, 'c, 'd, 'e>
#endif

type U14<'a, 'b, 'c, 'd, 'e, 'f, 'g, 'h, 'i, 'j, 'k, 'l, 'm, 'n> =
    | Case1 of 'a
    | Case2 of 'b
    | Case3 of 'c
    | Case4 of 'd
    | Case5 of 'e
    | Case6 of 'f
    | Case7 of 'g
    | Case8 of 'h
    | Case9 of 'i
    | Case10 of 'j
    | Case11 of 'k
    | Case12 of 'l
    | Case13 of 'm
    | Case14 of 'n
