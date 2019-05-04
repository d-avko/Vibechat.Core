import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormControl, Validators } from '@angular/forms';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';
import { MatSnackBar } from '@angular/material';
import { ServerResponse } from '../ApiModels/ServerResponse';
import { LoginResponse } from '../ApiModels/LoginResponse';
import { LoginRequest } from '../ApiModels/LoginRequest';
import { Router } from '@angular/router';

@Component({
  selector: 'login-view',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {

  private usernameOrEmail: FormControl;
  private password: FormControl;

  protected httpClient: HttpClient;
  protected baseUrl: string;
  protected snackbar: SnackBarHelper;
  protected router: Router;

  public canLogIn: boolean = true;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, snackbar: MatSnackBar, router: Router ) {
    this.httpClient = http;
    this.baseUrl = baseUrl;
    this.usernameOrEmail = new FormControl('', Validators.required);
    this.password = new FormControl('', Validators.required);
    this.snackbar = new SnackBarHelper(snackbar);
    this.router = router;
  }

  public Login(): void {
    this.canLogIn = false;

    let credentials = new LoginRequest({ UserNameOrEmail: this.usernameOrEmail.value, Password: this.password.value })

    this.httpClient.post<ServerResponse<LoginResponse>>(this.baseUrl + "api/login", credentials)
      .subscribe(result => this.OnLoginResultReceived(result));
  }

  public GotoRegisterPage() : void {
    this.router.navigateByUrl('/register');
  }

  private OnLoginResultReceived(result: ServerResponse<LoginResponse>) : void {
    if (!result.isSuccessfull) {
      this.snackbar.openSnackBar(result.errorMessage); 
    }
    this.canLogIn = true;
  }

}

