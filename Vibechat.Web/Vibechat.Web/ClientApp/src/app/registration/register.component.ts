import { Component } from '@angular/core';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { MatSnackBar } from '@angular/material';
import { RegisterRequest } from '../ApiModels/RegisterRequest';
import { ServerResponse } from '../ApiModels/ServerResponse';
import { LoginComponent } from '../login/login.component';
import { Router } from '@angular/router';
import { ApiRequestsBuilder } from '../Requests/ApiRequestsBuilder';
import { AuthService } from '../Auth/AuthService';

@Component({
  selector: 'register-view',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent extends LoginComponent {

  public registerGroup: FormGroup

  public canRegister: boolean = true;

  public canLogOut: boolean = false;

  constructor(requestsBuilder: ApiRequestsBuilder, snackbar: MatSnackBar, router: Router, auth: AuthService) {

    super(requestsBuilder, snackbar, router, auth);

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

    this.requestsBuilder.RegisterRequest(credentials).subscribe(result => this.OnRegisterResultReceived(result));
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

