//import { Injectable } from "@angular/core";
//import { Router } from "@angular/router";
//import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
//import { Observable } from "rxjs";
//import { ServerResponse } from "../ApiModels/ServerResponse";
//import { AuthService } from "../Auth/AuthService";
//@Injectable({
//  providedIn: 'root'
//})
//export class TokensService{
//  constructor(private router: Router, private requestsBuilder: ApiRequestsBuilder, private auth: AuthService) { }
//  public async RefreshToken() : Promise<ServerResponse<string>> {
//    let refreshToken: string = localStorage.getItem('refreshtoken');
//    if (!refreshToken) {
//      this.router.navigateByUrl('/login');
//      return;
//    }
//    return this.requestsBuilder.RefreshJwtToken(refreshToken, this.auth.User.id);
//  }
//}
//# sourceMappingURL=TokensService.js.map