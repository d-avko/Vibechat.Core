import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpResponse,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';

import { Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';
import { Router } from '@angular/router';
import { ApiRequestsBuilder } from '../Requests/ApiRequestsBuilder';
import { Cache } from '../Auth/Cache';

@Injectable()
export class HttpResponseInterceptor implements HttpInterceptor {

  private errorsLogger: SnackBarHelper;

  constructor(public errorDialogService: MatSnackBar, public router: Router, public requestsBuilder: ApiRequestsBuilder) { this.errorsLogger = new SnackBarHelper(errorDialogService); }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let token: string = localStorage.getItem('token');

    if (token) {
      let parsedToken = this.parseJwt(token);

      //update token every 3 minutes.

      if (Date.now() - 3 * 1000 * 60 >= parsedToken.exp * 1000) {
        const refreshToken: string = localStorage.getItem('refreshtoken');

        //this is to avoid stack overflow
        localStorage.removeItem('token');

        this.requestsBuilder.RefreshJwtToken(refreshToken, Cache.UserCache.id)
          .subscribe((result) => {

            if (!result.isSuccessfull) {
              this.router.navigateByUrl('/login');
              return;
            }

            localStorage.setItem('token', result.response);
            token = result.response;
          })

      }

      request = request.clone({ headers: request.headers.set('Authorization', 'Bearer ' + token) });
    }
    
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
          this.router.navigateByUrl('/login');
          return;
        }

        this.errorsLogger.openSnackBar('HttpError: ' + error.message);
        return throwError(error);
      }));
  }

  public parseJwt(token) : any {
    var base64Url = token.split('.')[1];
    var base64 = decodeURIComponent(atob(base64Url).split('').map(function (c) {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));

    return JSON.parse(base64);
  };
}
