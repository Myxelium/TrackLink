import {
  AlbumDetail,
  AlbumTrack,
  OrderContest,
  OrderTrackTally
} from '../../core/models';

export function emptyOrderContest(): OrderContest {
  return {
    locked: false,
    voteCount: 0,
    mySongIds: null
  };
}

export function emptyOrderTrackTally(): OrderTrackTally {
  return {
    myRank: null,
    averageRank: null,
    consensusPosition: null
  };
}

export function albumOrderContest(listedAlbum: AlbumDetail | null | undefined) {
  return listedAlbum?.order ?? emptyOrderContest();
}

export function canManageOrderContest(
  canManage: boolean,
  listedAlbum: AlbumDetail | null | undefined
) {
  return Boolean(canManage && listedAlbum && !listedAlbum.archived);
}

export function canLockOrderContest(
  canManage: boolean,
  listedAlbum: AlbumDetail | null | undefined
) {
  return canManageOrderContest(canManage, listedAlbum) && !albumOrderContest(listedAlbum).locked;
}

export function canUnlockOrderContest(
  canManage: boolean,
  listedAlbum: AlbumDetail | null | undefined
) {
  return canManageOrderContest(canManage, listedAlbum) && albumOrderContest(listedAlbum).locked;
}

export function trackOrderTally(listedTrack: AlbumTrack) {
  return listedTrack.order ?? emptyOrderTrackTally();
}

export function isCompleteOrder(songIds: number[], admittedSongIds: number[]) {
  if (songIds.length !== admittedSongIds.length) {
    return false;
  }

  const wantedIds = new Set(admittedSongIds);
  const seenIds = new Set<number>();

  for (const songId of songIds) {
    if (!wantedIds.has(songId) || seenIds.has(songId)) {
      return false;
    }

    seenIds.add(songId);
  }

  return true;
}

export function draftOrderSongIds(listedAlbum: AlbumDetail | null | undefined) {
  const albumTracks = listedAlbum?.tracks ?? [];
  const admittedIds = albumTracks.map((listedTrack) => listedTrack.songId);
  const mySongIds = listedAlbum?.order?.mySongIds;

  if (mySongIds && isCompleteOrder(mySongIds, admittedIds)) {
    return [...mySongIds];
  }

  return admittedIds;
}

export function moveRankedSong(songIds: number[], songId: number, rankDelta: number) {
  const nextIds = [...songIds];
  const currentIndex = nextIds.indexOf(songId);
  const nextIndex = currentIndex + rankDelta;

  if (currentIndex < 0 || nextIndex < 0 || nextIndex >= nextIds.length) {
    return nextIds;
  }

  const [movedId] = nextIds.splice(currentIndex, 1);

  nextIds.splice(nextIndex, 0, movedId);

  return nextIds;
}

export function averageRankLabel(averageRank: number | null) {
  if (averageRank == null) {
    return '-';
  }

  if (Number.isInteger(averageRank)) {
    return String(averageRank);
  }

  return averageRank.toFixed(1);
}

export function consensusRankLabel(listedTally: OrderTrackTally) {
  if (listedTally.consensusPosition == null) {
    return 'No ranks yet';
  }

  return `Avg ${averageRankLabel(listedTally.averageRank)} · #${listedTally.consensusPosition}`;
}
