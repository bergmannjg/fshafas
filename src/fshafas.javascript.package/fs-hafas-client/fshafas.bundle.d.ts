import type { HafasClient, ProductType, Profile } from './hafas-client'

export const fshafas: FsHafas

export interface FsHafas {
    dbProfile: Profile;
    bvgProfile: Profile;
    getProfile: (profile: 'db' | 'bvg') => Profile;
    createClient: (profile: Profile) => HafasClient;
    setDebug: () => void;
}

