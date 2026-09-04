import {
  Component,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  EMPTY,
  catchError,
  tap
} from 'rxjs';
import { TrackLinkApi } from '../../core/tracklink-api.service';

@Component({
  selector: 'app-join-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './join-page.component.html',
  styleUrl: '../studio/studio-page.component.scss'
})
export class JoinPageComponent implements OnInit {
  private readonly trackLinkApi = inject(TrackLinkApi);
  private readonly route = inject(ActivatedRoute);

  email = '';
  code = '';
  readonly notice = signal<string | null>(null);
  readonly accepted = signal(false);

  ngOnInit() {
    const query = this.route.snapshot.queryParamMap;

    this.code = query.get('code') ?? '';
    this.email = query.get('email') ?? '';

    if (this.code && this.email) {
      this.acceptInvite();
    }
  }

  googleLoginHref() {
    return this.code
      ? `/api/auth/google/login?invite=${encodeURIComponent(this.code)}`
      : '/api/auth/google/login';
  }

  acceptInvite() {
    this.notice.set(null);
    this.trackLinkApi.acceptInvite(this.email.trim(), this.code.trim()).pipe(
      tap((result) => {
        if (result.accepted) {
          this.accepted.set(true);
          this.notice.set('You are in the band. Open the studio.');
          return;
        }

        if (result.needsLogin && result.loginUrl) {
          window.location.assign(result.loginUrl);
          return;
        }

        this.notice.set(result.error || 'Could not accept that invite.');
      }),
      catchError(() => {
        this.notice.set('Could not accept that invite.');
        return EMPTY;
      })
    )
      .subscribe();
  }
}
