import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {ChatComponent} from './UiComponents/Chat/chat.component';
import {LoginComponent} from "./UiComponents/Login/login.component";
import {ChangeUserInfoComponent} from "./UiComponents/Registration/register.component";

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: '/chat'},
  { path: 'login', component: LoginComponent, data: { animation: 'LoginPage' } },
  { path: 'register', component: ChangeUserInfoComponent, data: { animation: 'RegisterPage' } },
  { path: 'chat', component: ChatComponent, data: { animation: 'ChatPage' } },
  { path: '**', redirectTo: '/login',}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutesModule {}
