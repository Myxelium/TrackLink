import {
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  EMPTY,
  catchError,
  tap
} from 'rxjs';
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
  private readonly trackLinkApi = inject(TrackLinkApi);
  private readonly memberSession = inject(SessionService);

  readonly availableMembers = signal<MemberSummary[]>([]);
  readonly currentMember = signal<Member | null>(null);
  readonly bandSongs = signal<Song[]>([]);
  readonly googleDriveStatus = signal<GoogleStatus | null>(null);
  readonly loadError = signal<string | null>(null);
  readonly playingId = signal<number | null>(null);
  readonly selectedMemberId = signal<number | null>(null);

  ngOnInit() {
    this.trackLinkApi.listMembers().pipe(
      tap((loadedMemberList) => {
        this.availableMembers.set(loadedMemberList);
        const storedMemberId = this.memberSession.memberId();
        const fallbackMemberId = loadedMemberList[0]?.id ?? null;
        const chosenMemberId = storedMemberId && loadedMemberList.some(
          (memberSummary) => memberSummary.id === storedMemberId
        ) ? storedMemberId : fallbackMemberId;

        this.selectedMemberId.set(chosenMemberId);

        if (chosenMemberId !== null) {
          this.memberSession.setMemberId(chosenMemberId);
          this.loadMember(chosenMemberId);
        }
      }),
      catchError(() => {
        this.loadError.set('Could not reach the TrackLink API. Start the API on port 5180.');
        return EMPTY;
      })
    )
      .subscribe();

    this.trackLinkApi.googleStatus().pipe(
      tap((currentDriveStatus) => this.googleDriveStatus.set(currentDriveStatus)),
      catchError(() => {
        this.googleDriveStatus.set({ configured: false, connected: false, email: null });
        return EMPTY;
      })
    )
      .subscribe();
  }

  onMemberChange(rawId: string) {
    const parsedMemberId = Number(rawId);

    this.selectedMemberId.set(parsedMemberId);
    this.memberSession.setMemberId(parsedMemberId);
    this.loadMember(parsedMemberId);
  }

  playTheSong(selectedSong: Song) {
    this.playingId.set(selectedSong.id);
  }

  audioUrl(songId: number) {
    return this.trackLinkApi.songAudioUrl(songId);
  }

  googleLoginHref() {
    return '/api/auth/google/login';
  }

  driveStatus() {
    const currentDriveStatus = this.googleDriveStatus();

    if (!currentDriveStatus) {
      return '';
    }

    if (currentDriveStatus.configured && currentDriveStatus.connected) {
      return currentDriveStatus.email ? `Drive connected as ${currentDriveStatus.email}.` : 'Drive connected.';
    }

    if (currentDriveStatus.configured) {
      return 'Connect Google Drive to play files stored there.';
    }

    return 'Drive OAuth needs Google client credentials on the API.';
  }

  private loadMember(memberId: number) {
    this.loadError.set(null);
    this.trackLinkApi.getMember(memberId).pipe(
      tap((loadedMember) => {
        this.currentMember.set(loadedMember);
        const bandId = loadedMember.bands[0]?.id;

        if (!bandId) {
          this.bandSongs.set([]);
          return;
        }

        this.trackLinkApi.listBandSongs(bandId).pipe(
          tap((loadedSongs) => this.bandSongs.set(loadedSongs)),
          catchError(() => {
            this.loadError.set('Could not load this band\'s songs.');
            return EMPTY;
          })
        )
          .subscribe();
      }),
      catchError(() => {
        this.loadError.set('Could not load member information.');
        return EMPTY;
      })
    )
      .subscribe();
  }
}
