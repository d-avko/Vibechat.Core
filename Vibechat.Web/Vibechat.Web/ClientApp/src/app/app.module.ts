import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';
import { MaterialModule } from './material.module';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutersModule } from './app.routes';

import { AppComponent } from './app.component';
import { LoginComponent } from './login/login.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NoopAnimationsModule } from '@angular/platform-browser/animations'
import { RegisterComponent } from './registration/register.component';
import { ChatComponent } from './Chat/chat.component';
import { ApiRequestsBuilder } from './Requests/ApiRequestsBuilder';
import { ScrollDispatchModule } from '@angular/cdk/scrolling';
import { ConversationsFormatter } from './Formatters/ConversationsFormatter';
import { ConnectionManager } from './Connections/ConnectionManager';

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
    NoopAnimationsModule,
    ScrollDispatchModule
  ],
  providers: [ApiRequestsBuilder, ConversationsFormatter, ConnectionManager],
  bootstrap: [AppComponent]
})
export class AppModule {}
