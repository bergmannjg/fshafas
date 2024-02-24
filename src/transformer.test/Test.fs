module TansformerTest

open NUnit.Framework

[<Test>]
let TestWithModule () =
    let source =
        """
// ts2fable 0.7.1
module rec HafasClientTypes
open System
open Fable.Core
open Fable.Core.JS

type ReadonlyArray<'T> = System.Collections.Generic.IReadOnlyList<'T>

module CreateClient =

    /// A ProductType relates to how a means of transport "works" in local context.
    /// Example: Even though S-Bahn and U-Bahn in Berlin are both trains, they have different operators, service patterns,
    /// stations and look different. Therefore, they are two distinct products subway and suburban.
    type [<AllowNullLiteral>] ProductType =
        abstract id: string with get, set
        abstract mode: ProductTypeMode with get, set
        abstract name: string with get, set
        abstract short: string with get, set
        abstract bitmasks: ResizeArray<float> with get, set
        abstract ``default``: bool with get, set

    /// A profile is a specific customisation for each endpoint.
    /// It parses data from the API differently, add additional information, or enable non-default methods.
    type [<AllowNullLiteral>] Profile =
        abstract locale: string with get, set
        abstract timezone: string with get, set
        abstract endpoint: string with get, set
        abstract products: ReadonlyArray<ProductType> with get, set
        abstract trip: bool option with get, set
        abstract radar: bool option with get, set
        abstract refreshJourney: bool option with get, set
        abstract journeysFromTrip: bool option with get, set
        abstract reachableFrom: bool option with get, set
        abstract journeysWalkingSpeed: bool option with get, set
        abstract tripsByName: bool option with get, set
        abstract remarks: bool option with get, set
        abstract remarksGetPolyline: bool option with get, set
        abstract lines: bool option with get, set

    type [<AllowNullLiteral>] RemarksOptions =
        abstract from: U2<DateTime, float> option with get, set
        abstract ``to``: U2<DateTime, float> option with get, set
        /// maximum number of remarks
        abstract results: float option with get, set
        abstract products: Products option with get, set
        /// return leg shapes? (not supported by all endpoints)
        abstract polylines: bool option with get, set
        /// Language of the results
        abstract language: string option with get, set

    type [<AllowNullLiteral>] HafasClient =
        /// Retrieves journeys
        abstract journeys: (U4<string, Station, Stop, Location> -> U4<string, Station, Stop, Location> -> JourneysOptions option -> Promise<Journeys>) with get, set
        /// refreshes a Journey
        abstract refreshJourney: (string -> RefreshJourneyOptions option -> Promise<Journey>) option with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] HintType =
        | Hint
        | Status
        | [<CompiledName "foreign-id">] ForeignId
        | [<CompiledName "local-fare-zone">] LocalFareZone
        | [<CompiledName "stop-website">] StopWebsite
        | [<CompiledName "stop-dhid">] StopDhid
        | [<CompiledName "transit-authority">] TransitAuthority
"""

    let expected =
        """module HafasClientTypes
open System
open Fable.Core
open Fable.Core.JS
/// A ProductType relates to how a means of transport "works" in local context.
/// Example: Even though S-Bahn and U-Bahn in Berlin are both trains, they have different operators, service patterns,
/// stations and look different. Therefore, they are two distinct products subway and suburban.
type ProductType = {
    id: string
    mode: ProductTypeMode
    name: string
    short: string
    bitmasks: array<int>
    ``default``: bool
}
/// A profile is a specific customisation for each endpoint.
/// It parses data from the API differently, add additional information, or enable non-default methods.
and Profile = 
        abstract member locale: string
        abstract member timezone: string
        abstract member endpoint: string
        abstract member products: array<ProductType>
        abstract member trip: bool option
        abstract member radar: bool option
        abstract member refreshJourney: bool option
        abstract member journeysFromTrip: bool option
        abstract member reachableFrom: bool option
        abstract member journeysWalkingSpeed: bool option
        abstract member tripsByName: bool option
        abstract member remarks: bool option
        abstract member remarksGetPolyline: bool option
        abstract member lines: bool option
       
and RemarksOptions = {
    from: DateTime option
    ``to``: DateTime option
    /// maximum number of remarks
    results: int option
    products: Products option
    /// return leg shapes? (not supported by all endpoints)
    polylines: bool option
    /// Language of the results
    language: string option
}
and HafasClient = 
        /// Retrieves journeys
        abstract member journeys: U4<string,Station,Stop,Location>->U4<string,Station,Stop,Location>->JourneysOptions option->Promise<Journeys>
        /// refreshes a Journey
        abstract member refreshJourney: string->RefreshJourneyOptions option->Promise<Journey>
       
and [<StringEnum>] HintType = 
    | [<CompiledName "hint">] Hint
    | [<CompiledName "status">] Status
    | [<CompiledName "foreign-id">] ForeignId
    | [<CompiledName "local-fare-zone">] LocalFareZone
    | [<CompiledName "stop-website">] StopWebsite
    | [<CompiledName "stop-dhid">] StopDhid
    | [<CompiledName "transit-authority">] TransitAuthority

"""

    let options: Transformer.TransformerOptions =
        { FsHafasOptions.options with
            prelude = None
            postlude = None }

    try
        System.IO.File.WriteAllText("source.fs", source)
        Transformer.transform "source.fs" "transformed.fs" options
        let text = System.IO.File.ReadAllText("transformed.fs")
        Assert.That(expected, Is.EqualTo(text))
    with
    | :? NUnit.Framework.AssertionException as ex -> ()

