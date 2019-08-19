import {Inject, NgModule} from '@angular/core';
import {AppRoutersModule} from './app.routes';

import {AppComponent} from './app.component';
import {Api} from './Services/Api/api.service';
import {ChatModule} from './Modules/ChatModule';
import {LoginModule} from './Modules/LoginModule';
import {AuthService} from './Services/AuthService';
import {ServiceWorkerModule} from '@angular/service-worker';
import {environment} from '../environments/environment';
import {MaterialModule} from './material.module';
import {BrowserModule} from '@angular/platform-browser';
import { LOCALE_ID } from '@angular/core';

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
    ServiceWorkerModule.register('ngsw-worker.js',
      {
        enabled: environment.production
      })
  ],
  providers: [Api, AuthService],
  bootstrap: [AppComponent]
})
export class AppModule {}
