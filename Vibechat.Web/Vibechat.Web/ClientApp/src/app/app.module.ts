import {NgModule} from '@angular/core';
import { AppRoutersModule } from './app.routes';

import { AppComponent } from './app.component';
import { ApiRequestsBuilder } from './Requests/ApiRequestsBuilder';
import { ChatModule } from './Modules/ChatModule';
import { LoginModule } from './Modules/LoginModule';
import { AuthService } from './Auth/AuthService';
import { ServiceWorkerModule } from '@angular/service-worker';
import { environment } from '../environments/environment';
import { LoadingScreenService } from './Loading/LoadingScreenService';
import { MaterialModule } from './material.module';
import { BrowserModule } from '@angular/platform-browser';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    AppRoutersModule,
    BrowserModule,
    ChatModule,
    LoginModule,
    MaterialModule,
    ServiceWorkerModule.register('ngsw-worker.js', { enabled: environment.production })
  ],
  providers: [ApiRequestsBuilder, AuthService, LoadingScreenService],
  bootstrap: [AppComponent]
})
export class AppModule {}
