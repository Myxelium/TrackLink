import {
  Component,
  input,
  output
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  AlbumDetail,
  AlbumProposal,
  AlbumTrack,
  DriveFile,
  ProposalReview,
  Song
} from '../../core/models';
import {
  albumArtContest,
  albumArtPollChoices,
  albumArtPreviewUrl,
  ArtPollChoice,
  artCandidateLabel,
  artPollChoiceLabel,
  artResolutionWarning,
  canLockArtContest,
  canSubmitArtVote,
  canUnlockArtContest,
  trimmedArtFileId
} from '../../domains/album/art-vote';
import { takeVersionLabel } from '../../domains/song/take-version-label';
import {
  inclusionChoiceLabel,
  inclusionChoices,
  inclusionCountFor,
  trackInclusion
} from '../../domains/album/inclusion-vote';
import {
  albumNameContest,
  canSubmitNameVote,
  trackNameContest,
  trimmedNameVote
} from '../../domains/album/name-vote';
import {
  albumOrderContest,
  canLockOrderContest,
  canUnlockOrderContest,
  consensusRankLabel,
  draftOrderSongIds,
  isCompleteOrder,
  moveRankedSong,
  trackOrderTally
} from '../../domains/album/order-vote';

@Component({
  selector: 'app-studio-album-work',
  imports: [FormsModule],
  templateUrl: './studio-album-work.component.html',
  styleUrl: './studio-page.component.scss'
})
export class StudioAlbumWorkComponent {
  readonly album = input<AlbumDetail | null>(null);

  readonly proposal = input<AlbumProposal | null>(null);

  readonly bandSongs = input<Song[]>([]);

  readonly driveImageFiles = input<DriveFile[]>([]);

  readonly canManage = input(false);

  readonly currentMemberId = input<number | null>(null);

  readonly proposeSongId = input<number | null>(null);

  readonly playingId = input<number | null>(null);

  readonly trackChosen = output<AlbumTrack>();

  readonly inclusionRequested = output<{ songId: number; choice: string }>();

  readonly nameRequested = output<{ songId: number | null; name: string }>();

  readonly orderRequested = output<number[]>();

  readonly orderLockRequested = output<boolean>();

  readonly artRequested = output<string>();

  readonly artFileRequested = output<File>();

  readonly artLockRequested = output<boolean>();

  readonly artSizeWarning = input<string | null>(null);

  readonly artUploading = input(false);

  rankedSongIds: number[] = [];

  private rankedAlbumStamp = '';

  readonly inclusionChoices = inclusionChoices;

  draftAlbumName = '';

  draftArtFileId = '';

  draftSongNames: Record<number, string> = {};

  readonly proposalChosen = output<AlbumProposal>();

  readonly proposalClosed = output<boolean>();

  readonly deskRequested = output<boolean>();

  readonly isOwner = input(false);

  readonly proposeSongIdChange = output<number | null>();

  readonly proposalRequested = output<boolean>();

  readonly decisionRequested = output<'approve' | 'reject'>();

  readonly withdrawRequested = output<boolean>();

  readonly archiveRequested = output<boolean>();

  readonly ruleChangeRequested = output<string>();

  readonly reviewStartMs = input<number | null>(null);

  readonly reviewEndMs = input<number | null>(null);

  readonly reviewBody = input('');

  readonly reviewBodyChange = output<string>();

  readonly markRequested = output<'start' | 'end'>();

  readonly reviewRequested = output<boolean>();

  readonly reviewDeleted = output<ProposalReview>();

  readonly reviewSeeked = output<ProposalReview>();

  proposeableSongs() {
    const album = this.album();

    if (!album) {
      return [];
    }

    const takenIds = new Set([
      ...album.tracks.map((track) => track.songId),
      ...album.proposals
        .filter((item) => item.status === 'open')
        .map((item) => item.songId)
    ]);

    return this.bandSongs().filter((song) => !takenIds.has(song.id));
  }

  versionLabel(listedTake: Song) {
    return takeVersionLabel(listedTake, this.bandSongs());
  }

  canVoteInclusion() {
    const album = this.album();

    return Boolean(this.currentMemberId() && album && !album.archived);
  }

  inclusionLabel(listedChoice: string) {
    return inclusionChoiceLabel(listedChoice);
  }

  inclusionCount(listedTrack: AlbumTrack, listedChoice: string) {
    return inclusionCountFor(listedTrack, listedChoice);
  }

  isMyInclusion(listedTrack: AlbumTrack, listedChoice: string) {
    return trackInclusion(listedTrack).myChoice === listedChoice;
  }

