import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { StudioPageComponent } from './studio-page.component';
import { credentialsInterceptor } from '../../core/credentials.interceptor';
import {
  Member,
  MemberSummary,
  Song
} from '../../core/models';

describe('StudioPageComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudioPageComponent],
      providers: [
        provideHttpClient(withInterceptors([credentialsInterceptor])),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    }).compileComponents();
  });

  it('uses the Google session member and loads that band catalog', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    const sessionMember: Member = {
      id: 4,
      userIdentifier: 'u',
      username: 'ada',
      fullname: 'Ada Vale',
      image: null,
      email: 'ada@example.com',
      bands: [
        {
          id: 9,
          name: 'Kindred',
          genre: 'indie',
          image: null,
          driveFolderId: 'folder-root',
          driveFolderName: 'Kindred takes',
          myRole: 'owner',
          isOwner: true
        }
      ],
      roles: [{ id: 3, roleName: 'owner', bandId: 9 }]
    };

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: true,
      connected: true,
      email: 'ada@example.com',
      member: sessionMember
    });

    const loadedSongs: Song[] = [
      {
        id: 4,
        name: 'Night Shift',
        description: 'Demo',
        uploadedBy: 1,
        version: 1,
        previousVersion: 0,
        storageKind: 'url'
      }
    ];

    httpTestingController.expectOne('/api/bands/9/songs').flush(loadedSongs);
    httpTestingController.expectOne('/api/bands/9/drive/files').flush([{ id: 'file-bottleneck', name: 'bottleneck.mp3', mimeType: 'audio/mpeg' }]);

    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;

    expect(renderedRoot.textContent).toContain('Ada Vale');
    expect(renderedRoot.textContent).toContain('Kindred');
    expect(renderedRoot.textContent).toContain('Night Shift');
    expect(renderedRoot.textContent).toContain('bottleneck.mp3');
    expect(renderedRoot.querySelector('.session select')).toBeNull();
    httpTestingController.verify();
  });

  it('shows a version chain for a same-name Drive take', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: true,
      connected: true,
      email: 'ada@example.com',
      member: {
        id: 4,
        userIdentifier: 'u',
        username: 'ada',
        fullname: 'Ada Vale',
        image: null,
        email: 'ada@example.com',
        bands: [
          {
            id: 9,
            name: 'Kindred',
            genre: 'indie',
            image: null,
            driveFolderId: 'folder-root',
            driveFolderName: 'Kindred takes',
            myRole: 'owner',
            isOwner: true
          }
        ],
        roles: []
      }
    });

    httpTestingController.expectOne('/api/bands/9/songs').flush([
      {
        id: 7,
        name: 'bottleneck.mp3',
        description: 'Linked from Google Drive',
        uploadedBy: 4,
        version: 1,
        previousVersion: 0,
        storageKind: 'gdrive',
        contentMd5: 'abc123',
        sourceModifiedAt: '2026-09-01T08:00:00Z'
      },
      {
        id: 8,
        name: 'bottleneck.mp3',
        description: 'Linked from Google Drive',
        uploadedBy: 4,
        version: 2,
        previousVersion: 7,
        storageKind: 'gdrive',
        contentMd5: 'def456',
        sourceModifiedAt: '2026-09-05T08:00:00Z'
      }
    ]);

    httpTestingController.expectOne('/api/bands/9/drive/files').flush([]);

    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;

    const versionCells = renderedRoot.querySelectorAll('.take-row .mono');

    expect(Array.from(versionCells).some((cell) => cell.textContent?.includes('\u2190'))).toBeTrue();
    expect(renderedRoot.textContent).toContain('2026-09-05');
    httpTestingController.verify();
  });

  it('links a Drive file with the session cookie and not X-Member-Id', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: true,
      connected: true,
      email: 'ada@example.com',
      member: {
        id: 4,
        userIdentifier: 'u',
        username: 'ada',
        fullname: 'Ada Vale',
        image: null,
        email: 'ada@example.com',
        bands: [
          {
            id: 9,
            name: 'Kindred',
            genre: 'indie',
            image: null,
            driveFolderId: 'folder-root',
            driveFolderName: 'Kindred takes',
            myRole: 'owner',
            isOwner: true
          }
        ],
        roles: []
      }
    });

    httpTestingController.expectOne('/api/bands/9/songs').flush([]);
    httpTestingController.expectOne('/api/bands/9/drive/files').flush([{ id: 'file-bottleneck', name: 'bottleneck.mp3', mimeType: 'audio/mpeg' }]);

    pageComponent.linkAndPlayDriveFile({
      id: 'file-bottleneck',
      name: 'bottleneck.mp3',
      mimeType: 'audio/mpeg'
    });

    const linkRequest = httpTestingController.expectOne('/api/bands/9/songs');

    expect(linkRequest.request.method).toBe('POST');
    expect(linkRequest.request.body).toEqual({
      driveFileId: 'file-bottleneck',
      name: 'bottleneck.mp3'
    });

    expect(linkRequest.request.headers.get('X-Member-Id')).toBeNull();
    expect(linkRequest.request.withCredentials).toBeTrue();

    linkRequest.flush({
      id: 7,
      name: 'bottleneck.mp3',
      description: 'Linked from Google Drive',
      uploadedBy: 4,
      version: 1,
      previousVersion: 0,
      storageKind: 'gdrive'
    });

    expect(pageComponent.playingId()).toBe(7);
    httpTestingController.verify();
  });

  it('creates an invite from the owner session', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: true,
      connected: true,
      email: 'ada@example.com',
      member: {
        id: 4,
        userIdentifier: 'u',
        username: 'ada',
        fullname: 'Ada Vale',
        image: null,
        email: 'ada@example.com',
        bands: [
          {
            id: 9,
            name: 'Kindred',
            genre: 'indie',
            image: null,
            driveFolderId: 'folder-root',
            driveFolderName: 'Kindred',
            myRole: 'owner',
            isOwner: true
          }
        ],
        roles: []
      }
    });

    httpTestingController.expectOne('/api/bands/9/songs').flush([]);
    httpTestingController.expectOne('/api/bands/9/drive/files').flush([]);

    pageComponent.inviteEmail = 'kit@example.com';
    pageComponent.inviteRole = 'uploader';
    pageComponent.sendInvite();

    const inviteRequest = httpTestingController.expectOne('/api/bands/9/invites');

    expect(inviteRequest.request.body).toEqual({
      email: 'kit@example.com',
      role: 'uploader'
    });

    inviteRequest.flush({
      id: 1,
      email: 'kit@example.com',
      roleName: 'uploader',
      code: 'abc12',
      acceptUrl: 'http://localhost:4200/join?code=abc12&email=kit%40example.com',
      emailSent: false,
      expiresAt: '2026-09-18T00:00:00Z'
    });

    expect(pageComponent.lastInvite()?.code).toBe('abc12');
    httpTestingController.verify();
  });

  it('loads the public demo catalog when nobody is signed in', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: false,
      connected: false,
      email: null,
      member: null
    });

    const memberSummaryList: MemberSummary[] = [
      {
        id: 1,
        userIdentifier: 'u',
        username: 'ada',
        fullname: 'Ada Vale',
        image: null,
        email: null
      }
    ];

    httpTestingController.expectOne('/api/members').flush(memberSummaryList);
    httpTestingController.expectOne('/api/members/1').flush({
      id: 1,
      userIdentifier: 'u',
      username: 'ada',
      fullname: 'Ada Vale',
      image: null,
      email: null,
      bands: [
        {
          id: 9,
          name: 'Kindred',
          genre: 'indie',
          image: null,
          driveFolderId: null,
          driveFolderName: null,
          myRole: 'owner',
          isOwner: true
        }
      ],
      roles: []
    });

    httpTestingController.expectOne('/api/bands/9/songs').flush([
      {
        id: 4,
        name: 'Night Shift',
        description: 'Demo',
        uploadedBy: 1,
        version: 1,
        previousVersion: 0,
        storageKind: 'url'
      }
    ]);

    componentFixture.detectChanges();

    expect((componentFixture.nativeElement as HTMLElement).textContent).toContain('Night Shift');
    httpTestingController.verify();
  });

  it('opens the album desk and records an approval', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: true,
      connected: true,
      email: 'ada@example.com',
      member: {
        id: 4,
        userIdentifier: 'u',
        username: 'ada',
        fullname: 'Ada Vale',
        image: null,
        email: 'ada@example.com',
        bands: [
          {
            id: 9,
            name: 'Kindred',
            genre: 'indie',
            image: null,
            driveFolderId: 'folder-root',
            driveFolderName: 'Kindred',
            myRole: 'owner',
            isOwner: true
          }
        ],
        roles: []
      }
    });

    httpTestingController.expectOne('/api/bands/9/songs').flush([
      {
        id: 4,
        name: 'Night Shift',
        description: 'Demo',
        uploadedBy: 1,
        version: 1,
        previousVersion: 0,
        storageKind: 'url'
      }
    ]);

    httpTestingController.expectOne('/api/bands/9/drive/files').flush([]);

    pageComponent.showAlbumsPanel();

    httpTestingController.expectOne('/api/bands/9/albums').flush([
      {
        id: 2,
        bandId: 9,
        name: 'First Light',
        archived: false,
        approvalRule: 'all',
        trackCount: 0,
        openProposalCount: 1
      }
    ]);

    pageComponent.selectAlbum({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      trackCount: 0,
      openProposalCount: 1
    });

    httpTestingController.expectOne('/api/albums/2').flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [],
      proposals: [
        {
          id: 8,
          albumId: 2,
          songId: 4,
          songName: 'Night Shift',
          songVersion: 1,
          proposedBy: 4,
          proposedByName: 'Ada Vale',
          status: 'open',
          createdAt: '2026-09-04T00:00:00Z',
          resolvedAt: null,
          decisions: [],
          waitingOn: [{ memberId: 5, name: 'Kit Reed' }],
          reviews: []
        }
      ]
    });

    pageComponent.openProposalPane({
      id: 8,
      albumId: 2,
      songId: 4,
      songName: 'Night Shift',
      songVersion: 1,
      proposedBy: 4,
      proposedByName: 'Ada Vale',
      status: 'open',
      createdAt: '2026-09-04T00:00:00Z',
      resolvedAt: null,
      decisions: [],
      waitingOn: [{ memberId: 5, name: 'Kit Reed' }],
      reviews: []
    });

    pageComponent.decideProposal('approve');

    const decisionRequest = httpTestingController.expectOne('/api/albums/2/proposals/8/decisions');

    expect(decisionRequest.request.body).toEqual({ decision: 'approve' });

    decisionRequest.flush({
      id: 8,
      albumId: 2,
      songId: 4,
      songName: 'Night Shift',
      songVersion: 1,
      proposedBy: 4,
      proposedByName: 'Ada Vale',
      status: 'open',
      createdAt: '2026-09-04T00:00:00Z',
      resolvedAt: null,
      decisions: [
        {
          memberId: 4,
          memberName: 'Ada Vale',
          decision: 'approve',
          updatedAt: '2026-09-04T00:01:00Z'
        }
      ],
      waitingOn: [{ memberId: 5, name: 'Kit Reed' }],
      reviews: []
    });

    httpTestingController.expectOne('/api/albums/2').flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [],
      proposals: []
    });

    expect(pageComponent.openProposal()?.decisions[0].decision).toBe('approve');
    httpTestingController.verify();
  });

  it('adds a timestamped review and seeks the take on click', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: true,
      connected: true,
      email: 'ada@example.com',
      member: {
        id: 4,
        userIdentifier: 'u',
        username: 'ada',
        fullname: 'Ada Vale',
        image: null,
        email: 'ada@example.com',
        bands: [
          {
            id: 9,
            name: 'Kindred',
            genre: 'indie',
            image: null,
            driveFolderId: 'folder-root',
            driveFolderName: 'Kindred',
            myRole: 'owner',
            isOwner: true
          }
        ],
        roles: []
      }
    });

    httpTestingController.expectOne('/api/bands/9/songs').flush([
      {
        id: 4,
        name: 'Night Shift',
        description: 'Demo',
        uploadedBy: 1,
        version: 1,
        previousVersion: 0,
        storageKind: 'url'
      }
    ]);

    httpTestingController.expectOne('/api/bands/9/drive/files').flush([]);

    pageComponent.showAlbumsPanel();
    httpTestingController.expectOne('/api/bands/9/albums').flush([
      {
        id: 2,
        bandId: 9,
        name: 'First Light',
        archived: false,
        approvalRule: 'all',
        trackCount: 0,
        openProposalCount: 1
      }
    ]);

    pageComponent.selectAlbum({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      trackCount: 0,
      openProposalCount: 1
    });

    httpTestingController.expectOne('/api/albums/2').flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [],
      proposals: [
        {
          id: 8,
          albumId: 2,
          songId: 4,
          songName: 'Night Shift',
          songVersion: 1,
          proposedBy: 4,
          proposedByName: 'Ada Vale',
          status: 'open',
          createdAt: '2026-09-04T00:00:00Z',
          resolvedAt: null,
          decisions: [],
          waitingOn: [{ memberId: 5, name: 'Kit Reed' }],
          reviews: []
        }
      ]
    });

    pageComponent.openProposalPane({
      id: 8,
      albumId: 2,
      songId: 4,
      songName: 'Night Shift',
      songVersion: 1,
      proposedBy: 4,
      proposedByName: 'Ada Vale',
      status: 'open',
      createdAt: '2026-09-04T00:00:00Z',
      resolvedAt: null,
      decisions: [],
      waitingOn: [{ memberId: 5, name: 'Kit Reed' }],
      reviews: []
    });

    pageComponent.reviewStartMs = 12500;
    pageComponent.reviewEndMs = 18300;
    pageComponent.reviewBody = 'Snare is late';
    pageComponent.addProposalReview();

    const reviewRequest = httpTestingController.expectOne('/api/albums/2/proposals/8/reviews');

    expect(reviewRequest.request.body).toEqual({
      startMs: 12500,
      endMs: 18300,
      body: 'Snare is late'
    });

    const savedReview = {
      id: 11,
      memberId: 4,
      memberName: 'Ada Vale',
      startMs: 12500,
      endMs: 18300,
      body: 'Snare is late',
      createdAt: '2026-09-04T00:02:00Z'
    };

    reviewRequest.flush({
      id: 8,
      albumId: 2,
      songId: 4,
      songName: 'Night Shift',
      songVersion: 1,
      proposedBy: 4,
      proposedByName: 'Ada Vale',
      status: 'open',
      createdAt: '2026-09-04T00:00:00Z',
      resolvedAt: null,
      decisions: [],
      waitingOn: [{ memberId: 5, name: 'Kit Reed' }],
      reviews: [savedReview]
    });

    httpTestingController.expectOne('/api/albums/2').flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [],
      proposals: [
        {
          id: 8,
          albumId: 2,
          songId: 4,
          songName: 'Night Shift',
          songVersion: 1,
          proposedBy: 4,
          proposedByName: 'Ada Vale',
          status: 'open',
          createdAt: '2026-09-04T00:00:00Z',
          resolvedAt: null,
          decisions: [],
          waitingOn: [{ memberId: 5, name: 'Kit Reed' }],
          reviews: [savedReview]
        }
      ]
    });

    expect(pageComponent.openProposal()?.reviews[0].body).toBe('Snare is late');

    pageComponent.seekProposalReview(savedReview);

    expect(pageComponent.playingId()).toBe(4);
    expect(pageComponent.seekMs()).toBe(12500);
    expect(pageComponent.seekEpoch()).toBe(1);
    httpTestingController.verify();
  });

  it('shows the Drive folder error when propose is rejected as outside the folder', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: true,
      connected: true,
      email: 'ada@example.com',
      member: {
        id: 4,
        userIdentifier: 'u',
        username: 'ada',
        fullname: 'Ada Vale',
        image: null,
        email: 'ada@example.com',
        bands: [
          {
            id: 9,
            name: 'Kindred',
            genre: 'indie',
            image: null,
            driveFolderId: 'folder-root',
            driveFolderName: 'Kindred',
            myRole: 'owner',
            isOwner: true
          }
        ],
        roles: []
      }
    });

    httpTestingController.expectOne('/api/bands/9/songs').flush([]);
    httpTestingController.expectOne('/api/bands/9/drive/files').flush([]);

    pageComponent.showAlbumsPanel();
    httpTestingController.expectOne('/api/bands/9/albums').flush([
      {
        id: 2,
        bandId: 9,
        name: 'First Light',
        archived: false,
        approvalRule: 'all',
        trackCount: 0,
        openProposalCount: 0
      }
    ]);

    pageComponent.selectAlbum({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      trackCount: 0,
      openProposalCount: 0
    });

    httpTestingController.expectOne('/api/albums/2').flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [],
      proposals: []
    });

    pageComponent.proposeSongId = 6;
    pageComponent.proposeSong();

    const proposeRequest = httpTestingController.expectOne('/api/albums/2/proposals');

    expect(proposeRequest.request.body).toEqual({ songId: 6 });

    proposeRequest.flush(
      { error: 'File is outside the band Drive folder' },
      { status: 404, statusText: 'Not Found' }
    );

    expect(pageComponent.loadError()).toBe('File is outside the band Drive folder');
    httpTestingController.verify();
  });
});
