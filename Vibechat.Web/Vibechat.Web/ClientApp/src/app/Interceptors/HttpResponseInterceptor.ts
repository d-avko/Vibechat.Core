import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpResponse,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';

import { Observable, throwError, BehaviorSubject } from 'rxjs';
import { map, catchError, retry } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';
import { Router } from '@angular/router';
import { ApiRequestsBuilder } from '../Requests/ApiRequestsBuilder';
import { Cache } from '../Auth/Cache';

@Injectable()
export class HttpResponseInterceptor implements HttpInterceptor {

  constructor(public errorsLogger: SnackBarHelper, public router: Router, public requestsBuilder: ApiRequestsBuilder)
  {
    setTimeout(() => {
      this.RefreshToken()
    }, 1000 * 60 * 3)
  }

  RefreshTokenWithTimeout() {
    let refreshToken: string = localStorage.getItem('refreshtoken');

    if (!refreshToken) {
      this.router.navigateByUrl('/login');
      return;
    }

    localStorage.removeItem('token');

    this.requestsBuilder.RefreshJwtToken(refreshToken, Cache.UserCache.id).subscribe((result) => {

      if (!result.isSuccessfull) {
        this.router.navigateByUrl('/login');
        return;
      }

      Cache.token = result.response;
      localStorage.setItem('token', result.response);
    })

    setTimeout(() => {
      this.RefreshTokenWithTimeout()
    }, 1000 * 60 * 3)
  }

  RefreshToken() {

    let refreshToken: string = localStorage.getItem('refreshtoken');

    if (!refreshToken) {
      this.router.navigateByUrl('/login');
      return;
    }

    localStorage.removeItem('token');

    this.requestsBuilder.RefreshJwtToken(refreshToken, Cache.UserCache.id).subscribe((result) => {

      if (!result.isSuccessfull) {
        this.router.navigateByUrl('/login');
        return;
      }

      Cache.token = result.response;
      localStorage.setItem('token', result.response);
    })

  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let token: string = localStorage.getItem('token');
    if (!token) {
      return this.handleRequest(request, next);
    }

    request = request.clone({ headers: request.headers.set('Authorization', 'Bearer ' + token) });

    return this.handleRequest(request, next);
  }

  public handleRequest(request: HttpRequest<any>, next: HttpHandler) {
    return next.handle(request).pipe(
      map((event: HttpEvent<any>) => {

        if (event instanceof HttpResponse) {
          //all requests contain failed / not failed payload
          if (!event.body.isSuccessfull) {
            this.errorsLogger.openSnackBar('Error: ' + event.body.errorMessage);
          }

        }

        return event;
      }),
      catchError((error: HttpErrorResponse) => {

        if (error.status == 401) {
          this.RefreshToken();
          window.location.reload();
          return;
        }

        this.errorsLogger.openSnackBar('HttpError: ' + error.message);
        return throwError(error);
      }));
  }
}
