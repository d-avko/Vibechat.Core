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

  public phoneNumber: FormControl;
  public smsCode: FormControl;
  protected snackbar: SnackBarHelper;
  protected router: Router;
  protected requestsBuilder: ApiRequestsBuilder;
  private confirmation: firebase.auth.ConfirmationResult;

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

  constructor(requestsBuilder: ApiRequestsBuilder, snackbar: MatSnackBar, router: Router, private auth: AuthService ) {
    this.phoneNumber = new FormControl('', Validators.required);
    this.smsCode = new FormControl('');
    this.snackbar = new SnackBarHelper(snackbar);
    this.router = router;
    this.requestsBuilder = requestsBuilder;
    this.phoneNumber.valueChanges.subscribe(() => this.OnNumberChanged());
  }

 
  public SignIn(): void {

    var phoneNumber: string = this.phoneNumber.value;

    if (!phoneNumber.startsWith("+")) {
      phoneNumber = "+" + phoneNumber;
    }

    this.confirmation.confirm(this.smsCode.value).then(async (result) => {
      //signed in, make api call
      let token = await result.user.getIdToken();

      let credentials = new LoginRequest({ UidToken: token, PhoneNumber: phoneNumber });

      let identity = await this.requestsBuilder.LoginRequest(credentials);

      this.OnLoginResultReceived(identity);
    }).catch(() => {
      this.snackbar.openSnackBar("Wrong code, try again.");
    })

  }

  public SendCode() {

    this.canLogIn = false;

    var phoneNumber: string = this.phoneNumber.value;

    if (!phoneNumber.startsWith("+")) {
      phoneNumber = "+" + phoneNumber;
    }

    firebase.auth().signInWithPhoneNumber(phoneNumber, this.Recaptcha)
    .then((confirmationResult) => {
      // SMS sent. 
      this.confirmation = confirmationResult;
      this.isCodeSent = true;
      this.canLogIn = true;
    }).catch((err) => {
      this.snackbar.openSnackBar("Couldn't send the message.");
      this.canLogIn = true;
    });
  }

  public OnNumberChanged() {
    this.isCodeSent = false;
  }

  public GotoRegisterPage() : void {
    this.router.navigateByUrl('/register');
  }

  private OnLoginResultReceived(result: ServerResponse<LoginResponse>): void {

    if (!result.isSuccessfull) {
      this.canLogIn = true;
      return;
    }

    this.canLogIn = true;

    this.auth.OnUserLoggedIn(result.response);

    if (result.response.isNewUser) {
      this.router.navigateByUrl('/register');
    } else {
      this.router.navigateByUrl('/chat');
    }
  }

}

