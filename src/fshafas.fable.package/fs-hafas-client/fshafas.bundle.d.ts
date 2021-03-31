import type { HafasClient, Profile } from 'hafas-client'

export const fshafas: FsHafas

export interface FsHafas {
    getProfile: (profile: 'db' | 'bvg') => Profile;
    createClient: (profile: 'db' | 'bvg') => HafasClient;
    setDebug: () => void;
}