  canVoteNames() {
    return this.canVoteInclusion();
  }

  albumNames() {
    return albumNameContest(this.album());
  }

  trackNames(listedTrack: AlbumTrack) {
    return trackNameContest(listedTrack);
  }

  draftSongName(songId: number) {
    return this.draftSongNames[songId] ?? '';
  }

  setDraftSongName(songId: number, listedName: string) {
    this.draftSongNames[songId] = listedName;
  }

  canSubmitAlbumName() {
    return this.canVoteNames() && canSubmitNameVote(this.draftAlbumName);
  }

  canSubmitSongName(songId: number) {
    return this.canVoteNames() && canSubmitNameVote(this.draftSongName(songId));
  }

  albumArt() {
    return albumArtContest(this.album());
  }

  artStatusLabel() {
    const artContest = this.albumArt();

    if (artContest.locked) {
      return artContest.appliedDriveFileId
        ? `Locked · ${this.artLabel(artContest.appliedDriveFileId)}`
        : 'Locked';
    }

    if (artContest.voteCount === 1) {
      return '1 vote';
    }

    return `${artContest.voteCount} votes`;
  }

  artLabel(driveFileId: string) {
    return artCandidateLabel(driveFileId, this.driveImageFiles());
  }

  artChoices() {
    const listedAlbum = this.album();
    const folderImages = this.canVoteArt() ? this.driveImageFiles() : [];

    return albumArtPollChoices(listedAlbum, folderImages);
  }

  artChoiceLabel(listedChoice: ArtPollChoice) {
    return artPollChoiceLabel(listedChoice);
  }

  artPreviewUrl() {
    const listedAlbum = this.album();
    const appliedFileId = albumArtContest(listedAlbum).appliedDriveFileId;

    return listedAlbum && appliedFileId ? albumArtPreviewUrl(listedAlbum.id) : null;
  }

  canVoteArt() {
    const listedAlbum = this.album();

    return Boolean(
      this.currentMemberId() &&
      listedAlbum &&
      !listedAlbum.archived &&
      !albumArtContest(listedAlbum).locked
    );
  }

  canSubmitArt() {
    return this.canVoteArt() && canSubmitArtVote(this.draftArtFileId);
  }

  canUploadArt() {
    return this.canVoteArt() && !this.artUploading();
  }

  shownArtWarning() {
    return artResolutionWarning(this.artSizeWarning());
  }

  requestArtUpload(listedEvent: Event) {
    const fileInput = listedEvent.target as HTMLInputElement;
    const chosenFile = fileInput.files?.[0];

    fileInput.value = '';

    if (!chosenFile || !this.canUploadArt()) {
      return;
    }

    this.artFileRequested.emit(chosenFile);
  }

  canLockArt() {
    return canLockArtContest(this.canManage(), this.album());
  }

  canUnlockArt() {
    return canUnlockArtContest(this.canManage(), this.album());
  }

  requestArt(listedFileId = this.draftArtFileId) {
    const driveFileId = trimmedArtFileId(listedFileId);

    if (!this.canVoteArt() || !canSubmitArtVote(driveFileId)) {
      return;
    }

    this.artRequested.emit(driveFileId);
    this.draftArtFileId = '';
  }

  requestArtLock() {
    if (!this.canLockArt()) {
      return;
    }

    this.artLockRequested.emit(true);
  }

  requestArtUnlock() {
    if (!this.canUnlockArt()) {
      return;
    }

    this.artLockRequested.emit(false);
  }

  requestAlbumName(listedName = this.draftAlbumName) {
    const trimmedName = trimmedNameVote(listedName);

    if (!this.canVoteNames() || !canSubmitNameVote(trimmedName)) {
      return;
    }

    this.nameRequested.emit({
      songId: null,
      name: trimmedName
    });

    this.draftAlbumName = '';
  }

  albumOrder() {
    return albumOrderContest(this.album());
  }

  orderStatusLabel() {
    const orderContest = this.albumOrder();

    if (orderContest.locked) {
      return 'Locked';
    }

    if (orderContest.voteCount === 1) {
      return '1 ranking';
    }

    return `${orderContest.voteCount} rankings`;
  }

  rankedTracks() {
    this.syncRankedDraft();
    const listedAlbum = this.album();

    if (!listedAlbum) {
      return [];
    }

    const bySongId = new Map(listedAlbum.tracks.map((listedTrack) => [listedTrack.songId, listedTrack]));

    return this.rankedSongIds
      .map((songId) => bySongId.get(songId))
      .filter((listedTrack): listedTrack is AlbumTrack => Boolean(listedTrack));
  }

