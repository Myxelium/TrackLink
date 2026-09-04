import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { JoinPageComponent } from './join-page.component';

describe('JoinPageComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JoinPageComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([{ path: 'join', component: JoinPageComponent }])
      ]
    }).compileComponents();
  });

  it('accepts an invite with email and code', () => {
    const componentFixture = TestBed.createComponent(JoinPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);

    pageComponent.email = 'kit@example.com';
    pageComponent.code = 'abc12';
    pageComponent.acceptInvite();

    const acceptRequest = httpTestingController.expectOne('/api/invites/accept');

    expect(acceptRequest.request.body).toEqual({
      email: 'kit@example.com',
      code: 'abc12'
    });

    acceptRequest.flush({
      accepted: true,
      needsLogin: false,
      loginUrl: null,
      error: null,
      bandId: 9
    });

    expect(pageComponent.accepted()).toBeTrue();
    httpTestingController.verify();
  });
});
