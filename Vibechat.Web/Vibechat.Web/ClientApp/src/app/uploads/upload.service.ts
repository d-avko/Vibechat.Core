import { Injectable } from '@angular/core';
import {
  HttpClient, HttpEvent, HttpEventType, HttpRequest, HttpErrorResponse, HttpHeaders
} from '@angular/common/http';

import { of, Observable } from 'rxjs';
import { catchError, last, map, tap } from 'rxjs/operators';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';
import { MatSnackBar } from '@angular/material';

@Injectable()
export class UploaderService {

  private logger: SnackBarHelper;

  constructor(
    private http: HttpClient,
    snackbar: MatSnackBar) { this.logger = new SnackBarHelper(snackbar); }

  public uploadImages(files: FileList, token: string) : Observable<HttpEvent<any>> {
    if (!files || files.length == 0) { return; }

    let data = new FormData();

    for (let i = 0; i < files.length; ++i) {
      data.append('images', files[i]);
    }

    // Create the request object that POSTs the file to an upload endpoint.
    // The `reportProgress` option tells HttpClient to listen and return
    // XHR progress events.
    const req = new HttpRequest('POST', '/Files/UploadImages', data, {
      reportProgress: true
    });


    return <Observable<HttpEvent<any>>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, files)),
      last(),
      catchError(this.handleError(files))
    );
  }

  /** Return distinct message for sent, upload progress, & response events */
  private getEventMessage(event: HttpEvent<any>, files: FileList) {
    switch (event.type) {
      case HttpEventType.Sent:
        return `Uploading "${files.length}" files.`;

      case HttpEventType.UploadProgress:
        // Compute and show the % done:
        const percentDone = Math.round(100 * event.loaded / event.total);
        return `Upload progress is ${percentDone}% .`;

      case HttpEventType.Response:
        return `Files were completely uploaded!`;
    }
  }

  /**
   * Returns a function that handles Http upload failures.
   * @param file - File object for file being uploaded
   *
   * When no `UploadInterceptor` and no server,
   * you'll end up here in the error handler.
   */
  private handleError(files: FileList) {
    const userMessage = `Failed to upload ${files.length} files.`;

    return (error: HttpErrorResponse) => {

      console.error(error); 

      const message = (error.error instanceof Error) ?
        error.error.message :
        `server returned code ${error.status} with body "${error.error}"`;

      this.logger.openSnackBar(message);

      // Let app keep running but indicate failure.
      return of(userMessage);
    };
  }

  private showProgress(event: HttpEvent<any>, files: FileList) {
    this.logger.openSnackBar(this.getEventMessage(event, files), 0.5);
  }
}


/*
Copyright Google LLC. All Rights Reserved.
Use of this source code is governed by an MIT-style license that
can be found in the LICENSE file at http://angular.io/license
*/
