import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideServerRendering } from '@angular/platform-server';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';

export const config: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideServerRendering(),
    provideRouter(routes),
    provideHttpClient()
  ]
};
