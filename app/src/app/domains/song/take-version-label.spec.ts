import { Song } from '../../core/models';
import {
  takeSourceDateLabel,
  takeVersionLabel
} from './take-version-label';

function sampleTake(fieldOverrides: Partial<Song>): Song {
  return {
    id: 1,
    name: 'bottleneck.mp3',
    description: null,
    uploadedBy: 4,
    version: 1,
    previousVersion: 0,
    storageKind: 'gdrive',
    contentMd5: null,
    sourceModifiedAt: null,
    ...fieldOverrides
  };
}

describe('takeVersionLabel', () => {
  it('shows the version chain when the prior take is in the catalog', () => {
    const firstTake = sampleTake({ id: 7, version: 1 });
    const secondTake = sampleTake({ id: 8, version: 2, previousVersion: 7 });
    const versionChain = takeVersionLabel(secondTake, [firstTake, secondTake]);

    expect(versionChain).toBe('2 ← 1');
  });

  it('shows only the version when the prior take is missing', () => {
    const orphanTake = sampleTake({ version: 2, previousVersion: 7 });
    const versionNumber = takeVersionLabel(orphanTake, []);

    expect(versionNumber).toBe('2');
  });
});

describe('takeSourceDateLabel', () => {
  it('keeps the calendar day from an ISO stamp', () => {
    const datedTake = sampleTake({ sourceModifiedAt: '2026-09-05T08:00:00Z' });
    const sourceDay = takeSourceDateLabel(datedTake);

    expect(sourceDay).toBe('2026-09-05');
  });
});
