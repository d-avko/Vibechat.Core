import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpResponse,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';

import { Observable, throwError, BehaviorSubject, from } from 'rxjs';
import { map, catchError, retry, switchMap, mergeMap } from 'rxjs/operators';
import { MatSnackBar } from '@angular/material';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';
import { Router } from '@angular/router';
import { ApiRequestsBuilder } from '../Requests/ApiRequestsBuilder';
import { Cache } from '../Auth/Cache';
import { TokensService } from '../tokens/TokensService';
import { ServerResponse } from '../ApiModels/ServerResponse';

@Injectable()
export class HttpResponseInterceptor implements HttpInterceptor {

  constructor(public errorsLogger: SnackBarHelper, public router: Router, public requestsBuilder: ApiRequestsBuilder, public tokensService: TokensService)
  {
    
  }

  public static IsRefreshingToken: boolean;

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    let token: string = localStorage.getItem('token');

    if (!token) {
      return this.handleRequest(request, next);
    }

    if (!HttpResponseInterceptor.IsRefreshingToken) {

      HttpResponseInterceptor.IsRefreshingToken = true;

      return from(this.tokensService.RefreshToken())
        .pipe(
          mergeMap(
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
      HttpResponseInterceptor.IsRefreshingToken = false;
      return;
    }

    Cache.token = response.response;
    localStorage.setItem('token', response.response);
    HttpResponseInterceptor.IsRefreshingToken = false;
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

          HttpResponseInterceptor.IsRefreshingToken = true;

          from(this.tokensService.RefreshToken())
            .subscribe((result) => {
              this.handleRefreshTokenResponse(result);
            });

          window.location.reload();
          return;
        }

        this.errorsLogger.openSnackBar('HttpError: ' + error.message);
        return throwError(error);
      }));
  }
}
