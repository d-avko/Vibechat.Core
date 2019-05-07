import { Component, Inject } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';
import { MatSnackBar } from '@angular/material';
import { ServerResponse } from '../ApiModels/ServerResponse';
import { LoginResponse } from '../ApiModels/LoginResponse';
import { LoginRequest } from '../ApiModels/LoginRequest';
import { Router } from '@angular/router';
import { Cache } from '../Auth/Cache';
import { ApiRequestsBuilder } from '../Requests/ApiRequestsBuilder';

@Component({
  selector: 'login-view',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  private usernameOrEmail: FormControl;
  private password: FormControl;

  protected snackbar: SnackBarHelper;
  protected router: Router;
  protected requestsBuilder: ApiRequestsBuilder;

  public canLogIn: boolean = true;

  constructor(requestsBuilder: ApiRequestsBuilder, snackbar: MatSnackBar, router: Router ) {
    this.usernameOrEmail = new FormControl('', Validators.required);
    this.password = new FormControl('', Validators.required);
    this.snackbar = new SnackBarHelper(snackbar);
    this.router = router;
    this.requestsBuilder = requestsBuilder;
  }

  public Login(): void {
    this.canLogIn = false;

    let credentials = new LoginRequest({ UserNameOrEmail: this.usernameOrEmail.value, Password: this.password.value })

    this.requestsBuilder.LoginRequest(credentials).subscribe(result => this.OnLoginResultReceived(result));
  }

  public GotoRegisterPage() : void {
    this.router.navigateByUrl('/register');
  }

  private OnLoginResultReceived(result: ServerResponse<LoginResponse>): void {

    if (!result.isSuccessfull) {
      this.snackbar.openSnackBar(result.errorMessage);
      return;
    }

    this.canLogIn = true;

    Cache.OnUserLoggedIn(result.response);

    this.router.navigateByUrl('/chat');
  }

}

