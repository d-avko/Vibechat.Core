import {NgModule} from '@angular/core';
import { AppRoutersModule } from './app.routes';

import { AppComponent } from './app.component';
import { ApiRequestsBuilder } from './Requests/ApiRequestsBuilder';
import { ChatModule } from './Modules/ChatModule';
import { LoginModule } from './Modules/LoginModule';
import { AuthService } from './Auth/AuthService';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    AppRoutersModule,
    ChatModule,
    LoginModule
  ],
  providers: [ApiRequestsBuilder, AuthService],
  bootstrap: [AppComponent]
})
export class AppModule {}
