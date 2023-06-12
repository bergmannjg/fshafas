import type { HafasClient, Profile, Journeys, JourneysOptions } from './hafas-client'

export const fshafas: FsHafas

export interface FsHafas {
    createClient: (profile: Profile) => HafasClient;
    setDebug: () => void;
    bestprices:  (profile: Profile, from: string, to: string, options: JourneysOptions) => Promise<Journeys>;
}

