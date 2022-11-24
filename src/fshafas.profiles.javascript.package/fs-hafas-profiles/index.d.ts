import type { Profile } from './hafas-client'

export const profiles: Profiles

export interface Profiles {
    getProfile: (profile: string) => Profile;
}

