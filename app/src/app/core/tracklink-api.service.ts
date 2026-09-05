import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  AcceptInviteResult,
  AlbumArtUpload,
  AlbumDetail,
  AlbumProposal,
  AlbumSummary,
  DriveFile,
  GoogleStatus,
  Invite,
  Member,
  MemberSummary,
  PickerToken,
  Song
} from './models';

@Injectable({ providedIn: 'root' })
export class TrackLinkApi {
  private readonly httpClient = inject(HttpClient);

  listMembers() {
    return this.httpClient.get<MemberSummary[]>('/api/members');
  }

  getMember(memberId: number) {
    return this.httpClient.get<Member>(`/api/members/${memberId}`);
  }

  loadSession() {
    return this.httpClient.get<GoogleStatus>('/api/auth/me');
  }

  listBandSongs(bandId: number, searchQuery = '') {
    const trimmedQuery = searchQuery.trim();

    return this.httpClient.get<Song[]>(`/api/bands/${bandId}/songs`, {
      params: trimmedQuery ? { q: trimmedQuery } : {}
    });
  }

  listBandDriveFiles(bandId: number, listedKind: 'audio' | 'image' = 'audio') {
    return this.httpClient.get<DriveFile[]>(`/api/bands/${bandId}/drive/files`, {
      params: listedKind === 'image' ? { kind: listedKind } : {}
    });
  }

  setBandDriveFolder(bandId: number, folderId: string, name: string | null) {
    return this.httpClient.put(`/api/bands/${bandId}/drive-folder`, {
      folderId,
      name
    });
  }

  createInvite(bandId: number, email: string, role: string) {
    return this.httpClient.post<Invite>(`/api/bands/${bandId}/invites`, { email, role });
  }

  acceptInvite(email: string, code: string) {
    return this.httpClient.post<AcceptInviteResult>('/api/invites/accept', { email, code });
  }

  pickerToken() {
    return this.httpClient.get<PickerToken>('/api/auth/google/picker-token');
  }

  signOutSession() {
    return this.httpClient.post<{ signedIn: boolean }>('/api/auth/logout', {});
  }

  songAudioUrl(songId: number) {
    return `/api/songs/${songId}/audio`;
  }

  googleStatus() {
    return this.loadSession();
  }

  linkDriveSong(bandId: number, driveFile: DriveFile) {
    return this.httpClient.post<Song>(`/api/bands/${bandId}/songs`, {
      driveFileId: driveFile.id,
      name: driveFile.name
    });
  }

  listAlbums(bandId: number) {
    return this.httpClient.get<AlbumSummary[]>(`/api/bands/${bandId}/albums`);
  }

  createAlbum(bandId: number, name: string, approvalRule: string) {
    return this.httpClient.post<AlbumSummary>(`/api/bands/${bandId}/albums`, {
      name,
      approvalRule
    });
  }

  getAlbum(albumId: number) {
    return this.httpClient.get<AlbumDetail>(`/api/albums/${albumId}`);
  }

  updateAlbum(
    albumId: number,
    body: { name?: string; archived?: boolean; approvalRule?: string }
  ) {
    return this.httpClient.patch<AlbumDetail>(`/api/albums/${albumId}`, body);
  }

  castInclusionVote(albumId: number, songId: number, inclusionChoice: string) {
    return this.httpClient.put<AlbumDetail>(`/api/albums/${albumId}/inclusion-votes`, {
      songId,
      choice: inclusionChoice
    });
  }

  castNameVote(albumId: number, songId: number | null, listedName: string) {
    return this.httpClient.put<AlbumDetail>(`/api/albums/${albumId}/name-votes`, {
      songId,
      name: listedName
    });
  }

  castOrderVote(albumId: number, songIds: number[]) {
    return this.httpClient.put<AlbumDetail>(`/api/albums/${albumId}/order-votes`, {
      songIds
    });
  }

  lockAlbumOrder(albumId: number, orderLocked: boolean) {
    return this.httpClient.put<AlbumDetail>(`/api/albums/${albumId}/order-lock`, {
      locked: orderLocked
    });
  }

  castArtVote(albumId: number, driveFileId: string) {
    return this.httpClient.put<AlbumDetail>(`/api/albums/${albumId}/art-votes`, {
      driveFileId
    });
  }

  lockAlbumArt(albumId: number, artLocked: boolean) {
    return this.httpClient.put<AlbumDetail>(`/api/albums/${albumId}/art-lock`, {
      locked: artLocked
    });
  }

  uploadAlbumArt(albumId: number, coverFile: File) {
    const body = new FormData();

    body.append('file', coverFile, coverFile.name);

    return this.httpClient.post<AlbumArtUpload>(`/api/albums/${albumId}/art`, body);
  }

  proposeAlbumSong(albumId: number, songId: number) {
    return this.httpClient.post<AlbumProposal>(`/api/albums/${albumId}/proposals`, { songId });
  }

  decideProposal(albumId: number, proposalId: number, decision: 'approve' | 'reject') {
    return this.httpClient.post<AlbumProposal>(
      `/api/albums/${albumId}/proposals/${proposalId}/decisions`,
      { decision }
    );
  }

  withdrawProposal(albumId: number, proposalId: number) {
    return this.httpClient.post<AlbumProposal>(
      `/api/albums/${albumId}/proposals/${proposalId}/withdraw`,
      {}
    );
  }

  addProposalReview(
    albumId: number,
    proposalId: number,
    body: { startMs: number; endMs: number; body: string }
  ) {
    return this.httpClient.post<AlbumProposal>(
      `/api/albums/${albumId}/proposals/${proposalId}/reviews`,
      body
    );
  }

  removeProposalReview(albumId: number, proposalId: number, reviewId: number) {
    return this.httpClient.delete<AlbumProposal>(
      `/api/albums/${albumId}/proposals/${proposalId}/reviews/${reviewId}`
    );
  }
}
