import { Injectable, signal } from '@angular/core';
import { Member } from './models';

@Injectable({ providedIn: 'root' })
export class SessionService {
  readonly currentMember = signal<Member | null>(null);

  setMember(signedInMember: Member | null) {
    this.currentMember.set(signedInMember);
  }
}
