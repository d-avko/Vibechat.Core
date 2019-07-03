import { LoginResponse } from "../ApiModels/LoginResponse";
import { UserInfo } from "../Data/UserInfo";
import { Router } from "@angular/router";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { Injectable } from '@angular/core';
import * as firebase from "firebase/app";
import "firebase/auth";

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

  public User: UserInfo;

  public Contacts: Array<UserInfo>;

  public token: string;

  public IsAuthenticated: boolean;

  public SignIn() {

  }

  public async TryAuthenticate(): Promise<void> {
    let refreshToken = localStorage.getItem('refreshtoken');
    let user = <UserInfo>JSON.parse(localStorage.getItem('user'));
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

  public async RefreshToken(): Promise<ServerResponse<string>> {

    let refreshToken: string = localStorage.getItem('refreshtoken');

    if (!refreshToken) {
      this.router.navigateByUrl('/login');
      return;
    }

    return this.requestsBuilder.RefreshJwtToken(refreshToken, this.User.id);
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
