namespace FsHafas.Parser

module internal Slug =

#if FABLE_COMPILER
    open Fable.Core
#endif

#if FABLE_JS
    [<ImportDefault("slugg")>]
    let slugify (x: string) : string = jsNative

#else
#if FABLE_PY
    // [<ImportMember("slugify")>]
    // let slugify (s:string) : string = nativeOnly

    let slugify (s: string) : string = s

#else
    open FSlugify.SlugGenerator

    let slugify (s: string) : string =
        FSlugify.SlugGenerator.slugify { DefaultSlugGeneratorOptions with Separator = '-' } s

#endif
#endif
