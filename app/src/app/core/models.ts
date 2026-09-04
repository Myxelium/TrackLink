export interface MemberSummary {
  id: number;
  userIdentifier: string;
  username: string;
  fullname: string | null;
  image: string | null;
  email: string | null;
}

export interface BandSummary {
  id: number;
  name: string;
  genre: string | null;
  image: string | null;
  driveFolderId: string | null;
  driveFolderName: string | null;
  myRole: string | null;
  isOwner: boolean;
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
  email: string | null;
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
  signedIn: boolean;
  connected: boolean;
  email: string | null;
  member: Member | null;
}

export interface DriveFile {
  id: string;
  name: string;
  mimeType: string | null;
}

export interface Invite {
  id: number;
  email: string;
  roleName: string;
  code: string;
  acceptUrl: string;
  emailSent: boolean;
  expiresAt: string;
}

export interface AcceptInviteResult {
  accepted: boolean;
  needsLogin: boolean;
  loginUrl: string | null;
  error: string | null;
  bandId: number | null;
}

export interface PickerToken {
  accessToken: string;
  clientId: string;
  apiKey: string | null;
}

export interface AlbumSummary {
  id: number;
  bandId: number;
  name: string;
  archived: boolean;
  approvalRule: string;
  trackCount: number;
  openProposalCount: number;
}

export interface AlbumTrack {
  id: number;
  songId: number;
  songName: string;
  version: number | null;
  sortOrder: number;
  addedAt: string;
}

export interface ProposalDecision {
  memberId: number;
  memberName: string;
  decision: string;
  updatedAt: string;
}

export interface WaitingMember {
  memberId: number;
  name: string;
}

export interface ProposalReview {
  id: number;
  memberId: number;
  memberName: string;
  startMs: number;
  endMs: number;
  body: string;
  createdAt: string;
}

export interface AlbumProposal {
  id: number;
  albumId: number;
  songId: number;
  songName: string;
  songVersion: number | null;
  proposedBy: number;
  proposedByName: string;
  status: string;
  createdAt: string;
  resolvedAt: string | null;
  decisions: ProposalDecision[];
  waitingOn: WaitingMember[];
  reviews: ProposalReview[];
}

export interface AlbumDetail {
  id: number;
  bandId: number;
  name: string;
  archived: boolean;
  approvalRule: string;
  tracks: AlbumTrack[];
  proposals: AlbumProposal[];
}
