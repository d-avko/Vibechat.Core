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
        phoneNumber: new FormControl(''),
        firstName: new FormControl(''),
        lastName: new FormControl('')//,
       // email: new FormControl('')
      }
    )
  }

  public async Register() {
    let phoneNumber: string = this.registerGroup.get('phoneNumber').value;

    if (!phoneNumber.startsWith("+")) {
      phoneNumber = "+" + phoneNumber;
    }

    let credentials = new RegisterRequest(
      {
        UserName: this.registerGroup.get('username').value,
        PhoneNumber: this.registerGroup.get('phoneNumber').value,
        FirstName: this.registerGroup.get('firstName').value,
        LastName: this.registerGroup.get('lastName').value//,
        //Email: this.registerGroup.get('email').value
      });

    this.canRegister = false;

    let response = await this.requestsBuilder.RegisterRequest(credentials);

    this.OnRegisterResultReceived(response);
  }

  public GotoLoginScreen() {
    this.router.navigateByUrl('/login');
  }

  private OnRegisterResultReceived(result: ServerResponse<string>) {
    if (result.isSuccessfull) {
      this.snackbar.openSnackBar("Successfully registered.");
    }

    this.canRegister = true;
  }

}

