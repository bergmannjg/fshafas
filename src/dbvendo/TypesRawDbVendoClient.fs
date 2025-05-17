namespace DbVendo.Raw

open FsHafas.Client

/// <namespacedoc>
///   <summary>Types of raw db vendo api</summary>
/// </namespacedoc>

type LocationsRequestBody =
    { locationTypes: array<string>
      searchTerm: string
      maxResults: int }

type Position =
    { latitude: float option
      longitude: float option }

type Location =
    { name: string option
      stationId: string option
      locationId: string option
      evaNr: string option
      position: Position option
      weight: int option
      products: array<string> option
      locationType: string option }

type LocationsResponse = Location array

type Reisende =
    { ermaessigungen: string array
      reisendenTyp: string }

type ReisendenProfil = { reisende: Reisende array }

type Fahrverguenstigungen =
    { deutschlandTicketVorhanden: bool
      nurDeutschlandTicketVerbindungen: bool }

type ZeitWunsch =
    { reiseDatum: System.DateTime
      zeitPunktArt: string }

type Wunsch =
    { abgangsLocationId: string
      verkehrsmittel: string array
      zeitWunsch: ZeitWunsch
      zielLocationId: string
      minUmstiegsdauer: int option
      maxUmstiege: int option
      fahrradmitnahme: bool }

type ReiseHin = { wunsch: Wunsch }

type JourneysRequestBody =
    { autonomeReservierung: bool
      einstiegsTypList: string array
      fahrverguenstigungen: Fahrverguenstigungen
      klasse: string
      reisendenProfil: ReisendenProfil
      reservierungsKontingenteVorhanden: bool
      reiseHin: ReiseHin }

type VerbindungHin = { kontext: string }

type RefreshJourneysRequestBody =
    { autonomeReservierung: bool
      einstiegsTypList: string array
      fahrverguenstigungen: Fahrverguenstigungen
      klasse: string
      reisendenProfil: ReisendenProfil
      reservierungsKontingenteVorhanden: bool
      verbindungHin: VerbindungHin }

type StationBoardRequestBody =
    { anfragezeit: string
      datum: string
      ursprungsBahnhofId: string
      verkehrsmittel: string array }

type Coordinates = { latitude: float; longitude: float }

type Area =
    { coordinates: Coordinates
      radius: int }

type NearByRequestBody =
    { area: Area
      maxResults: int
      products: string array }

type RequestBody =
    U5<LocationsRequestBody, JourneysRequestBody, RefreshJourneysRequestBody, StationBoardRequestBody, NearByRequestBody>

type RequestData =
    | Post of (string * RequestBody)
    | Get of string

type Request =
    { endpoint: string
      data: RequestData
      xCorrelationID: string
      accept: string }

type AuslastungsInfo =
    { klasse: string option
      stufe: int option
      anzeigeTextKurz: string option }

type EchtzeitNotiz = { text: string option }

type Halt =
    { abgangsDatum: string option
      ezAbgangsDatum: string option
      ankunftsDatum: string option
      ezAnkunftsDatum: string option
      ort: Location option
      gleis: string option
      plattform: string option
      auslastungsInfos: AuslastungsInfo array
      echtzeitNotizen: EchtzeitNotiz array }

type PolylineDesc = { coordinates: Coordinates array }

type PolylineGroup = { polylineDesc: PolylineDesc array }

type BahnhofstafelPosition =
    { kurztext: string option
      mitteltext: string option
      langtext: string option
      produktGattung: string option
      zuglaufId: string option
      halte: Halt array option
      abfrageOrt: Location option
      abgangsOrt: Location option
      abgangsDatum: string option
      ezAbgangsDatum: string option
      ankunftsOrt: Location option
      ankunftsDatum: string option
      ezAnkunftsDatum: string option
      auslastungsInfos: AuslastungsInfo array option
      richtung: string option
      risZuglaufId: string option
      gleis: string option
      ezGleis: string option }

type VerbindungsAbschnitt =
    { kurztext: string option
      mitteltext: string option
      langtext: string option
      zuglaufId: string option
      halte: Halt array option
      abgangsOrt: Location option
      abgangsDatum: string option
      ezAbgangsDatum: string option
      ankunftsOrt: Location option
      ankunftsDatum: string option
      ezAnkunftsDatum: string option
      auslastungsInfos: AuslastungsInfo array option
      richtung: string option
      verkehrsmittelNummer: string option
      produktGattung: string option
      risZuglaufId: string option
      typ: string option
      polylineGroup: PolylineGroup option }

type Verbindung =
    { reiseDauer: int option
      schemaVersion: string option
      schemaName: string option
      kontext: string option
      auslastungsInfos: AuslastungsInfo array
      verbindungsAbschnitte: VerbindungsAbschnitt array option }

type Betrag = { waehrung: string; betrag: float }

type GesamtPreis = { ab: Betrag; klasse: string }

type Preise =
    { istTeilpreis: bool
      hinRueckPauschalpreis: bool
      gesamt: GesamtPreis option }

type Angebote =
    { angebotsMeldungen: string array
      preise: Preise option }

type VerbindungEx =
    { verbindung: Verbindung
      angebote: Angebote option }

type JourneysResponse =
    { verbindungen: VerbindungEx array
      frueherContext: string option
      spaeterContext: string option }

type StationBoardResponse =
    { bahnhofstafelAbfahrtPositionen: BahnhofstafelPosition array option
      bahnhofstafelAnkunftPositionen: BahnhofstafelPosition array option }

type Produkt = { name: string }

type ProduktGattung =
    { produktGattung: string
      produkte: Produkt array }

type StopResponse =
    { haltName: string
      produktGattungen: ProduktGattung array }
