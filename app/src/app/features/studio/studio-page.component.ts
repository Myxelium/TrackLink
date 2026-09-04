import {
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TrackLinkApi } from '../../core/tracklink-api.service';
import { SessionService } from '../../core/session.service';
import {
  GoogleStatus,
  Member,
  MemberSummary,
  Song
} from '../../core/models';

@Component({
  selector: 'app-studio-page',
  imports: [FormsModule],
  templateUrl: './studio-page.component.html',
  styleUrl: './studio-page.component.scss'
})
export class StudioPageComponent implements OnInit {
  private readonly api = inject(TrackLinkApi);
  private readonly session = inject(SessionService);

  readonly members = signal<MemberSummary[]>([]);
  readonly member = signal<Member | null>(null);
  readonly songs = signal<Song[]>([]);
  readonly google = signal<GoogleStatus | null>(null);
  readonly loadError = signal<string | null>(null);
  readonly playingId = signal<number | null>(null);
  readonly selectedMemberId = signal<number | null>(null);

  ngOnInit() {
    this.api.listMembers().subscribe({
      next: (members) => {
        this.members.set(members);
        const stored = this.session.memberId();
        const fallback = members[0]?.id ?? null;
        const chosen = stored && members.some((item) => item.id === stored) ? stored : fallback;

        this.selectedMemberId.set(chosen);

        if (chosen !== null) {
          this.session.setMemberId(chosen);
          this.loadMember(chosen);
        }
      },
      error: () => this.loadError.set('Could not reach the TrackLink API. Start the API on port 5180.')
    });

    this.api.googleStatus().subscribe({
      next: (status) => this.google.set(status),
      error: () => this.google.set({ configured: false, connected: false, email: null })
    });
  }

  onMemberChange(rawId: string) {
    const id = Number(rawId);

    this.selectedMemberId.set(id);
    this.session.setMemberId(id);
    this.loadMember(id);
  }

  play(song: Song) {
    this.playingId.set(song.id);
  }

  audioUrl(songId: number) {
    return this.api.songAudioUrl(songId);
  }

  googleLoginHref() {
    return '/api/auth/google/login';
  }

  driveStatus() {
    const status = this.google();

    if (!status) {
      return '';
    }

    if (status.configured && status.connected) {
      return status.email ? `Drive connected as ${status.email}.` : 'Drive connected.';
    }

    if (status.configured) {
      return 'Connect Google Drive to play files stored there.';
    }

    return 'Drive OAuth needs Google client credentials on the API.';
  }

  private loadMember(id: number) {
    this.loadError.set(null);
    this.api.getMember(id).subscribe({
      next: (member) => {
        this.member.set(member);
        const bandId = member.bands[0]?.id;

        if (!bandId) {
          this.songs.set([]);
          return;
        }

        this.api.listBandSongs(bandId).subscribe({
          next: (songs) => this.songs.set(songs),
          error: () => this.loadError.set('Could not load this band\'s songs.')
        });
      },
      error: () => this.loadError.set('Could not load member information.')
    });
  }
}
