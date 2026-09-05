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
  contentMd5?: string | null;
  sourceModifiedAt?: string | null;
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

export interface InclusionTally {
  myChoice: string | null;
  inCount: number;
  outCount: number;
  abstainCount: number;
}

export interface NameCandidate {
  name: string;
  voteCount: number;
  isMine: boolean;
}

export interface NameContest {
  myName: string | null;
  candidates: NameCandidate[];
}

export interface OrderTrackTally {
  myRank: number | null;
  averageRank: number | null;
  consensusPosition: number | null;
}

export interface OrderContest {
  locked: boolean;
  voteCount: number;
  mySongIds: number[] | null;
}

export interface ArtCandidate {
  driveFileId: string;
  voteCount: number;
  isMine: boolean;
}

export interface ArtContest {
  locked: boolean;
  appliedDriveFileId: string | null;
  voteCount: number;
  myDriveFileId: string | null;
  candidates: ArtCandidate[];
}

export interface AlbumArtUpload {
  driveFileId: string;
  name: string;
  mimeType: string | null;
  width: number | null;
  height: number | null;
  warning: string | null;
}

export interface AlbumTrack {
  id: number;
  songId: number;
  songName: string;
  version: number | null;
  sortOrder: number;
  addedAt: string;
  inclusion?: InclusionTally;
  names?: NameContest;
  order?: OrderTrackTally;
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
  names?: NameContest;
  order?: OrderContest;
  art?: ArtContest;
}
