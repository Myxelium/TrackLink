import {
  AlbumDetail,
  ArtCandidate,
  ArtContest,
  DriveFile
} from '../../core/models';

export interface ArtPollChoice {
  driveFileId: string;
  label: string;
  voteCount: number;
  isMine: boolean;
  previewUrl: string;
}

export function emptyArtContest(): ArtContest {
  return {
    locked: false,
    appliedDriveFileId: null,
    voteCount: 0,
    myDriveFileId: null,
    candidates: []
  };
}

export function albumArtContest(listedAlbum: AlbumDetail | null | undefined) {
  return listedAlbum?.art ?? emptyArtContest();
}

export function trimmedArtFileId(listedFileId: string) {
  return listedFileId.trim();
}

export function canSubmitArtVote(listedFileId: string) {
  const driveFileId = trimmedArtFileId(listedFileId);

  return /^[A-Za-z0-9_-]{1,128}$/.test(driveFileId);
}

export function artCandidateLabel(driveFileId: string, driveFiles: DriveFile[]) {
  return driveFiles.find((driveFile) => driveFile.id === driveFileId)?.name ?? driveFileId;
}

export function albumArtPreviewUrl(albumId: number, driveFileId?: string | null) {
  const previewFileId = driveFileId?.trim();

  if (previewFileId) {
    return `/api/albums/${albumId}/art?fileId=${encodeURIComponent(previewFileId)}`;
  }

  return `/api/albums/${albumId}/art`;
}

export function albumArtPollChoices(
  listedAlbum: AlbumDetail | null | undefined,
  driveFiles: DriveFile[]
) {
  if (!listedAlbum) {
    return [];
  }

  const byFileId = new Map<string, ArtPollChoice>();

  for (const driveFile of driveFiles) {
    byFileId.set(driveFile.id, {
      driveFileId: driveFile.id,
      label: driveFile.name,
      voteCount: 0,
      isMine: false,
      previewUrl: albumArtPreviewUrl(listedAlbum.id, driveFile.id)
    });
  }

  for (const listedCandidate of albumArtContest(listedAlbum).candidates) {
    const existingChoice = byFileId.get(listedCandidate.driveFileId);

    if (existingChoice) {
      existingChoice.voteCount = listedCandidate.voteCount;
      existingChoice.isMine = listedCandidate.isMine;
    } else {
      byFileId.set(
        listedCandidate.driveFileId,
        artPollChoiceFromCandidate(listedAlbum.id, listedCandidate, driveFiles)
      );
    }
  }

  return [...byFileId.values()];
}

export function artPollChoiceLabel(listedChoice: ArtPollChoice) {
  return listedChoice.voteCount > 0
    ? `${listedChoice.label} ${listedChoice.voteCount}`
    : listedChoice.label;
}

function artPollChoiceFromCandidate(
  albumId: number,
  listedCandidate: ArtCandidate,
  driveFiles: DriveFile[]
): ArtPollChoice {
  return {
    driveFileId: listedCandidate.driveFileId,
    label: artCandidateLabel(listedCandidate.driveFileId, driveFiles),
    voteCount: listedCandidate.voteCount,
    isMine: listedCandidate.isMine,
    previewUrl: albumArtPreviewUrl(albumId, listedCandidate.driveFileId)
  };
}

export function canManageArtContest(
  canManage: boolean,
  listedAlbum: AlbumDetail | null | undefined
) {
  return Boolean(canManage && listedAlbum && !listedAlbum.archived);
}

export function canLockArtContest(
  canManage: boolean,
  listedAlbum: AlbumDetail | null | undefined
) {
  return canManageArtContest(canManage, listedAlbum) && !albumArtContest(listedAlbum).locked;
}

export function canUnlockArtContest(
  canManage: boolean,
  listedAlbum: AlbumDetail | null | undefined
) {
  return canManageArtContest(canManage, listedAlbum) && albumArtContest(listedAlbum).locked;
}

export function artResolutionWarning(listedWarning: string | null | undefined) {
  const trimmedWarning = listedWarning?.trim();

  return trimmedWarning ? trimmedWarning : null;
}
