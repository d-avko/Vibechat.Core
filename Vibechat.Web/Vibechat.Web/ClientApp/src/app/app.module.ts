import {NgModule} from '@angular/core';
import { AppRoutersModule } from './app.routes';

import { AppComponent } from './app.component';
import { ApiRequestsBuilder } from './Requests/ApiRequestsBuilder';
import { ChatModule } from './Modules/ChatModule';
import { LoginModule } from './Modules/LoginModule';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    AppRoutersModule,
    ChatModule,
    LoginModule
  ],
  providers: [ApiRequestsBuilder],
  bootstrap: [AppComponent]
})
export class AppModule {}
