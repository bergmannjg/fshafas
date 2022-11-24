import type { HafasClient, Profile } from './hafas-client'

export const fshafas: FsHafas

export interface FsHafas {
    createClient: (profile: Profile) => HafasClient;
    setDebug: () => void;
}

