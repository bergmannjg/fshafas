import { fshafas } from 'fs-hafas-client';
import { profiles } from 'fs-hafas-profiles';

export function createClient(profile) {
    return fshafas.createClient(profiles.getProfile(profile));
}