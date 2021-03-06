import {Component} from '@angular/core';
import {FormControl, FormGroup, Validators} from '@angular/forms';
import {Router} from '@angular/router';
import {Meta} from "@angular/platform-browser";
import {Api} from "../../Services/Api/api.service";
import {MessageReportingService} from "../../Services/MessageReportingService";
import {ChangeUserInfoRequest} from "../../ApiModels/RegisterRequest";

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

  constructor(private requestsBuilder: Api,
              private messagesService: MessageReportingService,
              private router: Router,
              public meta: Meta) {

    this.registerGroup = new FormGroup(
      {
        username: new FormControl('',Validators.compose([
          Validators.minLength(5), Validators.maxLength(120),
          Validators.required
        ])),
        firstName: new FormControl(''),
        lastName: new FormControl('')
      }
    );
    meta.addTags([
      {name: 'description', content: 'Telegram-like messenger created with ASP.NET Core and Angular 8.'},
      {name: 'viewport', content: 'width=device-width, initial-scale=1'},
      {name: 'keywords', content: 'ASP.NET Core,TypeScript, Angular'},
      {httpEquiv: 'Content-Type', content: 'text/html'},
      {charset: 'UTF-8'}
    ], true);
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
      this.messagesService.WrongUserDataLength();
      return;
    }

    this.canRegister = false;

    let response = await this.requestsBuilder.ChangeUserInfo(credentials);

    if (response.isSuccessfull) {
      this.router.navigateByUrl('/chat');
    }

    this.canRegister = true;
  }

  public GotoLoginScreen() {
    this.router.navigateByUrl('/login');
  }


}

