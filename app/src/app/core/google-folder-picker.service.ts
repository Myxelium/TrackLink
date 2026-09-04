import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TrackLinkApi } from './tracklink-api.service';

export interface PickedFolder {
  id: string;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class GoogleFolderPicker {
  private readonly trackLinkApi = inject(TrackLinkApi);
  private pickerScript: Promise<void> | null = null;

  async pickFolder(): Promise<PickedFolder | null> {
    const token = await firstValueFrom(this.trackLinkApi.pickerToken());

    await this.loadPicker();

    return new Promise((resolve) => {
      const pickerApi = readPickerApi();

      if (!pickerApi) {
        resolve(null);
        return;
      }

      const folderView = new pickerApi.DocsView(pickerApi.ViewId.FOLDERS);

      folderView.setSelectFolderEnabled(true);
      folderView.setMimeTypes('application/vnd.google-apps.folder');
      folderView.setIncludeFolders(true);

      const builder = new pickerApi.PickerBuilder();

      builder.addView(folderView);
      builder.setOAuthToken(token.accessToken);
      builder.setTitle('Band Drive folder');
      builder.setCallback((data: PickerCallbackData) => {
        if (data.action === pickerApi.Action.CANCEL) {
          resolve(null);
          return;
        }

        if (data.action !== pickerApi.Action.PICKED) {
          return;
        }

        const document = data.docs?.[0];

        resolve(document?.id ? { id: document.id, name: document.name || 'Folder' } : null);
      });

      if (token.apiKey) {
        builder.setDeveloperKey(token.apiKey);
      }

      builder.build().setVisible(true);
    });
  }

  private loadPicker() {
    if (this.pickerScript) {
      return this.pickerScript;
    }

    this.pickerScript = new Promise((finishLoad, failLoad) => {
      if (document.querySelector('script[data-google-picker]')) {
        finishLoad();
        return;
      }

      const scriptElement = document.createElement('script');

      scriptElement.src = 'https://apis.google.com/js/api.js';
      scriptElement.dataset['googlePicker'] = '1';

      scriptElement.onload = () => {
        const googleClient = (window as Window & { gapi?: { load: (name: string, afterLoad: () => void) => void } }).gapi;

        if (!googleClient) {
          failLoad(new Error('Google API failed to load'));
          return;
        }

        googleClient.load('picker', () => finishLoad());
      };

      scriptElement.onerror = () => failLoad(new Error('Google Picker script failed to load'));
      document.head.appendChild(scriptElement);
    });

    return this.pickerScript;
  }
}

interface PickerCallbackData {
  action: string;
  docs?: { id?: string; name?: string }[];
}

interface PickerSurface {
  Action: { PICKED: string; CANCEL: string };
  ViewId: { FOLDERS: string };
  DocsView: new (viewId: string) => {
    setSelectFolderEnabled: (enabled: boolean) => void;
    setMimeTypes: (types: string) => void;
    setIncludeFolders: (include: boolean) => void;
  };
  PickerBuilder: new () => {
    addView: (view: unknown) => void;
    setOAuthToken: (token: string) => void;
    setTitle: (title: string) => void;
    setCallback: (afterPick: (data: PickerCallbackData) => void) => void;
    setDeveloperKey: (key: string) => void;
    build: () => { setVisible: (visible: boolean) => void };
  };
}

function readPickerApi(): PickerSurface | null {
  const googleWindow = window as Window & { google?: { picker?: PickerSurface } };

  return googleWindow.google?.picker ?? null;
}
