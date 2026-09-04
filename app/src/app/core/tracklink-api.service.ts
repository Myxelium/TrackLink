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
  private readonly http = inject(HttpClient);

  listMembers() {
    return this.http.get<MemberSummary[]>('/api/members');
  }

  getMember(id: number) {
    return this.http.get<Member>(`/api/members/${id}`);
  }

  listBandSongs(bandId: number) {
    return this.http.get<Song[]>(`/api/bands/${bandId}/songs`);
  }

  songAudioUrl(songId: number) {
    return `/api/songs/${songId}/audio`;
  }

  googleStatus() {
    return this.http.get<GoogleStatus>('/api/auth/google/status');
  }
}
