export interface MemberSummary {
  id: number;
  userIdentifier: string;
  username: string;
  fullname: string | null;
  image: string | null;
}

export interface BandSummary {
  id: number;
  name: string;
  genre: string | null;
  image: string | null;
}

export interface RoleSummary {
  id: number;
  roleName: string;
  bandId: number;
}

export interface Member {
  id: number;
  userIdentifier: string;
  username: string;
  fullname: string | null;
  image: string | null;
  bands: BandSummary[];
  roles: RoleSummary[];
}

export interface Song {
  id: number;
  name: string;
  description: string | null;
  uploadedBy: number;
  version: number | null;
  previousVersion: number;
  storageKind: 'gdrive' | 'url' | string;
}

export interface GoogleStatus {
  configured: boolean;
  connected: boolean;
  email: string | null;
}
