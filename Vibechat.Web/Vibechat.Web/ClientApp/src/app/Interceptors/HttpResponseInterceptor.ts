import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpResponse,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';

import { Observable, throwError, from } from 'rxjs';
import { map, catchError, retry, switchMap, mergeMap } from 'rxjs/operators';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';
import { Router } from '@angular/router';
import { ApiRequestsBuilder } from '../Requests/ApiRequestsBuilder';
import { ServerResponse } from '../ApiModels/ServerResponse';
import { AuthService } from '../Auth/AuthService';

@Injectable()
export class HttpResponseInterceptor implements HttpInterceptor {

  constructor(
    public errorsLogger: SnackBarHelper,
    public router: Router,
    public requestsBuilder: ApiRequestsBuilder,
    public authService: AuthService)
  {
    
  }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let token: string = localStorage.getItem('token');
    
    if (!token || request.headers.has('unauthorized')) {
      return this.handleRequest(request, next);
    }

    if (!request.headers.has('refreshtoken')) {

      request = request.clone({ headers: request.headers.set('refreshtoken', '1') });

      return from(this.authService.RefreshToken())
        .pipe(
          switchMap(
            response => {

              let token = this.handleRefreshTokenResponse(response);

              if (!token) {
                return;
              }

              request = request.clone({ headers: request.headers.set('Authorization', 'Bearer ' + token) });
              return this.handleRequest(request, next);
            }
          )
        )

    } else {
      request = request.clone({ headers: request.headers.set('Authorization', 'Bearer ' + token) });
      return this.handleRequest(request, next);
    }
  }

  public handleRefreshTokenResponse(response: ServerResponse<string>) : string {
    if (!response.isSuccessfull) {
      this.router.navigateByUrl('/login');
      return;
    }

    this.authService.token = response.response;
    localStorage.setItem('token', response.response);
    return response.response;
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

          from(this.authService.RefreshToken())
            .subscribe((result) => {
              this.handleRefreshTokenResponse(result);
            });

          return;
        }

        this.errorsLogger.openSnackBar('HttpError: ' + error.message);
        return throwError(error);
      }));
  }
}
