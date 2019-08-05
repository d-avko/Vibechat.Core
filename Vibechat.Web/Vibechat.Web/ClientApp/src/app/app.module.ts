import {NgModule} from '@angular/core';
import {AppRoutersModule} from './app.routes';

import {AppComponent} from './app.component';
import {ApiRequestsBuilder} from './Requests/ApiRequestsBuilder';
import {ChatModule} from './Modules/ChatModule';
import {LoginModule} from './Modules/LoginModule';
import {AuthService} from './Services/AuthService';
import {ServiceWorkerModule} from '@angular/service-worker';
import {environment} from '../environments/environment';
import {MaterialModule} from './material.module';
import {BrowserModule} from '@angular/platform-browser';

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
  providers: [ApiRequestsBuilder, AuthService],
  bootstrap: [AppComponent]
})
export class AppModule {}