  orderLabel(listedTrack: AlbumTrack) {
    return consensusRankLabel(trackOrderTally(listedTrack));
  }

  canVoteOrder() {
    const listedAlbum = this.album();

    return Boolean(
      this.currentMemberId() &&
      listedAlbum &&
      !listedAlbum.archived &&
      !albumOrderContest(listedAlbum).locked &&
      listedAlbum.tracks.length > 0
    );
  }

  canSubmitOrder() {
    const listedAlbum = this.album();

    return Boolean(
      this.canVoteOrder() &&
      listedAlbum &&
      isCompleteOrder(this.rankedSongIds, listedAlbum.tracks.map((listedTrack) => listedTrack.songId))
    );
  }

  canLockOrder() {
    return canLockOrderContest(this.canManage(), this.album());
  }

  canUnlockOrder() {
    return canUnlockOrderContest(this.canManage(), this.album());
  }

  moveOrderTake(songId: number, rankDelta: number) {
    if (!this.canVoteOrder()) {
      return;
    }

    this.rankedSongIds = moveRankedSong(this.rankedSongIds, songId, rankDelta);
  }

  requestOrder() {
    if (!this.canSubmitOrder()) {
      return;
    }

    this.orderRequested.emit([...this.rankedSongIds]);
  }

  requestOrderLock() {
    if (!this.canLockOrder()) {
      return;
    }

    this.orderLockRequested.emit(true);
  }

  requestOrderUnlock() {
    if (!this.canUnlockOrder()) {
      return;
    }

    this.orderLockRequested.emit(false);
  }

  requestSongName(songId: number, listedName = this.draftSongName(songId)) {
    const trimmedName = trimmedNameVote(listedName);

    if (!this.canVoteNames() || !canSubmitNameVote(trimmedName)) {
      return;
    }

    this.nameRequested.emit({
      songId,
      name: trimmedName
    });

    this.draftSongNames[songId] = '';
  }

  waitingLabel(proposal: AlbumProposal) {
    if (proposal.status === 'approved') {
      return 'On the album';
    }

    if (proposal.status === 'rejected') {
      return 'Rejected';
    }

    if (proposal.status === 'withdrawn') {
      return 'Withdrawn';
    }

    if (proposal.waitingOn.length === 0) {
      return 'Open';
    }

    return `Waiting on ${proposal.waitingOn.map((member) => member.name).join(', ')}`;
  }

  canWithdraw() {
    const proposal = this.proposal();
    const memberId = this.currentMemberId();

    return Boolean(
      proposal &&
      memberId &&
      (proposal.proposedBy === memberId || this.isOwner()) &&
      (proposal.status === 'open' || proposal.status === 'rejected')
    );
  }

  canDecide() {
    const proposal = this.proposal();

    return Boolean(proposal && (proposal.status === 'open' || proposal.status === 'rejected'));
  }

  canReview() {
    const proposal = this.proposal();

    return Boolean(proposal && proposal.status !== 'withdrawn');
  }

  canDeleteReview(review: ProposalReview) {
    const memberId = this.currentMemberId();

    return Boolean(memberId && (review.memberId === memberId || this.isOwner()));
  }

  canSubmitReview() {
    return this.reviewStartMs() !== null && this.reviewBody().trim().length > 0;
  }

  rangeLabel(review: ProposalReview) {
    return formatReviewRange(review.startMs, review.endMs);
  }

  draftRangeLabel() {
    const startMs = this.reviewStartMs();

    if (startMs === null) {
      return 'Mark a start on the take';
    }

    return formatReviewRange(startMs, this.reviewEndMs() ?? startMs);
  }

  private syncRankedDraft() {
    const listedAlbum = this.album();
    const albumStamp = listedAlbum
      ? `${listedAlbum.id}:${listedAlbum.tracks.map((listedTrack) => listedTrack.songId).join(',')}`
      : '';

    if (albumStamp === this.rankedAlbumStamp) {
      return;
    }

    this.rankedAlbumStamp = albumStamp;
    this.rankedSongIds = draftOrderSongIds(listedAlbum);
  }
}

export function formatReviewRange(startMs: number, endMs: number) {
  if (endMs <= startMs) {
    return formatReviewMs(startMs);
  }

  return `${formatReviewMs(startMs)}-${formatReviewMs(endMs)}`;
}

export function formatReviewMs(totalMs: number) {
  const totalSeconds = Math.max(0, Math.floor(totalMs / 1000));
  const minuteCount = Math.floor(totalSeconds / 60);
  const secondCount = totalSeconds % 60;

  return `${minuteCount}:${secondCount.toString().padStart(2, '0')}`;
}
