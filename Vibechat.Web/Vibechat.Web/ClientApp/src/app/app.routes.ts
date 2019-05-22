import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './registration/register.component';
import { ChatComponent } from './Chat/chat.component';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: '/chat'},
  { path: 'login', component: LoginComponent, data: { animation: 'LoginPage' } },
  { path: 'register', component: RegisterComponent, data: { animation: 'RegisterPage' } },
  { path: 'chat', component: ChatComponent, data: { animation: 'ChatPage' } }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutersModule {}
