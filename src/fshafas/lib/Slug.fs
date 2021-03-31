namespace FsHafas.Parser

module internal Slug =

#if FABLE_COMPILER
    open Fable.Core

    [<ImportDefault("slugg")>]
    let slugify (x: string): string = jsNative

#else
    open FSlugify.SlugGenerator

    let slugify (s: string): string =
        FSlugify.SlugGenerator.slugify
            { DefaultSlugGeneratorOptions with
                  Separator = '-' }
            s

#endif
