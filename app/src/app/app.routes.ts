import { Routes } from '@angular/router';
import { StudioPageComponent } from './features/studio/studio-page.component';

export const clientApplicationRoutes: Routes = [
  {
    path: '',
    component: StudioPageComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
