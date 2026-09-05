import { AlbumTrack, InclusionTally } from '../../core/models';

export const inclusionChoices = [
  'in',
  'out',
  'abstain'
] as const;

export type InclusionChoice = (typeof inclusionChoices)[number];

export function emptyInclusionTally(): InclusionTally {
  return {
    myChoice: null,
    inCount: 0,
    outCount: 0,
    abstainCount: 0
  };
}

export function trackInclusion(listedTrack: AlbumTrack) {
  return listedTrack.inclusion ?? emptyInclusionTally();
}

export function inclusionChoiceLabel(listedChoice: string) {
  if (listedChoice === 'in') {
    return 'In';
  }

  if (listedChoice === 'out') {
    return 'Out';
  }

  return 'Abstain';
}

export function inclusionCountFor(listedTrack: AlbumTrack, listedChoice: string) {
  const inclusionTally = trackInclusion(listedTrack);

  if (listedChoice === 'in') {
    return inclusionTally.inCount;
  }

  if (listedChoice === 'out') {
    return inclusionTally.outCount;
  }

  return inclusionTally.abstainCount;
}
