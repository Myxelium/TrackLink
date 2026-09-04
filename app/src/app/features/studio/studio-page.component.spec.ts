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
    const fixture = TestBed.createComponent(StudioPageComponent);
    const http = TestBed.inject(HttpTestingController);

    fixture.detectChanges();

    const members: MemberSummary[] = [{ id: 1, userIdentifier: 'u', username: 'ada', fullname: 'Ada Vale', image: null }];

    http.expectOne('/api/members').flush(members);
    http.expectOne('/api/auth/google/status').flush({
      configured: false,
      connected: false,
      email: null
    });

    const member: Member = {
      id: 1,
      userIdentifier: 'u',
      username: 'ada',
      fullname: 'Ada Vale',
      image: null,
      bands: [{ id: 9, name: 'Kindred', genre: 'indie', image: null }],
      roles: [{ id: 3, roleName: 'Producer', bandId: 9 }]
    };

    http.expectOne('/api/members/1').flush(member);

    const songs: Song[] = [
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

    http.expectOne('/api/bands/9/songs').flush(songs);
    fixture.detectChanges();

    const el = fixture.nativeElement as HTMLElement;

    expect(el.textContent).toContain('Ada Vale');
    expect(el.textContent).toContain('Kindred');
    expect(el.textContent).toContain('Night Shift');
    expect(el.textContent).toContain('Producer');
    http.verify();
  });
});
