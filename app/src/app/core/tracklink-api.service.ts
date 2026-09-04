import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  AcceptInviteResult,
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

  listBandSongs(bandId: number) {
    return this.httpClient.get<Song[]>(`/api/bands/${bandId}/songs`);
  }

  listBandDriveFiles(bandId: number) {
    return this.httpClient.get<DriveFile[]>(`/api/bands/${bandId}/drive/files`);
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
