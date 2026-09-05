import { AlbumDetail, AlbumTrack } from '../../core/models';
import {
  albumOrderContest,
  canLockOrderContest,
  canUnlockOrderContest,
  consensusRankLabel,
  draftOrderSongIds,
  isCompleteOrder,
  moveRankedSong,
  trackOrderTally
} from './order-vote';

function sampleTrack(fieldOverrides: Partial<AlbumTrack> = {}): AlbumTrack {
  return {
    id: 1,
    songId: 4,
    songName: 'Night Shift',
    version: 1,
    sortOrder: 1,
    addedAt: '2026-09-05T00:00:00Z',
    order: {
      myRank: 2,
      averageRank: 1.5,
      consensusPosition: 2
    },
    ...fieldOverrides
  };
}

function sampleAlbum(fieldOverrides: Partial<AlbumDetail> = {}): AlbumDetail {
  return {
    id: 2,
    bandId: 9,
    name: 'First Light',
    archived: false,
    approvalRule: 'all',
    tracks: [
      sampleTrack(),
      sampleTrack({
        id: 2,
        songId: 7,
        songName: 'Dawn Chorus',
        sortOrder: 2,
        order: {
          myRank: 1,
          averageRank: 1.5,
          consensusPosition: 1
        }
      })
    ],
    proposals: [],
    order: {
      locked: false,
      voteCount: 1,
      mySongIds: [7, 4]
    },
    ...fieldOverrides
  };
}

describe('order-vote', () => {
  it('reads tallies, drafts a complete ranking, and moves a take', () => {
    const listedAlbum = sampleAlbum();
    const listedTrack = sampleTrack();

    expect(albumOrderContest(listedAlbum).voteCount).toBe(1);
    expect(albumOrderContest({
      ...listedAlbum,
      order: undefined
    }).locked).toBe(false);

    expect(trackOrderTally(listedTrack).consensusPosition).toBe(2);
    expect(trackOrderTally({
      ...listedTrack,
      order: undefined
    }).myRank).toBeNull();

    const listedTally = trackOrderTally(listedTrack);
    const staleAlbum = {
      ...listedAlbum,
      order: {
        locked: false,
        voteCount: 0,
        mySongIds: [4]
      }
    };

    expect(consensusRankLabel(listedTally)).toBe('Avg 1.5 · #2');
    expect(isCompleteOrder([7, 4], [4, 7])).toBe(true);
    expect(isCompleteOrder([4], [4, 7])).toBe(false);
    expect(draftOrderSongIds(listedAlbum)).toEqual([7, 4]);
    expect(draftOrderSongIds(staleAlbum)).toEqual([4, 7]);
    expect(moveRankedSong([4, 7], 4, 1)).toEqual([7, 4]);
    expect(moveRankedSong([4, 7], 4, -1)).toEqual([4, 7]);
    expect(canLockOrderContest(true, listedAlbum)).toBe(true);
    expect(canUnlockOrderContest(true, listedAlbum)).toBe(false);
    expect(canUnlockOrderContest(true, sampleAlbum({
      order: {
        locked: true,
        voteCount: 1,
        mySongIds: [7, 4]
      }
    }))).toBe(true);

    expect(canUnlockOrderContest(false, sampleAlbum({
      order: {
        locked: true,
        voteCount: 1,
        mySongIds: [7, 4]
      }
    }))).toBe(false);
  });
});
