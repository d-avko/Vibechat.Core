import {LoginResponse} from "../ApiModels/LoginResponse";
import {AppUser} from "../Data/AppUser";
import {Router} from "@angular/router";
import {ServerResponse} from "../ApiModels/ServerResponse";
import {ApiRequestsBuilder} from "../Requests/ApiRequestsBuilder";
import {Injectable} from '@angular/core';
import * as firebase from "firebase/app";
import "firebase/auth";
import {LoginRequest} from "../ApiModels/LoginRequest";
import * as jwtDecode from "jwt-decode";

@Injectable({
  providedIn: 'root'
})
export class AuthService  {
  constructor(public router: Router, public requestsBuilder: ApiRequestsBuilder) {
    var firebaseConfig = {
      apiKey: "AIzaSyDqVFEN02Mvb2UU4bcrCDmTtDmV7RMla8E",
      authDomain: "vibechat-ng.firebaseapp.com",
      databaseURL: "https://vibechat-ng.firebaseio.com",
      projectId: "vibechat-ng",
      storageBucket: "vibechat-ng.appspot.com",
      messagingSenderId: "105631356118",
      appId: "1:105631356118:web:7006855db55d021e"
    };
    // Initialize Firebase
    firebase.initializeApp(firebaseConfig);
  }

  public User: AppUser;

  public Contacts: Array<AppUser>;

  public token: string;

  public static SMSCODE_LENGTH = 6;

  public static PHONENUMBER_LENGTH = 13;

  public IsAuthenticated: boolean;

  private confirmation: firebase.auth.ConfirmationResult;

  private phoneNumberToConfirm: string;

  private MaxMillisecondsForTokenToExpire: number = 1000 * 60 * 2;

  public async SendSmsCode(phoneNumber: string, recaptcha: firebase.auth.RecaptchaVerifier) : Promise<boolean> {
    phoneNumber = this.NormalizeMobilePhone(phoneNumber);

    try {
      this.confirmation = await firebase.auth().signInWithPhoneNumber(phoneNumber, recaptcha);
      return true;
    } catch (e) {
      return false;
    }
  }

  public async SignIn(smsCode: string) {
    try {
      let result = await this.confirmation.confirm(smsCode);
      let token = await result.user.getIdToken();

      let credentials = new LoginRequest({ UidToken: token, PhoneNumber: this.phoneNumberToConfirm });

      let identity = await this.requestsBuilder.LoginRequest(credentials);
      return this.OnLoginResultReceived(identity);

    } catch (e) {
      return false;
    }
  }

  private OnLoginResultReceived(result: ServerResponse<LoginResponse>): boolean {

    if (!result.isSuccessfull) {
      return false;
    }

    this.OnUserLoggedIn(result.response);

    if (result.response.isNewUser) {
      this.router.navigateByUrl('/register');
    } else {
      this.router.navigateByUrl('/chat');
    }

    return true;
  }

  private NormalizeMobilePhone(phoneNumber: string) {
    if (!phoneNumber.startsWith("+")) {
      phoneNumber = "+" + phoneNumber;
    }
    return phoneNumber;
  }

  public async RefreshLocalData(): Promise<void> {
    let refreshToken = localStorage.getItem('refreshtoken');
    let user = <AppUser>JSON.parse(localStorage.getItem('user'));
    if (user == null || refreshToken == null) {
      this.router.navigateByUrl('/login');
      return;
    }

    this.User = user;

    let newToken = await this.RefreshToken();

    if (!newToken.isSuccessfull) {
      this.router.navigateByUrl('/login');
      return;
    }

    localStorage.setItem('token', newToken.response);
    this.IsAuthenticated = true;
    this.token = newToken.response;
  }

  public RefreshToken(): Promise<ServerResponse<string>> {

    let refreshToken: string = localStorage.getItem('refreshtoken');

    if (!refreshToken) {
      this.router.navigateByUrl('/login');
      return;
    }

    if(this.IsTokenExpired()){
      return this.requestsBuilder.RefreshJwtToken(refreshToken, this.User.id);
    }else{
      let fakeResponse = new ServerResponse<string>();
      fakeResponse.isSuccessfull = true;
      fakeResponse.response = localStorage.getItem('token');
      return Promise.resolve(fakeResponse);
    }
  }

  public IsTokenExpired(){
    let token = localStorage.getItem('token');

    if(!token){
      return true;
    }

    class tokenDto{
      exp: number;
    }

    try {
        let decoded = jwtDecode<tokenDto>(token);
        let x = new Date().getTime();
        return (decoded.exp - this.MaxMillisecondsForTokenToExpire) <= x;
    }
    catch (e) {
        return true;
    }
  }

  public OnUserLoggedIn(credentials: LoginResponse): void {
    this.User = credentials.info;
    localStorage.setItem('token', credentials.token);
    localStorage.setItem('refreshtoken', credentials.refreshToken);
    localStorage.setItem('user', JSON.stringify(credentials.info));
    this.IsAuthenticated = true;
    this.token = credentials.token;
  }

  public LogOut(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshtoken');
    localStorage.removeItem('user');
    localStorage.removeItem('authKeys');
    this.router.navigateByUrl('/login');
  }

}
