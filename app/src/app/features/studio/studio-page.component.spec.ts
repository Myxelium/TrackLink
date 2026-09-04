import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { StudioPageComponent } from './studio-page.component';
import {
  Member,
  MemberSummary,
  Song
} from '../../core/models';

describe('StudioPageComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudioPageComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();
  });

  it('loads member information and band songs on the first page', () => {
    const componentFixture = TestBed.createComponent(StudioPageComponent);
    const httpTestingController = TestBed.inject(HttpTestingController);

    componentFixture.detectChanges();

    const memberSummaryList: MemberSummary[] = [{ id: 1, userIdentifier: 'u', username: 'ada', fullname: 'Ada Vale', image: null }];

    httpTestingController.expectOne('/api/members').flush(memberSummaryList);
    httpTestingController.expectOne('/api/auth/google/status').flush({
      configured: false,
      connected: false,
      email: null
    });

    const loadedMember: Member = {
      id: 1,
      userIdentifier: 'u',
      username: 'ada',
      fullname: 'Ada Vale',
      image: null,
      bands: [{ id: 9, name: 'Kindred', genre: 'indie', image: null }],
      roles: [{ id: 3, roleName: 'Producer', bandId: 9 }]
    };

    httpTestingController.expectOne('/api/members/1').flush(loadedMember);

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
    componentFixture.detectChanges();

    const renderedRoot = componentFixture.nativeElement as HTMLElement;

    expect(renderedRoot.textContent).toContain('Ada Vale');
    expect(renderedRoot.textContent).toContain('Kindred');
    expect(renderedRoot.textContent).toContain('Night Shift');
    expect(renderedRoot.textContent).toContain('Producer');
    httpTestingController.verify();
  });
});
