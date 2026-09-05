import { ComponentFixture, TestBed } from '@angular/core/testing';
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

  it('filters the Takes sheet through band song search', () => {
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

    const catalogSongs: Song[] = [
      {
        id: 4,
        name: 'Night Shift',
        description: 'Demo take',
        uploadedBy: 1,
        version: 1,
        previousVersion: 0,
        storageKind: 'url'
      },
      {
        id: 8,
        name: 'bottleneck.mp3',
        description: 'Linked from Google Drive',
        uploadedBy: 4,
        version: 2,
        previousVersion: 7,
        storageKind: 'gdrive'
      }
    ];

    httpTestingController.expectOne('/api/bands/9/songs').flush(catalogSongs);
    httpTestingController.expectOne('/api/bands/9/drive/files').flush([]);
    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;
    const searchField = renderedRoot.querySelector('input[type="search"]') as HTMLInputElement;

    expect(renderedRoot.textContent).toContain('Night Shift');
    expect(renderedRoot.textContent).toContain('bottleneck.mp3');
    expect(searchField).toBeTruthy();

    searchField.value = 'night';
    searchField.dispatchEvent(new Event('input'));

    httpTestingController.expectOne('/api/bands/9/songs?q=night').flush([catalogSongs[0]]);
    componentFixture.detectChanges();

    expect(renderedRoot.textContent).toContain('Night Shift');
    expect(renderedRoot.textContent).not.toContain('bottleneck.mp3');
    expect(renderedRoot.textContent).toContain('1 take');

    searchField.value = '';
    searchField.dispatchEvent(new Event('input'));
    componentFixture.detectChanges();

    expect(renderedRoot.textContent).toContain('Night Shift');
    expect(renderedRoot.textContent).toContain('bottleneck.mp3');
    expect(renderedRoot.textContent).toContain('2 takes');
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

  it('shows the API-down banner when the session request fails', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').error(new ProgressEvent('error'));
    httpTestingController.expectOne('/api/members').error(new ProgressEvent('error'));

    expect(pageComponent.loadError()).toBe(
      'Could not reach the TrackLink API. Start the API on port 5180.'
    );

    httpTestingController.verify();
  });

  it('does not call a failed invite an unreachable API', () => {
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
    pageComponent.sendInvite();

    httpTestingController.expectOne('/api/bands/9/invites').flush(
      { error: 'Sign in with Google first' },
      { status: 401, statusText: 'Unauthorized' }
    );

    expect(pageComponent.loadError()).toBe('Could not create that invite.');
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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);

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

  it('records an inclusion vote on an album take without touching proposals', () => {
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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
    httpTestingController.expectOne('/api/bands/9/albums').flush([
      {
        id: 2,
        bandId: 9,
        name: 'First Light',
        archived: false,
        approvalRule: 'all',
        trackCount: 1,
        openProposalCount: 0
      }
    ]);

    pageComponent.selectAlbum({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      trackCount: 1,
      openProposalCount: 0
    });

    httpTestingController.expectOne('/api/albums/2').flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          inclusion: {
            myChoice: null,
            inCount: 0,
            outCount: 0,
            abstainCount: 0
          }
        }
      ],
      proposals: []
    });

    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;
    const inButton = Array.from(renderedRoot.querySelectorAll('.inclusion-choices button'))
      .find((button) => button.textContent?.includes('In')) as HTMLButtonElement;

    expect(inButton).toBeTruthy();
    inButton.click();

    const voteRequest = httpTestingController.expectOne('/api/albums/2/inclusion-votes');

    expect(voteRequest.request.method).toBe('PUT');
    expect(voteRequest.request.body).toEqual({ songId: 4, choice: 'in' });

    voteRequest.flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          inclusion: {
            myChoice: 'in',
            inCount: 1,
            outCount: 0,
            abstainCount: 0
          }
        }
      ],
      proposals: []
    });

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.tracks[0].inclusion?.myChoice).toBe('in');
    expect(renderedRoot.textContent).toContain('In 1');
    httpTestingController.verify();
  });

  it('records album and song title votes without treating them as a rename', () => {
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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
    httpTestingController.expectOne('/api/bands/9/albums').flush([
      {
        id: 2,
        bandId: 9,
        name: 'First Light',
        archived: false,
        approvalRule: 'all',
        trackCount: 1,
        openProposalCount: 0
      }
    ]);

    pageComponent.selectAlbum({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      trackCount: 1,
      openProposalCount: 0
    });

    httpTestingController.expectOne('/api/albums/2').flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      names: {
        myName: null,
        candidates: [
          {
            name: 'First Light',
            voteCount: 0,
            isMine: false
          }
        ]
      },
      tracks: [
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          inclusion: {
            myChoice: null,
            inCount: 0,
            outCount: 0,
            abstainCount: 0
          },
          names: {
            myName: null,
            candidates: [
              {
                name: 'Night Shift',
                voteCount: 0,
                isMine: false
              }
            ]
          }
        }
      ],
      proposals: []
    });

    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;
    const firstLightButton = Array.from(renderedRoot.querySelectorAll('.name-choices button'))
      .find((button) => button.textContent?.includes('First Light')) as HTMLButtonElement;

    expect(firstLightButton).toBeTruthy();
    firstLightButton.click();

    const albumNameRequest = httpTestingController.expectOne('/api/albums/2/name-votes');

    expect(albumNameRequest.request.method).toBe('PUT');
    expect(albumNameRequest.request.body).toEqual({
      songId: null,
      name: 'First Light'
    });

    albumNameRequest.flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      names: {
        myName: 'First Light',
        candidates: [
          {
            name: 'First Light',
            voteCount: 1,
            isMine: true
          }
        ]
      },
      tracks: [
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          inclusion: {
            myChoice: null,
            inCount: 0,
            outCount: 0,
            abstainCount: 0
          },
          names: {
            myName: null,
            candidates: [
              {
                name: 'Night Shift',
                voteCount: 0,
                isMine: false
              }
            ]
          }
        }
      ],
      proposals: []
    });

    componentFixture.detectChanges();

    const nightShiftButton = Array.from(renderedRoot.querySelectorAll('.name-choices button'))
      .find((button) => button.textContent?.includes('Night Shift')) as HTMLButtonElement;

    expect(pageComponent.openAlbum()?.name).toBe('First Light');
    expect(pageComponent.openAlbum()?.names?.myName).toBe('First Light');
    expect(renderedRoot.textContent).toContain('First Light 1');
    nightShiftButton.click();

    const songNameRequest = httpTestingController.expectOne('/api/albums/2/name-votes');

    expect(songNameRequest.request.body).toEqual({
      songId: 4,
      name: 'Night Shift'
    });

    songNameRequest.flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      names: {
        myName: 'First Light',
        candidates: [
          {
            name: 'First Light',
            voteCount: 1,
            isMine: true
          }
        ]
      },
      tracks: [
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          inclusion: {
            myChoice: null,
            inCount: 0,
            outCount: 0,
            abstainCount: 0
          },
          names: {
            myName: 'Night Shift',
            candidates: [
              {
                name: 'Night Shift',
                voteCount: 1,
                isMine: true
              }
            ]
          }
        }
      ],
      proposals: []
    });

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.tracks[0].songName).toBe('Night Shift');
    expect(pageComponent.openAlbum()?.tracks[0].names?.myName).toBe('Night Shift');
    expect(renderedRoot.textContent).toContain('Night Shift 1');
    httpTestingController.verify();
  });

  it('records a ranked order and locks it onto the album', () => {
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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
    httpTestingController.expectOne('/api/bands/9/albums').flush([
      {
        id: 2,
        bandId: 9,
        name: 'First Light',
        archived: false,
        approvalRule: 'all',
        trackCount: 2,
        openProposalCount: 0
      }
    ]);

    pageComponent.selectAlbum({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      trackCount: 2,
      openProposalCount: 0
    });

    httpTestingController.expectOne('/api/albums/2').flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          order: {
            myRank: null,
            averageRank: null,
            consensusPosition: null
          }
        },
        {
          id: 5,
          songId: 7,
          songName: 'Dawn Chorus',
          version: 1,
          sortOrder: 2,
          addedAt: '2026-09-05T00:00:00Z',
          order: {
            myRank: null,
            averageRank: null,
            consensusPosition: null
          }
        }
      ],
      proposals: [],
      order: {
        locked: false,
        voteCount: 0,
        mySongIds: null
      }
    });

    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;
    const downButton = Array.from(renderedRoot.querySelectorAll('.order-moves button'))
      .find((button) => button.textContent?.includes('Down')) as HTMLButtonElement;

    expect(downButton).toBeTruthy();
    downButton.click();
    componentFixture.detectChanges();

    const saveButton = Array.from(renderedRoot.querySelectorAll('.order-actions button'))
      .find((button) => button.textContent?.includes('Save ranking')) as HTMLButtonElement;

    expect(saveButton).toBeTruthy();
    saveButton.click();

    const voteRequest = httpTestingController.expectOne('/api/albums/2/order-votes');

    expect(voteRequest.request.method).toBe('PUT');
    expect(voteRequest.request.body).toEqual({ songIds: [7, 4] });

    voteRequest.flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          order: {
            myRank: 2,
            averageRank: 2,
            consensusPosition: 2
          }
        },
        {
          id: 5,
          songId: 7,
          songName: 'Dawn Chorus',
          version: 1,
          sortOrder: 2,
          addedAt: '2026-09-05T00:00:00Z',
          order: {
            myRank: 1,
            averageRank: 1,
            consensusPosition: 1
          }
        }
      ],
      proposals: [],
      order: {
        locked: false,
        voteCount: 1,
        mySongIds: [7, 4]
      }
    });

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.order?.mySongIds).toEqual([7, 4]);
    expect(renderedRoot.textContent).toContain('Avg 1 · #1');

    const lockButton = Array.from(renderedRoot.querySelectorAll('.order-actions button'))
      .find((button) => button.textContent?.includes('Lock order')) as HTMLButtonElement;

    expect(lockButton).toBeTruthy();
    lockButton.click();

    const lockRequest = httpTestingController.expectOne('/api/albums/2/order-lock');

    expect(lockRequest.request.method).toBe('PUT');
    expect(lockRequest.request.body).toEqual({ locked: true });

    lockRequest.flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [
        {
          id: 5,
          songId: 7,
          songName: 'Dawn Chorus',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          order: {
            myRank: 1,
            averageRank: 1,
            consensusPosition: 1
          }
        },
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 2,
          addedAt: '2026-09-05T00:00:00Z',
          order: {
            myRank: 2,
            averageRank: 2,
            consensusPosition: 2
          }
        }
      ],
      proposals: [],
      order: {
        locked: true,
        voteCount: 1,
        mySongIds: [7, 4]
      }
    });

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.order?.locked).toBe(true);
    expect(pageComponent.openAlbum()?.tracks[0].songId).toBe(7);
    expect(renderedRoot.textContent).toContain('Locked');

    const unlockOrderButton = Array.from(renderedRoot.querySelectorAll('.order-actions button'))
      .find((button) => button.textContent?.includes('Unlock order')) as HTMLButtonElement;

    expect(unlockOrderButton).toBeTruthy();
    expect(Array.from(renderedRoot.querySelectorAll('button'))
      .some((button) => button.textContent?.includes('Lock order'))).toBe(false);

    unlockOrderButton.click();

    const unlockRequest = httpTestingController.expectOne('/api/albums/2/order-lock');

    expect(unlockRequest.request.method).toBe('PUT');
    expect(unlockRequest.request.body).toEqual({ locked: false });

    unlockRequest.flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [
        {
          id: 5,
          songId: 7,
          songName: 'Dawn Chorus',
          version: 1,
          sortOrder: 1,
          addedAt: '2026-09-05T00:00:00Z',
          order: {
            myRank: 1,
            averageRank: 1,
            consensusPosition: 1
          }
        },
        {
          id: 3,
          songId: 4,
          songName: 'Night Shift',
          version: 1,
          sortOrder: 2,
          addedAt: '2026-09-05T00:00:00Z',
          order: {
            myRank: 2,
            averageRank: 2,
            consensusPosition: 2
          }
        }
      ],
      proposals: [],
      order: {
        locked: false,
        voteCount: 1,
        mySongIds: [7, 4]
      }
    });

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.order?.locked).toBe(false);
    expect(pageComponent.openAlbum()?.tracks.map((track) => track.songId)).toEqual([7, 4]);
    httpTestingController.verify();
  });

  it('records an album art vote and applies the winning cover', () => {
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
    httpTestingController.expectOne('/api/bands/9/drive/files').flush([
      {
        id: 'file-bottleneck',
        name: 'bottleneck.mp3',
        mimeType: 'audio/mpeg'
      }
    ]);

    pageComponent.showAlbumsPanel();
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([
      {
        id: 'cover-dawn',
        name: 'dawn.jpg',
        mimeType: 'image/jpeg'
      }
    ]);

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
      proposals: [],
      art: {
        locked: false,
        appliedDriveFileId: null,
        voteCount: 1,
        myDriveFileId: null,
        candidates: [
          {
            driveFileId: 'cover-dawn',
            voteCount: 1,
            isMine: false
          }
        ]
      }
    });

    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;
    const artChoiceButton = Array.from(renderedRoot.querySelectorAll('.art-contest .name-choices button'))
      .find((button) => button.textContent?.includes('dawn.jpg')) as HTMLButtonElement;

    expect(artChoiceButton).toBeTruthy();
    expect((artChoiceButton.querySelector('.art-thumb') as HTMLImageElement).getAttribute('src'))
      .toBe('/api/albums/2/art?fileId=cover-dawn');

    artChoiceButton.click();

    const voteRequest = httpTestingController.expectOne('/api/albums/2/art-votes');

    expect(voteRequest.request.method).toBe('PUT');
    expect(voteRequest.request.body).toEqual({ driveFileId: 'cover-dawn' });

    voteRequest.flush({
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
      }
    });

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.art?.myDriveFileId).toBe('cover-dawn');
    expect(renderedRoot.textContent).toContain('dawn.jpg 1');

    const applyButton = Array.from(renderedRoot.querySelectorAll('.art-actions button'))
      .find((button) => button.textContent?.includes('Apply art')) as HTMLButtonElement;

    expect(applyButton).toBeTruthy();
    applyButton.click();

    const lockRequest = httpTestingController.expectOne('/api/albums/2/art-lock');

    expect(lockRequest.request.method).toBe('PUT');
    expect(lockRequest.request.body).toEqual({ locked: true });

    lockRequest.flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [],
      proposals: [],
      art: {
        locked: true,
        appliedDriveFileId: 'cover-dawn',
        voteCount: 1,
        myDriveFileId: 'cover-dawn',
        candidates: [
          {
            driveFileId: 'cover-dawn',
            voteCount: 1,
            isMine: true
          }
        ]
      }
    });

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.art?.locked).toBe(true);
    expect(pageComponent.openAlbum()?.art?.appliedDriveFileId).toBe('cover-dawn');
    expect(renderedRoot.textContent).toContain('Locked');
    expect(renderedRoot.textContent).toContain('dawn.jpg');

    const coverImage = renderedRoot.querySelector('.art-preview') as HTMLImageElement;

    expect(coverImage).toBeTruthy();
    expect(coverImage.getAttribute('src')).toBe('/api/albums/2/art');
    expect((renderedRoot.querySelector('.art-contest .art-thumb') as HTMLImageElement).getAttribute('src'))
      .toBe('/api/albums/2/art?fileId=cover-dawn');

    const unlockArtButton = Array.from(renderedRoot.querySelectorAll('.art-actions button'))
      .find((button) => button.textContent?.includes('Unlock art')) as HTMLButtonElement;

    expect(unlockArtButton).toBeTruthy();
    expect(Array.from(renderedRoot.querySelectorAll('button'))
      .some((button) => button.textContent?.includes('Apply art'))).toBe(false);

    unlockArtButton.click();

    const unlockRequest = httpTestingController.expectOne('/api/albums/2/art-lock');

    expect(unlockRequest.request.method).toBe('PUT');
    expect(unlockRequest.request.body).toEqual({ locked: false });

    unlockRequest.flush({
      id: 2,
      bandId: 9,
      name: 'First Light',
      archived: false,
      approvalRule: 'all',
      tracks: [],
      proposals: [],
      art: {
        locked: false,
        appliedDriveFileId: 'cover-dawn',
        voteCount: 1,
        myDriveFileId: 'cover-dawn',
        candidates: [
          {
            driveFileId: 'cover-dawn',
            voteCount: 1,
            isMine: true
          }
        ]
      }
    });

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.art?.locked).toBe(false);
    expect(pageComponent.openAlbum()?.art?.appliedDriveFileId).toBe('cover-dawn');
    expect((renderedRoot.querySelector('.art-preview') as HTMLImageElement).getAttribute('src'))
      .toBe('/api/albums/2/art');

    httpTestingController.verify();
  });

  it('uploads a local cover as an art candidate and shows the size warning', () => {
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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
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
      proposals: [],
      art: {
        locked: false,
        appliedDriveFileId: null,
        voteCount: 0,
        myDriveFileId: null,
        candidates: []
      }
    });

    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;
    const fileInput = renderedRoot.querySelector('.art-contest input[type=file]') as HTMLInputElement;
    const coverFile = new File([
      new Uint8Array([
        1,
        2,
        3
      ])
    ], 'tiny.png', { type: 'image/png' });
    const chosenFiles = new DataTransfer();

    chosenFiles.items.add(coverFile);
    fileInput.files = chosenFiles.files;
    fileInput.dispatchEvent(new Event('change'));

    const uploadRequest = httpTestingController.expectOne('/api/albums/2/art');

    expect(uploadRequest.request.method).toBe('POST');
    expect(uploadRequest.request.body instanceof FormData).toBe(true);

    uploadRequest.flush({
      driveFileId: 'cover-tiny',
      name: 'tiny.png',
      mimeType: 'image/png',
      width: 400,
      height: 400,
      warning: 'Cover is 400x400. Recommended size is 1600-3000 pixels on each side.'
    });

    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([
      {
        id: 'cover-tiny',
        name: 'tiny.png',
        mimeType: 'image/png'
      }
    ]);

    componentFixture.detectChanges();

    expect(pageComponent.openAlbum()?.art?.appliedDriveFileId).toBeNull();
    expect(pageComponent.driveImageFiles().map((driveFile) => driveFile.id)).toEqual(['cover-tiny']);
    expect(renderedRoot.querySelector('.art-warning')?.textContent).toContain(
      'Cover is 400x400. Recommended size is 1600-3000 pixels on each side.'
    );

    expect(renderedRoot.querySelector('.art-preview')).toBeNull();
    expect((renderedRoot.querySelector('.art-contest .art-thumb') as HTMLImageElement).getAttribute('src'))
      .toBe('/api/albums/2/art?fileId=cover-tiny');

    httpTestingController.verify();
  });

  it('shows a Drive re-login banner when cover upload is forbidden', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    openAlbumDeskForArtUpload(componentFixture, pageComponent, httpTestingController);

    pageComponent.uploadAlbumArt(new File([
      new Uint8Array([
        1,
        2,
        3
      ])
    ], 'cover.png', { type: 'image/png' }));

    const uploadRequest = httpTestingController.expectOne('/api/albums/2/art');

    uploadRequest.flush(
      { error: 'Sign in with Google again so TrackLink can write album art to Drive.' },
      { status: 403, statusText: 'Forbidden' }
    );

    componentFixture.detectChanges();

    expect(pageComponent.artUploading()).toBe(false);
    expect(pageComponent.loadError()).toBe(
      'Sign in with Google again so TrackLink can write album art to Drive.'
    );

    expect((componentFixture.nativeElement as HTMLElement).querySelector('.album-banner')?.textContent)
      .toContain('Sign in with Google again');

    httpTestingController.verify();
  });

  it('shows the API-down banner when cover upload cannot reach the API', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    openAlbumDeskForArtUpload(componentFixture, pageComponent, httpTestingController);

    pageComponent.uploadAlbumArt(new File([
      new Uint8Array([
        1,
        2,
        3
      ])
    ], 'cover.png', { type: 'image/png' }));

    httpTestingController.expectOne('/api/albums/2/art').error(new ProgressEvent('error'));
    componentFixture.detectChanges();

    expect(pageComponent.artUploading()).toBe(false);
    expect(pageComponent.loadError()).toBe(
      'Could not reach the TrackLink API. Start the API on port 5180.'
    );

    expect((componentFixture.nativeElement as HTMLElement).querySelector('.album-banner')?.textContent)
      .toContain('Start the API on port 5180');

    httpTestingController.verify();
  });

  it('shows the API-down banner when the album desk cannot reach the API', () => {
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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').error(new ProgressEvent('error'));
    httpTestingController.expectOne('/api/bands/9/albums').error(new ProgressEvent('error'));
    componentFixture.detectChanges();

    expect(pageComponent.loadError()).toBe(
      'Could not reach the TrackLink API. Start the API on port 5180.'
    );

    expect((componentFixture.nativeElement as HTMLElement).querySelector('.album-banner')?.textContent)
      .toContain('Start the API on port 5180');

    httpTestingController.verify();
  });

  it('shows a cover-upload banner when the API returns a 500 HTML body', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    openAlbumDeskForArtUpload(componentFixture, pageComponent, httpTestingController);

    pageComponent.uploadAlbumArt(new File([
      new Uint8Array([
        1,
        2,
        3
      ])
    ], 'cover.png', { type: 'image/png' }));

    httpTestingController.expectOne('/api/albums/2/art').flush(
      '<html>Exception</html>',
      { status: 500, statusText: 'Server Error' }
    );

    componentFixture.detectChanges();

    expect(pageComponent.artUploading()).toBe(false);
    expect(pageComponent.loadError()).toBe('Could not upload that album cover.');
    expect((componentFixture.nativeElement as HTMLElement).querySelector('.album-banner')?.textContent)
      .toContain('Could not upload that album cover.');

    httpTestingController.verify();
  });

  it('shows the server error when a typed art file is not an image', () => {
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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
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
      proposals: [],
      art: {
        locked: false,
        appliedDriveFileId: null,
        voteCount: 0,
        myDriveFileId: null,
        candidates: []
      }
    });

    pageComponent.castArtVote('take-night');

    const voteRequest = httpTestingController.expectOne('/api/albums/2/art-votes');

    expect(voteRequest.request.body).toEqual({ driveFileId: 'take-night' });

    voteRequest.flush(
      { error: 'Cover art must be an image in the band Drive folder' },
      { status: 400, statusText: 'Bad Request' }
    );

    expect(pageComponent.loadError()).toBe('Cover art must be an image in the band Drive folder');
    httpTestingController.verify();
  });

  it('keeps images out of the take picker and lists them only for album art', () => {
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
    httpTestingController.expectOne('/api/bands/9/drive/files').flush([
      {
        id: 'file-bottleneck',
        name: 'bottleneck.mp3',
        mimeType: 'audio/mpeg'
      }
    ]);

    pageComponent.showDrivePanel();
    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;

    expect(pageComponent.driveFiles().map((driveFile) => driveFile.id)).toEqual(['file-bottleneck']);
    expect(renderedRoot.textContent).toContain('bottleneck.mp3');
    expect(renderedRoot.textContent).not.toContain('dawn.jpg');

    pageComponent.showAlbumsPanel();
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([
      {
        id: 'cover-dawn',
        name: 'dawn.jpg',
        mimeType: 'image/jpeg'
      }
    ]);

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
      proposals: [],
      art: {
        locked: false,
        appliedDriveFileId: null,
        voteCount: 0,
        myDriveFileId: null,
        candidates: []
      }
    });

    componentFixture.detectChanges();

    expect(pageComponent.driveFiles().map((driveFile) => driveFile.id)).toEqual(['file-bottleneck']);
    expect(pageComponent.driveImageFiles().map((driveFile) => driveFile.id)).toEqual(['cover-dawn']);

    const artOptionLabels = Array.from(renderedRoot.querySelectorAll('.art-contest select option'))
      .map((option) => option.textContent ?? '');

    expect(artOptionLabels.some((label) => label.includes('dawn.jpg'))).toBe(true);
    expect(artOptionLabels.some((label) => label.includes('bottleneck.mp3'))).toBe(false);
    expect((renderedRoot.querySelector('.art-contest .art-thumb') as HTMLImageElement).getAttribute('src'))
      .toBe('/api/albums/2/art?fileId=cover-dawn');

    pageComponent.showDrivePanel();
    componentFixture.detectChanges();

    expect(renderedRoot.textContent).toContain('bottleneck.mp3');
    expect(renderedRoot.textContent).not.toContain('dawn.jpg');
    httpTestingController.verify();
  });

  it('hides unlock controls from members who cannot manage albums', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    httpTestingController.expectOne('/api/auth/me').flush({
      configured: true,
      signedIn: true,
      connected: true,
      email: 'moe@example.com',
      member: {
        id: 6,
        userIdentifier: 'u',
        username: 'moe',
        fullname: 'Moe Vale',
        image: null,
        email: 'moe@example.com',
        bands: [
          {
            id: 9,
            name: 'Kindred',
            genre: 'indie',
            image: null,
            driveFolderId: 'folder-root',
            driveFolderName: 'Kindred',
            myRole: 'member',
            isOwner: false
          }
        ],
        roles: []
      }
    });

    httpTestingController.expectOne('/api/bands/9/songs').flush([]);
    httpTestingController.expectOne('/api/bands/9/drive/files').flush([]);

    pageComponent.showAlbumsPanel();
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
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
      proposals: [],
      order: {
        locked: true,
        voteCount: 1,
        mySongIds: null
      },
      art: {
        locked: true,
        appliedDriveFileId: 'cover-dawn',
        voteCount: 1,
        myDriveFileId: 'cover-dawn',
        candidates: [
          {
            driveFileId: 'cover-dawn',
            voteCount: 1,
            isMine: true
          }
        ]
      }
    });

    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;
    const buttonLabels = Array.from(renderedRoot.querySelectorAll('button'))
      .map((button) => button.textContent ?? '');

    expect(pageComponent.canManageAlbums()).toBe(false);
    expect(buttonLabels.some((label) => label.includes('Unlock order'))).toBe(false);
    expect(buttonLabels.some((label) => label.includes('Unlock art'))).toBe(false);
    expect((renderedRoot.querySelector('.art-preview') as HTMLImageElement).getAttribute('src'))
      .toBe('/api/albums/2/art');

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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
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
    httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
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

function openAlbumDeskForArtUpload(
  componentFixture: ComponentFixture<StudioPageComponent>,
  pageComponent: StudioPageComponent,
  httpTestingController: HttpTestingController
) {
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
  httpTestingController.expectOne('/api/bands/9/drive/files?kind=image').flush([]);
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
    proposals: [],
    art: {
      locked: false,
      appliedDriveFileId: null,
      voteCount: 0,
      myDriveFileId: null,
      candidates: []
    }
  });

  componentFixture.detectChanges();
}
