import { Component, input } from '@angular/core';
import { Member } from '../../core/models';

@Component({
  selector: 'app-studio-session-facts',
  templateUrl: './studio-session-facts.component.html',
  styleUrl: './studio-page.component.scss'
})
export class StudioSessionFactsComponent {
  readonly currentMember = input.required<Member>();
}
