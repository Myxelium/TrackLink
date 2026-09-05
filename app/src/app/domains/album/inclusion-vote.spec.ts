import { AlbumTrack } from '../../core/models';
import {
  inclusionChoiceLabel,
  inclusionCountFor,
  trackInclusion
} from './inclusion-vote';

function sampleTrack(fieldOverrides: Partial<AlbumTrack> = {}): AlbumTrack {
  return {
    id: 1,
    songId: 4,
    songName: 'Night Shift',
    version: 1,
    sortOrder: 1,
    addedAt: '2026-09-05T00:00:00Z',
    inclusion: {
      myChoice: 'in',
      inCount: 2,
      outCount: 1,
      abstainCount: 0
    },
    ...fieldOverrides
  };
}

describe('inclusion-vote', () => {
  it('reads the tally for a choice and falls back when the track has none', () => {
    const listedTrack = sampleTrack();

    expect(inclusionChoiceLabel('in')).toBe('In');
    expect(inclusionCountFor(listedTrack, 'in')).toBe(2);
    expect(inclusionCountFor(listedTrack, 'out')).toBe(1);
    expect(trackInclusion({
      ...listedTrack,
      inclusion: undefined
    }).inCount).toBe(0);
  });
});
