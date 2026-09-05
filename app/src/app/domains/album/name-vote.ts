import {
  AlbumDetail,
  AlbumTrack,
  NameContest
} from '../../core/models';

export function emptyNameContest(): NameContest {
  return {
    myName: null,
    candidates: []
  };
}

export function albumNameContest(listedAlbum: AlbumDetail | null | undefined) {
  return listedAlbum?.names ?? emptyNameContest();
}

export function trackNameContest(listedTrack: AlbumTrack) {
  return listedTrack.names ?? emptyNameContest();
}

export function trimmedNameVote(listedName: string) {
  return listedName.trim();
}

export function canSubmitNameVote(listedName: string) {
  const trimmedName = trimmedNameVote(listedName);

  return trimmedName.length > 0 && trimmedName.length <= 50;
}
