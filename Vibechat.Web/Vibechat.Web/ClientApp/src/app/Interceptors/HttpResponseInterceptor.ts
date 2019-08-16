import {Inject, Injectable} from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpResponse
} from '@angular/common/http';

import {from, Observable, throwError} from 'rxjs';
import {catchError, map, switchMap} from 'rxjs/operators';
import {SnackBarHelper} from '../Snackbar/SnackbarHelper';
import {Router} from '@angular/router';
import {Api} from '../Services/Api/api.service';
import {ServerResponse} from '../ApiModels/ServerResponse';
import {AuthService} from '../Services/AuthService';
import {MessageReportingService} from "../Services/MessageReportingService";
import {LocalesService} from "../Services/LocalesService";

@Injectable()
export class HttpResponseInterceptor implements HttpInterceptor {

  constructor(
    public errorsLogger: SnackBarHelper,
    public router: Router,
    public requestsBuilder: Api,
    public authService: AuthService,
    public messages: MessageReportingService,
    @Inject('BASE_URL') private baseUrl,
    private locales: LocalesService)
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

    return response.response;
  }

  public handleRequest(request: HttpRequest<any>, next: HttpHandler) {
    return next.handle(request).pipe(
      map((event: HttpEvent<any>) => {

        if (event instanceof HttpResponse) {
          //all requests contain failed / not failed payload
          if (!event.body.isSuccessfull) {
            this.messages.ApiError(event.body.errorMessage);
          }

        }

        return event;
      }),
      catchError((error: HttpErrorResponse) => {

        switch (error.status) {
          case 401: {
            from(this.authService.RefreshToken())
              .subscribe((result) => {
                this.handleRefreshTokenResponse(result);
              });

            break;
          }
          //Gateway timeout
          case 504:
          case 0: {
            this.messages.NoInternet();
            break;
          }
          default: {
            this.messages.ServerReturnedError(error.status, error.error);
          }
        }

        return throwError(error);
      }));
  }
}
