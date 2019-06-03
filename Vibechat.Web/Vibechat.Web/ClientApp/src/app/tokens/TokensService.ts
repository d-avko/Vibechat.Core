import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { Cache } from "../Auth/Cache";
import { Observable } from "rxjs";
import { ServerResponse } from "../ApiModels/ServerResponse";

@Injectable({
  providedIn: 'root'
})
export class TokensService{
  constructor(public router: Router, public requestsBuilder: ApiRequestsBuilder) { }

  public RefreshToken() : Observable<ServerResponse<string>> {

    let refreshToken: string = localStorage.getItem('refreshtoken');

    if (!refreshToken) {
      this.router.navigateByUrl('/login');
      return;
    }

    return this.requestsBuilder.RefreshJwtToken(refreshToken, Cache.UserCache.id);
  }

}
