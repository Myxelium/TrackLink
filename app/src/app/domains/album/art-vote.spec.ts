import { AlbumDetail } from '../../core/models';
import {
  albumArtContest,
  albumArtPollChoices,
  albumArtPreviewUrl,
  artCandidateLabel,
  artPollChoiceLabel,
  artResolutionWarning,
  canLockArtContest,
  canSubmitArtVote,
  canUnlockArtContest,
  trimmedArtFileId
} from './art-vote';

function sampleAlbum(fieldOverrides: Partial<AlbumDetail> = {}): AlbumDetail {
  return {
    id: 2,
    bandId: 9,
    name: 'First Light',
    archived: false,
    approvalRule: 'all',
    tracks: [],
    proposals: [],
    art: {
      locked: false,
      appliedDriveFileId: null,
      voteCount: 1,
      myDriveFileId: 'cover-dawn',
      candidates: [
        {
          driveFileId: 'cover-dawn',
          voteCount: 1,
          isMine: true
        }
      ]
    },
    ...fieldOverrides
  };
}

describe('art-vote', () => {
  it('reads the album art contest and accepts a Drive file id', () => {
    const listedAlbum = sampleAlbum();

    expect(albumArtContest(listedAlbum).myDriveFileId).toBe('cover-dawn');
    expect(albumArtContest({
      ...listedAlbum,
      art: undefined
    }).voteCount).toBe(0);

    expect(trimmedArtFileId('  cover-dawn  ')).toBe('cover-dawn');
    expect(canSubmitArtVote('   ')).toBe(false);
    expect(canSubmitArtVote('../outside')).toBe(false);
    expect(canSubmitArtVote('cover-dawn')).toBe(true);
    expect(artCandidateLabel('cover-dawn', [
      {
        id: 'cover-dawn',
        name: 'dawn.jpg',
        mimeType: 'image/jpeg'
      }
    ])).toBe('dawn.jpg');

    expect(artCandidateLabel('cover-dusk', [])).toBe('cover-dusk');
    expect(albumArtPreviewUrl(2)).toBe('/api/albums/2/art');
    expect(albumArtPreviewUrl(2, 'cover-dawn')).toBe('/api/albums/2/art?fileId=cover-dawn');
    expect(albumArtPreviewUrl(2, '  cover-dawn  ')).toBe('/api/albums/2/art?fileId=cover-dawn');
    expect(canLockArtContest(true, listedAlbum)).toBe(true);
    expect(canUnlockArtContest(true, listedAlbum)).toBe(false);

    const lockedArtAlbum = sampleAlbum({
      art: {
        locked: true,
        appliedDriveFileId: 'cover-dawn',
        voteCount: 1,
        myDriveFileId: 'cover-dawn',
        candidates: albumArtContest(listedAlbum).candidates
      }
    });

    expect(canUnlockArtContest(true, lockedArtAlbum)).toBe(true);
    expect(canUnlockArtContest(false, lockedArtAlbum)).toBe(false);

    const pollChoices = albumArtPollChoices(listedAlbum, [
      {
        id: 'cover-dawn',
        name: 'dawn.jpg',
        mimeType: 'image/jpeg'
      },
      {
        id: 'cover-dusk',
        name: 'dusk.jpg',
        mimeType: 'image/jpeg'
      }
    ]);

    expect(pollChoices.map((listedChoice) => listedChoice.driveFileId)).toEqual(['cover-dawn', 'cover-dusk']);
    expect(pollChoices[0].previewUrl).toBe('/api/albums/2/art?fileId=cover-dawn');
    expect(artPollChoiceLabel(pollChoices[0])).toBe('dawn.jpg 1');
    expect(artPollChoiceLabel(pollChoices[1])).toBe('dusk.jpg');
    expect(albumArtPollChoices(null, [])).toEqual([]);
    expect(artResolutionWarning('  Cover is 400x400. Recommended size is 1600-3000 pixels on each side.  '))
      .toBe('Cover is 400x400. Recommended size is 1600-3000 pixels on each side.');

    expect(artResolutionWarning('   ')).toBeNull();
    expect(artResolutionWarning(null)).toBeNull();
  });
});