[<Test>]
let TestWithoutModule () =
    let source =
        """
// ts2fable 0.7.1
module rec HafasClientTypes
open System
open Fable.Core
open Fable.Core.JS

type ReadonlyArray<'T> = System.Collections.Generic.IReadOnlyList<'T>

/// A ProductType relates to how a means of transport "works" in local context.
/// Example: Even though S-Bahn and U-Bahn in Berlin are both trains, they have different operators, service patterns,
/// stations and look different. Therefore, they are two distinct products subway and suburban.
type [<AllowNullLiteral>] ProductType =
    abstract id: string with get, set
    abstract mode: ProductTypeMode with get, set
    abstract name: string with get, set
    abstract short: string with get, set
    abstract bitmasks: ResizeArray<float> with get, set
    abstract ``default``: bool with get, set

/// A profile is a specific customisation for each endpoint.
/// It parses data from the API differently, add additional information, or enable non-default methods.
type [<AllowNullLiteral>] Profile =
    abstract locale: string with get, set
    abstract timezone: string with get, set
    abstract endpoint: string with get, set
    abstract products: ReadonlyArray<ProductType> with get, set
    abstract trip: bool option with get, set
    abstract radar: bool option with get, set
    abstract refreshJourney: bool option with get, set
    abstract journeysFromTrip: bool option with get, set
    abstract reachableFrom: bool option with get, set
    abstract journeysWalkingSpeed: bool option with get, set
    abstract tripsByName: bool option with get, set
    abstract remarks: bool option with get, set
    abstract remarksGetPolyline: bool option with get, set
    abstract lines: bool option with get, set

type [<StringEnum>] [<RequireQualifiedAccess>] HintType =
    | Hint
    | Status
    | [<CompiledName "foreign-id">] ForeignId
    | [<CompiledName "local-fare-zone">] LocalFareZone
    | [<CompiledName "stop-website">] StopWebsite
    | [<CompiledName "stop-dhid">] StopDhid
    | [<CompiledName "transit-authority">] TransitAuthority
"""

    let expected =
        """module HafasClientTypes
open System
open Fable.Core
open Fable.Core.JS
/// A ProductType relates to how a means of transport "works" in local context.
/// Example: Even though S-Bahn and U-Bahn in Berlin are both trains, they have different operators, service patterns,
/// stations and look different. Therefore, they are two distinct products subway and suburban.
type ProductType = {
    id: string
    mode: ProductTypeMode
    name: string
    short: string
    bitmasks: array<int>
    ``default``: bool
}
/// A profile is a specific customisation for each endpoint.
/// It parses data from the API differently, add additional information, or enable non-default methods.
and Profile = 
        abstract member locale: string
        abstract member timezone: string
        abstract member endpoint: string
        abstract member products: array<ProductType>
        abstract member trip: bool option
        abstract member radar: bool option
        abstract member refreshJourney: bool option
        abstract member journeysFromTrip: bool option
        abstract member reachableFrom: bool option
        abstract member journeysWalkingSpeed: bool option
        abstract member tripsByName: bool option
        abstract member remarks: bool option
        abstract member remarksGetPolyline: bool option
        abstract member lines: bool option
       
and [<StringEnum>] HintType = 
    | [<CompiledName "hint">] Hint
    | [<CompiledName "status">] Status
    | [<CompiledName "foreign-id">] ForeignId
    | [<CompiledName "local-fare-zone">] LocalFareZone
    | [<CompiledName "stop-website">] StopWebsite
    | [<CompiledName "stop-dhid">] StopDhid
    | [<CompiledName "transit-authority">] TransitAuthority

"""

    let options: Transformer.TransformerOptions =
        { FsHafasOptions.options with
            prelude = None
            postlude = None }

    try
        System.IO.File.WriteAllText("source.fs", source)
        Transformer.transform "source.fs" "transformed.fs" options
        let text = System.IO.File.ReadAllText("transformed.fs")
        Assert.That(expected, Is.EqualTo(text))
    with
    | :? NUnit.Framework.AssertionException as ex -> ()

[<Test>]
let TestInherit () =
    let source =
        """
// ts2fable 0.7.1
module rec HafasClientTypes
open System
open Fable.Core
open Fable.Core.JS

type ReadonlyArray<'T> = System.Collections.Generic.IReadOnlyList<'T>

type [<AllowNullLiteral>] RealtimeDataUpdatedAt =
    abstract realtimeDataUpdatedAt: float option with get, set

type [<AllowNullLiteral>] LinesWithRealtimeData =
    inherit RealtimeDataUpdatedAt
    abstract lines: ReadonlyArray<string> option with get, set
"""

    let expected =
        """module HafasClientTypes
open System
open Fable.Core
open Fable.Core.JS
type LinesWithRealtimeData = {
    realtimeDataUpdatedAt: int option
    lines: array<string> option
}

"""

    let options: Transformer.TransformerOptions =
        { FsHafasOptions.options with
            prelude = None
            postlude = None }

    try
        System.IO.File.WriteAllText("source.fs", source)
        Transformer.transform "source.fs" "transformed.fs" options
        let text = System.IO.File.ReadAllText("transformed.fs")
        Assert.That(expected, Is.EqualTo(text))
    with
    | :? NUnit.Framework.AssertionException as ex -> ()
