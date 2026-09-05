import {
  Component,
  OnInit,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { StudioAlbumDeskComponent } from './studio-album-desk.component';
import { StudioAlbumWorkComponent } from './studio-album-work.component';
import { StudioBandSetupComponent } from './studio-band-setup.component';
import { StudioDeckComponent } from './studio-deck.component';
import { StudioDriveListComponent } from './studio-drive-list.component';
import { StudioSessionFactsComponent } from './studio-session-facts.component';
import {
  EMPTY,
  catchError,
  tap
} from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { TrackLinkApi } from '../../core/tracklink-api.service';
import { SessionService } from '../../core/session.service';
import { GoogleFolderPicker } from '../../core/google-folder-picker.service';
import {
  AlbumDetail,
  AlbumProposal,
  AlbumSummary,
  AlbumTrack,
  DriveFile,
  GoogleStatus,
  Invite,
  Member,
  ProposalReview,
  Song
} from '../../core/models';
import { takeSourceDateLabel, takeVersionLabel } from '../../domains/song/take-version-label';

@Component({
  selector: 'app-studio-page',
  imports: [
    StudioAlbumDeskComponent,
    StudioAlbumWorkComponent,
    StudioBandSetupComponent,
    StudioDeckComponent,
    StudioDriveListComponent,
    StudioSessionFactsComponent
  ],
  templateUrl: './studio-page.component.html',
  styleUrl: './studio-page.component.scss'
})
export class StudioPageComponent implements OnInit {
  private readonly trackLinkApi = inject(TrackLinkApi);
  private readonly memberSession = inject(SessionService);
  private readonly folderPicker = inject(GoogleFolderPicker);

  readonly currentMember = signal<Member | null>(null);
  readonly bandSongs = signal<Song[]>([]);
  readonly googleDriveStatus = signal<GoogleStatus | null>(null);
  readonly driveFiles = signal<DriveFile[]>([]);
  readonly driveError = signal<string | null>(null);
  readonly loadError = signal<string | null>(null);
  readonly playingId = signal<number | null>(null);
  readonly focusedStudioPanel = signal<'takes' | 'drive' | 'albums'>('takes');
  readonly lastInvite = signal<Invite | null>(null);
  readonly albums = signal<AlbumSummary[]>([]);
  readonly openAlbum = signal<AlbumDetail | null>(null);
  readonly openProposal = signal<AlbumProposal | null>(null);
  inviteEmail = '';
  inviteRole = 'uploader';
  newAlbumName = '';
  newAlbumRule = 'all';
  proposeSongId: number | null = null;
  reviewStartMs: number | null = null;
  reviewEndMs: number | null = null;
  reviewBody = '';
  readonly seekMs = signal<number | null>(null);
  readonly seekEpoch = signal(0);
  private readonly deck = viewChild(StudioDeckComponent);

  ngOnInit() {
    this.trackLinkApi.loadSession().pipe(
      tap((session) => this.applySession(session)),
      catchError(() => {
        this.googleDriveStatus.set({
          configured: false,
          signedIn: false,
          connected: false,
          email: null,
          member: null
        });

        this.loadError.set('Could not reach the TrackLink API. Start the API on port 5180.');
        this.loadDemoCatalog();
        return EMPTY;
      })
    )
      .subscribe();
  }

  playTheSong(selectedSong: Song) {
    this.playingId.set(selectedSong.id);
    this.focusedStudioPanel.set('takes');
  }

  showTakesPanel() {
    this.focusedStudioPanel.set('takes');
  }

  showDrivePanel() {
    this.focusedStudioPanel.set('drive');
  }

  showAlbumsPanel() {
    this.focusedStudioPanel.set('albums');
    this.loadAlbums();
  }

  canManageAlbums() {
    const role = this.currentBand()?.myRole;

    return role === 'owner' || role === 'uploader';
  }

  isBandOwner() {
    return Boolean(this.currentBand()?.isOwner);
  }

  selectAlbum(album: AlbumSummary) {
    this.openProposal.set(null);
    this.loadAlbum(album.id);
  }

  closeOpenAlbum() {
    this.openAlbum.set(null);
    this.openProposal.set(null);
  }

  createAlbum() {
    const bandId = this.currentBand()?.id;
    const name = this.newAlbumName.trim();

    if (!bandId || !name) {
      return;
    }

    this.trackLinkApi.createAlbum(bandId, name, this.newAlbumRule).pipe(
      tap((created) => {
        this.newAlbumName = '';
        this.albums.update((currentAlbums) => [...currentAlbums, created]);
        this.loadAlbum(created.id);
      }),
      catchError(() => {
        this.loadError.set('Could not create that album.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  playAlbumTrack(track: AlbumTrack) {
    const knownTake = this.bandSongs().find((catalogSong) => catalogSong.id === track.songId);

    this.playTheSong(knownTake ?? {
      id: track.songId,
      name: track.songName,
      description: null,
      uploadedBy: 0,
      version: track.version,
      previousVersion: 0,
      storageKind: 'url'
    });
  }

  openProposalPane(proposal: AlbumProposal) {
    this.openProposal.set(proposal);
  }

  closeProposalPane() {
    this.openProposal.set(null);
    this.reviewBody = '';
    this.reviewStartMs = null;
    this.reviewEndMs = null;
  }

  proposeSong() {
    const albumId = this.openAlbum()?.id;
    const songId = this.proposeSongId;

    if (!albumId || !songId) {
      return;
    }

    this.trackLinkApi.proposeAlbumSong(albumId, songId).pipe(
      tap((proposal) => {
        this.proposeSongId = null;
        this.openProposal.set(proposal.status === 'approved' ? null : proposal);
        this.loadAlbum(albumId);
      }),
      catchError((proposeFailure: HttpErrorResponse) => {
        const folderErrorText = proposeFailure.error?.error;

        this.loadError.set(
          typeof folderErrorText === 'string' && folderErrorText.length > 0
            ? folderErrorText
            : 'Could not open that proposal. The take must already be on this band.'
        );

        return EMPTY;
      })
    )
      .subscribe();
  }

  decideProposal(decision: 'approve' | 'reject') {
    const album = this.openAlbum();
    const proposal = this.openProposal();

    if (!album || !proposal) {
      return;
    }

    this.trackLinkApi.decideProposal(album.id, proposal.id, decision).pipe(
      tap((updated) => {
        this.openProposal.set(updated);
        this.loadAlbum(album.id);
      }),
      catchError(() => {
        this.loadError.set('Could not record that approval.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  withdrawProposal() {
    const album = this.openAlbum();
    const proposal = this.openProposal();

    if (!album || !proposal) {
      return;
    }

    this.trackLinkApi.withdrawProposal(album.id, proposal.id).pipe(
      tap((updated) => {
        this.openProposal.set(updated);
        this.loadAlbum(album.id);
      }),
      catchError(() => {
        this.loadError.set('Could not withdraw that proposal.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  markReviewBound(bound: 'start' | 'end') {
    const atMs = this.deck()?.currentTimeMs() ?? 0;

    if (bound === 'start') {
      this.reviewStartMs = atMs;

      if (this.reviewEndMs !== null && this.reviewEndMs < atMs) {
        this.reviewEndMs = atMs;
      }

      return;
    }

    this.reviewEndMs = atMs;

    if (this.reviewStartMs === null || this.reviewStartMs > atMs) {
      this.reviewStartMs = atMs;
    }
  }

  addProposalReview() {
    const album = this.openAlbum();
    const proposal = this.openProposal();
    const startMs = this.reviewStartMs;
    const body = this.reviewBody.trim();

    if (!album || !proposal || startMs === null || !body) {
      return;
    }

    this.trackLinkApi.addProposalReview(album.id, proposal.id, {
      startMs,
      endMs: this.reviewEndMs ?? startMs,
      body
    }).pipe(
      tap((updated) => {
        this.reviewBody = '';
        this.reviewStartMs = null;
        this.reviewEndMs = null;
        this.openProposal.set(updated);
        this.loadAlbum(album.id);
      }),
      catchError(() => {
        this.loadError.set('Could not add that review. Mark a start and write a comment.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  deleteProposalReview(review: ProposalReview) {
    const album = this.openAlbum();
    const proposal = this.openProposal();

    if (!album || !proposal) {
      return;
    }

    this.trackLinkApi.removeProposalReview(album.id, proposal.id, review.id).pipe(
      tap((updated) => {
        this.openProposal.set(updated);
        this.loadAlbum(album.id);
      }),
      catchError(() => {
        this.loadError.set('Could not remove that review.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  seekProposalReview(review: ProposalReview) {
    this.playAlbumTrack({
      id: review.id,
      songId: this.openProposal()?.songId ?? 0,
      songName: this.openProposal()?.songName ?? 'Take',
      version: this.openProposal()?.songVersion ?? null,
      sortOrder: 0,
      addedAt: review.createdAt
    });

    this.seekMs.set(review.startMs);
    this.seekEpoch.update((epoch) => epoch + 1);
  }

  archiveAlbum() {
    const album = this.openAlbum();

    if (!album) {
      return;
    }

    this.trackLinkApi.updateAlbum(album.id, { archived: true }).pipe(
      tap((updated) => {
        this.openAlbum.set(updated);
        this.loadAlbums();
      }),
      catchError(() => {
        this.loadError.set('Could not archive that album.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  changeAlbumRule(approvalRule: string) {
    const album = this.openAlbum();

    if (!album || album.approvalRule === approvalRule) {
      return;
    }

    this.trackLinkApi.updateAlbum(album.id, { approvalRule }).pipe(
      tap((updated) => this.openAlbum.set(updated)),
      catchError(() => {
        this.loadError.set('Could not change the approval rule.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  currentBand() {
    return this.currentMember()?.bands[0] ?? null;
  }

  currentBandName() {
    return this.currentBand()?.name || 'No band';
  }

  takeCountLabel() {
    const takeCount = this.bandSongs().length;

    return takeCount === 1 ? '1 take' : `${takeCount} takes`;
  }

  versionLabel(listedTake: Song) {
    return takeVersionLabel(listedTake, this.bandSongs());
  }

  sourceDateLabel(listedTake: Song) {
    return takeSourceDateLabel(listedTake);
  }

  armedTakeLabel() {
    const armedTakeId = this.playingId();
    const armedTake = this.bandSongs().find((catalogSong) => catalogSong.id === armedTakeId);

    return armedTake ? armedTake.name : 'No take armed';
  }

  async pickBandFolder() {
    const band = this.currentBand();

    if (!band) {
      return;
    }

    try {
      const folder = await this.folderPicker.pickFolder();

      if (!folder) {
        return;
      }

      this.trackLinkApi.setBandDriveFolder(band.id, folder.id, folder.name).pipe(
        tap(() => this.reloadSession()),
        catchError(() => {
          this.loadError.set('Could not save that Drive folder. Check that you can open it.');
          return EMPTY;
        })
      )
        .subscribe();
    } catch {
      this.loadError.set('Google folder picker could not open. Set Google:ApiKey if the picker stays blank.');
    }
  }

  sendInvite() {
    const band = this.currentBand();

    if (!band || !this.inviteEmail.trim()) {
      return;
    }

    this.trackLinkApi.createInvite(band.id, this.inviteEmail.trim(), this.inviteRole).pipe(
      tap((invite) => {
        this.lastInvite.set(invite);
        this.inviteEmail = '';
      }),
      catchError(() => {
        this.loadError.set('Could not create that invite.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  linkAndPlayDriveFile(driveFile: DriveFile) {
    const bandId = this.currentBand()?.id;

    if (!bandId) {
      this.loadError.set('Sign in and join a band before linking Drive audio.');
      return;
    }

    this.trackLinkApi.linkDriveSong(bandId, driveFile).pipe(
      tap((linkedSong) => {
        this.bandSongs.update((currentSongs) => {
          if (currentSongs.some((catalogSong) => catalogSong.id === linkedSong.id)) {
            return currentSongs.map((catalogSong) =>
              catalogSong.id === linkedSong.id ? linkedSong : catalogSong);
          }

          return [...currentSongs, linkedSong];
        });

        this.playTheSong(linkedSong);
      }),
      catchError(() => {
        this.loadError.set('Could not link that Drive file. It must sit inside the band folder.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  googleLoginHref() {
    return '/api/auth/google/login';
  }

  signOut() {
    this.trackLinkApi.signOutSession().pipe(
      tap(() => {
        this.memberSession.setMember(null);
        this.currentMember.set(null);
        this.googleDriveStatus.set({
          configured: this.googleDriveStatus()?.configured ?? true,
          signedIn: false,
          connected: false,
          email: null,
          member: null
        });

        this.driveFiles.set([]);
        this.albums.set([]);
        this.openAlbum.set(null);
        this.openProposal.set(null);
        this.loadDemoCatalog();
      }),
      catchError(() => EMPTY)
    )
      .subscribe();
  }

  shouldOfferGoogleConnect() {
    const currentDriveStatus = this.googleDriveStatus();

    return Boolean(currentDriveStatus?.configured && !currentDriveStatus.signedIn);
  }

  driveStatus() {
    const currentDriveStatus = this.googleDriveStatus();

    if (!currentDriveStatus) {
      return '';
    }

    if (currentDriveStatus.signedIn && currentDriveStatus.email) {
      const folderName = this.currentBand()?.driveFolderName;

      if (folderName) {
        return `${currentDriveStatus.email} · ${folderName}`;
      }

      return `Signed in as ${currentDriveStatus.email}.`;
    }

    if (currentDriveStatus.configured) {
      return 'Sign in with Google to use Drive and invites.';
    }

    return 'Drive OAuth needs Google client credentials on the API.';
  }

  private applySession(session: GoogleStatus) {
    this.googleDriveStatus.set(session);
    this.currentMember.set(session.member);
    this.memberSession.setMember(session.member);

    if (session.member) {
      const bandId = session.member.bands[0]?.id;

      if (!bandId) {
        this.bandSongs.set([]);
        this.driveFiles.set([]);
        this.albums.set([]);
        this.openAlbum.set(null);
        this.openProposal.set(null);
        return;
      }

      this.loadBandSongs(bandId);
      this.loadDriveFiles(bandId, session.connected, session.member.bands[0]?.driveFolderId ?? null);
      return;
    }

    this.loadDemoCatalog();
  }

  private reloadSession() {
    this.trackLinkApi.loadSession().pipe(
      tap((session) => this.applySession(session)),
      catchError(() => EMPTY)
    )
      .subscribe();
  }

  private loadAlbums() {
    const bandId = this.currentBand()?.id;

    if (!bandId) {
      this.albums.set([]);

      if (!this.currentMember()) {
        this.loadError.set('Sign in with Google to open the album desk.');
      }

      return;
    }

    this.trackLinkApi.listAlbums(bandId).pipe(
      tap((listedAlbums) => this.albums.set(listedAlbums)),
      catchError(() => {
        this.loadError.set('Could not load albums for this band.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  private loadAlbum(albumId: number) {
    this.trackLinkApi.getAlbum(albumId).pipe(
      tap((album) => {
        this.openAlbum.set(album);
        const openProposalId = this.openProposal()?.id;
        const matchingProposal = album.proposals.find((proposal) => proposal.id === openProposalId);

        this.openProposal.set(matchingProposal ?? this.openProposal());
      }),
      catchError(() => {
        this.loadError.set('Could not open that album.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  private loadBandSongs(bandId: number) {
    this.loadError.set(null);
    this.trackLinkApi.listBandSongs(bandId).pipe(
      tap((loadedSongs) => this.bandSongs.set(loadedSongs)),
      catchError(() => {
        this.loadError.set('Could not load this band\'s songs.');
        return EMPTY;
      })
    )
      .subscribe();
  }

  private loadDemoCatalog() {
    this.trackLinkApi.listMembers().pipe(
      tap((members) => {
        const demoMemberId = members[0]?.id;

        if (!demoMemberId) {
          return;
        }

        this.trackLinkApi.getMember(demoMemberId).pipe(
          tap((demoMember) => {
            const bandId = demoMember.bands[0]?.id;

            if (bandId) {
              this.loadBandSongs(bandId);
            }
          }),
          catchError(() => EMPTY)
        )
          .subscribe();
      }),
      catchError(() => EMPTY)
    )
      .subscribe();
  }

  private loadDriveFiles(bandId: number, isConnected: boolean, folderId: string | null) {
    this.driveError.set(null);

    if (!isConnected || !folderId) {
      this.driveFiles.set([]);
      return;
    }

    this.trackLinkApi.listBandDriveFiles(bandId).pipe(
      tap((listedFiles) => this.driveFiles.set(listedFiles)),
      catchError((error: { status?: number }) => {
        this.driveFiles.set([]);

        if (error.status === 403) {
          this.driveError.set('Ask the owner to share the band folder with your Google account.');
        } else if (error.status === 409) {
          this.driveError.set('The owner still needs to pick the band Drive folder.');
        } else {
          this.driveError.set('Could not list Drive files for this band.');
        }

        return EMPTY;
      })
    )
      .subscribe();
  }
}
