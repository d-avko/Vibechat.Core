import { Component, Inject, OnInit, AfterViewInit } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';
import { MatSnackBar } from '@angular/material';
import { ServerResponse } from '../ApiModels/ServerResponse';
import { LoginResponse } from '../ApiModels/LoginResponse';
import { LoginRequest } from '../ApiModels/LoginRequest';
import { Router } from '@angular/router';
import { ApiRequestsBuilder } from '../Requests/ApiRequestsBuilder';
import { AuthService } from "../Auth/AuthService";
import * as firebase from "firebase/app";

@Component({
  selector: 'login-view',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  constructor(private requestsBuilder: ApiRequestsBuilder, private snackbar: SnackBarHelper,private router: Router, private auth: AuthService) {
    this.phoneNumber = new FormControl('',
      Validators.compose(
        [
          Validators.maxLength(AuthService.PHONENUMBER_LENGTH),
          Validators.minLength(AuthService.PHONENUMBER_LENGTH - 1)]
      ));
    this.smsCode = new FormControl('', Validators.maxLength(AuthService.SMSCODE_LENGTH));
    this.phoneNumber.valueChanges.subscribe(() => this.OnNumberChanged());
  }

  public phoneNumber: FormControl;
  public smsCode: FormControl;

  public canLogIn: boolean = true;

  public isCodeSent: boolean = false;

  private recaptchaVerifier: firebase.auth.RecaptchaVerifier;

  get Recaptcha(): firebase.auth.RecaptchaVerifier {

    if (!this.recaptchaVerifier) {
      this.recaptchaVerifier = new firebase.auth.RecaptchaVerifier('sign-in-button', { 'size': 'invisible' });
    }

    return this.recaptchaVerifier;
  }
  set Recaptcha(value: firebase.auth.RecaptchaVerifier) {
    this.recaptchaVerifier = value;
  }

 
  public async SignIn() {
    this.canLogIn = false;

    if (!await this.auth.SignIn(this.smsCode.value)) {
      this.snackbar.openSnackBar("Wrong code, try again.");
    }

    this.canLogIn = true;
  }

  public async SendCode() {

    this.canLogIn = false;

    let result = await this.auth.SendSmsCode(this.phoneNumber.value, this.Recaptcha);

    if (result) {
      this.isCodeSent = true;
    } else {
      this.snackbar.openSnackBar("Couldn't send the message.");
    }

    this.canLogIn = true;
  }

  public OnNumberChanged() {
    this.isCodeSent = false;
  }

  public GotoRegisterPage() : void {
    this.router.navigateByUrl('/register');
  }


}

