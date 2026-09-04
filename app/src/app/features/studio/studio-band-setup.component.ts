import {
  Component,
  input,
  output
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BandSummary, Invite } from '../../core/models';

@Component({
  selector: 'app-studio-band-setup',
  imports: [FormsModule],
  templateUrl: './studio-band-setup.component.html',
  styleUrl: './studio-page.component.scss'
})
export class StudioBandSetupComponent {
  readonly band = input.required<BandSummary>();
  readonly lastInvite = input<Invite | null>(null);
  readonly inviteEmail = input('');
  readonly inviteRole = input('uploader');
  readonly folderPicked = output<boolean>();
  readonly inviteRequested = output<boolean>();
  readonly inviteEmailChange = output<string>();
  readonly inviteRoleChange = output<string>();

  pickFolder() {
    this.folderPicked.emit(true);
  }

  sendInvite() {
    this.inviteRequested.emit(true);
  }
}
