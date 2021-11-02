import type { HafasClient, Profile } from './hafas-client'

export function getProfile(profile: 'db' | 'bvg'): Profile;
export function createClient(profile: Profile): HafasClient;
export function setDebug(): void;


