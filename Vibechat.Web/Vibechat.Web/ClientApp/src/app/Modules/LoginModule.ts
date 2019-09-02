import {NgModule} from "@angular/core";
import {MaterialModule} from "../material.module";
import {CommonModule} from "@angular/common";
import {ReactiveFormsModule} from "@angular/forms";
import {BrowserModule} from "@angular/platform-browser";
import {HttpClientModule} from "@angular/common/http";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {AuthService} from "../Services/AuthService";
import {TranslationModule} from "./translation/translation.module";
import {AppRoutesModule} from "../app.routes";
import {LoginComponent} from "../UiComponents/Login/login.component";
import {ChangeUserInfoComponent} from "../UiComponents/Registration/register.component";

@NgModule({
  declarations: [
    LoginComponent,
    ChangeUserInfoComponent
  ],
  imports: [
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
    BrowserModule,
    HttpClientModule,
    BrowserAnimationsModule,
    TranslationModule,
    AppRoutesModule
  ],
  entryComponents:[],
  providers: [AuthService]
})
export class LoginModule { }
