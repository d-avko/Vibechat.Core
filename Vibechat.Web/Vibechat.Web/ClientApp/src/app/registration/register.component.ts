import {Component} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';
import {ChangeUserInfoRequest} from '../ApiModels/RegisterRequest';
import {Router} from '@angular/router';
import {ApiRequestsBuilder} from '../Requests/ApiRequestsBuilder';
import {SnackBarHelper} from '../Snackbar/SnackbarHelper';
import {MessageReportingService} from "../Services/MessageReportingService";

@Component({
  selector: 'register-view',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class ChangeUserInfoComponent {

  public registerGroup: FormGroup;

  public canRegister: boolean = true;

  public static maxNameLength: number = 120;

  public static minUsernameLength: number = 5;

  constructor(private requestsBuilder: ApiRequestsBuilder, private messagesService: MessageReportingService, private router: Router) {

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
      || credentials.UserName.length > ChangeUserInfoComponent.maxNameLength
      || credentials.UserName.length < ChangeUserInfoComponent.minUsernameLength) {
      this.messagesService.DisplayMessage("Either username or first name or last name was too long / too short.");
      return;
    }

    this.canRegister = false;

    let response = await this.requestsBuilder.ChangeUserInfo(credentials);

    if (response.isSuccessfull) {
      this.router.navigateByUrl('/chat');
    } else {
      this.messagesService.DisplayMessage(response.errorMessage);
    }

    this.canRegister = true;
  }

  public GotoLoginScreen() {
    this.router.navigateByUrl('/login');
  }


}

