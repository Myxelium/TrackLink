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

  it('sends an unsigned invitee to Google login from a 401 body', () => {
    const componentFixture = TestBed.createComponent(JoinPageComponent);
    const pageComponent = componentFixture.componentInstance;
    const httpTestingController = TestBed.inject(HttpTestingController);
    const assignSpy = spyOn(pageComponent, 'openLoginUrl');

    pageComponent.email = 'kit@example.com';
    pageComponent.code = 'abc12';
    pageComponent.acceptInvite();

    const acceptRequest = httpTestingController.expectOne('/api/invites/accept');

    acceptRequest.flush({
      accepted: false,
      needsLogin: true,
      loginUrl: '/api/auth/google/login?invite=abc12',
      error: null,
      bandId: 9
    }, {
      status: 401,
      statusText: 'Unauthorized'
    });

    expect(assignSpy).toHaveBeenCalledWith('/api/auth/google/login?invite=abc12');
    expect(pageComponent.accepted()).toBeFalse();
    expect(pageComponent.notice()).toBeNull();
    httpTestingController.verify();
  });
});
