import { Song } from '../../core/models';

export function takeVersionLabel(listedTake: Song, listedTakes: Song[]) {
  const versionNumber = listedTake.version || 1;
  const previousTake = listedTakes.find((catalogTake) => catalogTake.id === listedTake.previousVersion);

  if (!previousTake) {
    return String(versionNumber);
  }

  const previousNumber = previousTake.version || 1;

  return `${versionNumber} ← ${previousNumber}`;
}

export function takeSourceDateLabel(listedTake: Song) {
  const sourceStamp = listedTake.sourceModifiedAt;

  if (!sourceStamp) {
    return '';
  }

  return sourceStamp.slice(0, 10);
}
