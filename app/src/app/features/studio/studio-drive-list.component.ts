import {
  Component,
  input,
  output
} from '@angular/core';
import { DriveFile } from '../../core/models';

@Component({
  selector: 'app-studio-drive-list',
  templateUrl: './studio-drive-list.component.html',
  styleUrl: './studio-page.component.scss'
})
export class StudioDriveListComponent {
  readonly driveFiles = input<DriveFile[]>([]);
  readonly fileChosen = output<DriveFile>();

  chooseDriveFile(driveFile: DriveFile) {
    this.fileChosen.emit(driveFile);
  }
}
