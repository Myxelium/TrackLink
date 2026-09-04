import { Routes } from '@angular/router';
import { StudioPageComponent } from './features/studio/studio-page.component';
import { JoinPageComponent } from './features/invite/join-page.component';

export const clientApplicationRoutes: Routes = [
  {
    path: '',
    component: StudioPageComponent
  },
  {
    path: 'join',
    component: JoinPageComponent
  },
  {
    path: '**',
    redirectTo: ''
  }
];
