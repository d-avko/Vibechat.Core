import { Component } from '@angular/core';
import { FormControl, Validators, FormGroup } from '@angular/forms';
import { MatSnackBar } from '@angular/material';
import { ChangeUserInfoRequest } from '../ApiModels/RegisterRequest';
import { ServerResponse } from '../ApiModels/ServerResponse';
import { LoginComponent } from '../login/login.component';
import { Router } from '@angular/router';
import { ApiRequestsBuilder } from '../Requests/ApiRequestsBuilder';
import { AuthService } from '../Auth/AuthService';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';

@Component({
  selector: 'register-view',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class ChangeUserInfoComponent {

  public registerGroup: FormGroup

  public canRegister: boolean = true;

  public static maxNameLength: number = 120;

  constructor(private requestsBuilder: ApiRequestsBuilder, private snackbar: SnackBarHelper, private router: Router) {

    this.registerGroup = new FormGroup(
      {
        username: new FormControl('', Validators.required),
        firstName: new FormControl(''),
        lastName: new FormControl('')
      }
    )
  }

  public async Register() {

    let credentials = new ChangeUserInfoRequest(
      {
        UserName: this.registerGroup.get('username').value,
        FirstName: this.registerGroup.get('firstName').value,
        LastName: this.registerGroup.get('lastName').value
      });

    if (credentials.FirstName.length > ChangeUserInfoComponent.maxNameLength
      || credentials.LastName.length > ChangeUserInfoComponent.maxNameLength
      || credentials.UserName.length > ChangeUserInfoComponent.maxNameLength ) {
      this.snackbar.openSnackBar("Either username or first name or last name was too long.");
      return;
    }

    this.canRegister = false;

    let response = await this.requestsBuilder.ChangeUserInfo(credentials);

    if (response.isSuccessfull) {
      this.router.navigateByUrl('/chat');
    } else {
      this.snackbar.openSnackBar(response.errorMessage, 2);
    }

    this.canRegister = true;
  }

  public GotoLoginScreen() {
    this.router.navigateByUrl('/login');
  }


}

