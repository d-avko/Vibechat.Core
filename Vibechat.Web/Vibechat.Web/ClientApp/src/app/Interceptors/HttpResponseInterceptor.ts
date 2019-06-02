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

@Injectable()
export class HttpResponseInterceptor implements HttpInterceptor {

  private errorsLogger: SnackBarHelper;

  constructor(public errorDialogService: MatSnackBar, public router: Router) { this.errorsLogger = new SnackBarHelper(errorDialogService); }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token: string = localStorage.getItem('token');

    if (token) {
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
}
