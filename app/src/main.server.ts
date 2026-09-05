import { BootstrapContext, bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { mergedServerConfig } from './app/app.config.server';

const bootstrapTheApplication = (bootstrapContext: BootstrapContext) =>
  bootstrapApplication(AppComponent, mergedServerConfig, bootstrapContext);

export default bootstrapTheApplication;
