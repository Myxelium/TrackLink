import {
  Injectable,
  PLATFORM_ID,
  inject,
  signal
} from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

const STORAGE_KEY = 'tracklink.memberId';

@Injectable({ providedIn: 'root' })
export class SessionService {
  private readonly platformId = inject(PLATFORM_ID);

  readonly memberId = signal<number | null>(null);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const storedMemberIdText = localStorage.getItem(STORAGE_KEY);
      const parsedMemberId = storedMemberIdText ? Number(storedMemberIdText) : NaN;

      this.memberId.set(Number.isFinite(parsedMemberId) ? parsedMemberId : null);
    }
  }

  setMemberId(memberIdValue: number | null) {
    this.memberId.set(memberIdValue);

    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    if (memberIdValue === null) {
      localStorage.removeItem(STORAGE_KEY);
      return;
    }

    localStorage.setItem(STORAGE_KEY, String(memberIdValue));
  }
}
