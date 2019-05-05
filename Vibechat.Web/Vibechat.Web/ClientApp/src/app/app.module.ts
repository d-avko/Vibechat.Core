import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';
import { MaterialModule } from './material.module';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutersModule } from './app.routes';

import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'
import { RegisterComponent } from './registration/register.component';
import { ChatComponent } from './Chat/chat.component';
import { ApiRequestsBuilder } from './Requests/ApiRequestsBuilder';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    ChatComponent
  ],
  imports: [
    BrowserModule,
    MaterialModule,
    AppRoutersModule,
    HttpClientModule,
    ReactiveFormsModule,
    BrowserAnimationsModule
  ],
  providers: [ ApiRequestsBuilder ],
  bootstrap: [AppComponent]
})
export class AppModule {}
