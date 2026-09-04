import {
  Component,
  input,
  output
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AlbumSummary } from '../../core/models';

@Component({
  selector: 'app-studio-album-desk',
  imports: [FormsModule],
  templateUrl: './studio-album-desk.component.html',
  styleUrl: './studio-page.component.scss'
})
export class StudioAlbumDeskComponent {
  readonly albums = input<AlbumSummary[]>([]);
  readonly selectedAlbumId = input<number | null>(null);
  readonly canCreate = input(false);
  readonly newAlbumName = input('');
  readonly newAlbumRule = input('all');
  readonly albumChosen = output<AlbumSummary>();
  readonly albumRequested = output<boolean>();
  readonly newAlbumNameChange = output<string>();
  readonly newAlbumRuleChange = output<string>();
  creating = false;

  chooseAlbum(album: AlbumSummary) {
    this.albumChosen.emit(album);
  }

  createAlbum() {
    this.creating = false;
    this.albumRequested.emit(true);
  }
}
