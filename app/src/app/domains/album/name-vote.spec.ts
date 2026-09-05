import { AlbumTrack } from '../../core/models';
import {
  albumNameContest,
  canSubmitNameVote,
  trackNameContest,
  trimmedNameVote
} from './name-vote';

function sampleTrack(fieldOverrides: Partial<AlbumTrack> = {}): AlbumTrack {
  return {
    id: 1,
    songId: 4,
    songName: 'Night Shift',
    version: 1,
    sortOrder: 1,
    addedAt: '2026-09-05T00:00:00Z',
    names: {
      myName: 'Night Shift (radio)',
      candidates: [
        {
          name: 'Night Shift (radio)',
          voteCount: 2,
          isMine: true
        },
        {
          name: 'Night Shift',
          voteCount: 1,
          isMine: false
        }
      ]
    },
    ...fieldOverrides
  };
}

describe('name-vote', () => {
  it('reads album and track contests and rejects a blank title', () => {
    const listedTrack = sampleTrack();

    expect(trackNameContest(listedTrack).myName).toBe('Night Shift (radio)');
    expect(trackNameContest({
      ...listedTrack,
      names: undefined
    }).candidates.length).toBe(0);

    expect(albumNameContest({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [],
      proposals: [],
      names: {
        myName: 'Midnight Sun',
        candidates: [
          {
            name: 'Midnight Sun',
            voteCount: 1,
            isMine: true
          }
        ]
      }
    }).myName).toBe('Midnight Sun');

    expect(trimmedNameVote('  Dawn Chorus  ')).toBe('Dawn Chorus');
    expect(canSubmitNameVote('   ')).toBe(false);
    expect(canSubmitNameVote('Midnight Sun')).toBe(true);
  });
});
