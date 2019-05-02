import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { MatSnackBar } from '@angular/material';
import { RegisterRequest } from '../ApiModels/RegisterRequest';
import { ServerResponse } from '../ApiModels/ServerResponse';
import { LoginComponent } from '../login/login.component';
import { Router } from '@angular/router';

@Component({
  selector: 'register-view',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent extends LoginComponent {

  private registerGroup: FormGroup

  public canRegister: boolean = true;

  public canLogOut: boolean = false;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string, snackbar: MatSnackBar, router: Router) {

    super(http, baseUrl, snackbar, router);

    this.registerGroup = new FormGroup(
      {
        username: new FormControl('', Validators.required),
        email: new FormControl(''),
        password: new FormControl('', Validators.required),
        confirmPassword: new FormControl('', Validators.required),
        firstName: new FormControl(''),
        lastName: new FormControl('')
      }
    )
  }

  public Register(): void {
    let password = this.registerGroup.get('password').value;
    let confirmedPassword = this.registerGroup.get('confirmPassword').value;

    if (password != confirmedPassword) {
      this.snackbar.openSnackBar("Passwords must match!");
      return;
    }

    let credentials = new RegisterRequest(
      {
        UserName: this.registerGroup.get('username').value,
        Password: password,
        Email: this.registerGroup.get('email').value,
        FirstName: this.registerGroup.get('firstName').value,
        LastName: this.registerGroup.get('lastName').value,
      });

    this.canRegister = false;

    this.httpClient.post<ServerResponse<string>>(this.baseUrl + "api/register", credentials)
      .subscribe(result => this.OnRegisterResultReceived(result));
  }

  public GotoLoginScreen() {
    this.router.navigateByUrl('/login');
  }

  private OnRegisterResultReceived(result: ServerResponse<string>) {
    if (!result.isSuccessfull) {
      this.snackbar.openSnackBar(result.errorMessage);
    }
    else {
      this.snackbar.openSnackBar("Successfully registered.");
    }

    this.canRegister = true;
  }

}

