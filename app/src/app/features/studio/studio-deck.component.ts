import {
  Component,
  ElementRef,
  effect,
  input,
  viewChild
} from '@angular/core';

@Component({
  selector: 'app-studio-deck',
  templateUrl: './studio-deck.component.html',
  styleUrl: './studio-page.component.scss'
})
export class StudioDeckComponent {
  readonly playingId = input<number | null>(null);
  readonly armedTakeLabel = input('No take armed');
  readonly driveStatusText = input('');
  readonly googleLoginHref = input('/api/auth/google/login');
  readonly showConnect = input(false);
  readonly seekMs = input<number | null>(null);
  readonly seekEpoch = input(0);
  private readonly player = viewChild<ElementRef<HTMLAudioElement>>('player');

  constructor() {
    effect(() => {
      const ms = this.seekMs();
      const epoch = this.seekEpoch();
      const audio = this.player()?.nativeElement;

      if (ms === null || epoch === 0 || !audio) {
        return;
      }

      const apply = () => {
        audio.currentTime = ms / 1000;
      };

      if (audio.readyState >= 1) {
        apply();
        return;
      }

      audio.addEventListener('loadedmetadata', apply, { once: true });
    });
  }

  audioUrl(songId: number) {
    return `/api/songs/${songId}/audio`;
  }

  currentTimeMs() {
    const seconds = this.player()?.nativeElement.currentTime;

    if (seconds === undefined || Number.isNaN(seconds)) {
      return 0;
    }

    return Math.round(seconds * 1000);
  }
}
