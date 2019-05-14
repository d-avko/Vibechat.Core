import { NgModule } from "@angular/core";
import { LoginComponent } from "../login/login.component";
import { RegisterComponent } from "../registration/register.component";
import { MaterialModule } from "../material.module";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { AppRoutersModule } from "../app.routes";
import { HttpClientModule } from "@angular/common/http";
import { NoopAnimationsModule, BrowserAnimationsModule } from "@angular/platform-browser/animations";

@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent
  ],
  imports: [
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
    BrowserModule,
    AppRoutersModule,
    HttpClientModule,
    BrowserAnimationsModule],
  providers: []
})
export class LoginModule { }
