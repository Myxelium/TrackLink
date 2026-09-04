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
      const raw = localStorage.getItem(STORAGE_KEY);
      const parsed = raw ? Number(raw) : NaN;

      this.memberId.set(Number.isFinite(parsed) ? parsed : null);
    }
  }

  setMemberId(id: number | null) {
    this.memberId.set(id);

    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    if (id === null) {
      localStorage.removeItem(STORAGE_KEY);
      return;
    }

    localStorage.setItem(STORAGE_KEY, String(id));
  }
}
