import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  GoogleStatus,
  Member,
  MemberSummary,
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

  listBandSongs(bandId: number) {
    return this.httpClient.get<Song[]>(`/api/bands/${bandId}/songs`);
  }

  songAudioUrl(songId: number) {
    return `/api/songs/${songId}/audio`;
  }

  googleStatus() {
    return this.httpClient.get<GoogleStatus>('/api/auth/google/status');
  }
}
