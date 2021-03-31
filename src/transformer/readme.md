# Transform F# interface types to record types

Transform F# interface types from the ts2fable generated [file](https://github.com/bergmannjg/hafas-client-fable/blob/master/src/HafasClientTypes.fs) to record types in this [file](../Types-Hafas.fs).

## Example

The following part of a TypeScript declaration file

```js
interface Station {
    type: 'station';
    id: string;
    name: string;
    station?: Station;
    location?: Location;
    products?: Products;
    isMeta?: boolean;
    /** region ids */
    regions?: ReadonlyArray<string>;
    facilities?: Facilities;
    reisezentrumOpeningHours?: ReisezentrumOpeningHours;
}
```

is tranformed with [ts2fable](https://github.com/fable-compiler/ts2fable) to a F# interface type

```fsharp
type [<AllowNullLiteral>] Station =
    abstract ``type``: string with get, set
    abstract id: string option with get, set
    abstract name: string option with get, set
    abstract station: Station option with get, set
    abstract location: Location option with get, set
    abstract products: Products option with get, set
    abstract isMeta: bool option with get, set
    /// region ids
    abstract regions: ReadonlyArray<string> option with get, set
    abstract facilities: Facilities option with get, set
    abstract reisezentrumOpeningHours: ReisezentrumOpeningHours option with get, set
    abstract stops: ReadonlyArray<U3<Station, Stop, Location>> option with get, set
    abstract entrances: ReadonlyArray<Location> option with get, set
    abstract transitAuthority: string option with get, set
    abstract distance: float option with get, set
```

and this is tranformed with [transformer](./src/transformer) to the following F# record type

```fsharp
type Station =
    { ``type``: string option
      id: string option
      name: string option
      station: Station option
      location: Location option
      products: Products option
      isMeta: bool option
      /// region ids
      regions: array<string> option
      facilities: Facilities option
      reisezentrumOpeningHours: ReisezentrumOpeningHours option
      stops: array<U3<Station,Stop,Location>> option
      entrances: array<Location> option
      transitAuthority: string option
      distance: float option }
```