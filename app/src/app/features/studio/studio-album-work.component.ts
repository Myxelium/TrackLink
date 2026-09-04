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
  ProposalReview,
  Song
} from '../../core/models';

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
  readonly canManage = input(false);
  readonly currentMemberId = input<number | null>(null);
  readonly proposeSongId = input<number | null>(null);
  readonly playingId = input<number | null>(null);
  readonly trackChosen = output<AlbumTrack>();
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
