import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import {
  provideHttpClient,
  withFetch,
  withInterceptors
} from '@angular/common/http';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import { clientApplicationRoutes } from './app.routes';
import { credentialsInterceptor } from './core/credentials.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(clientApplicationRoutes),
    provideHttpClient(withFetch(), withInterceptors([credentialsInterceptor])),
    provideClientHydration(withEventReplay())
  ]
};
