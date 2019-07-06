import { Injectable } from '@angular/core';
import {
  HttpClient, HttpEvent, HttpEventType, HttpRequest, HttpErrorResponse, HttpHeaders, HttpResponse
} from '@angular/common/http';

import { of, Observable } from 'rxjs';
import { catchError, last, map, tap } from 'rxjs/operators';
import { SnackBarHelper } from '../Snackbar/SnackbarHelper';

@Injectable()
export class UploaderService {

  public static maxUploadImageSizeMb: number = 5;

  public static maxFileUploadSizeMb: number = 25;

  constructor(
    private http: HttpClient,
    private logger: SnackBarHelper) { }

  public uploadImagesToChat(files: FileList, chatId: string) : Observable<any> {
    if (!files || files.length == 0) { return; }

    for (let i = 0; i < files.length; ++i) {
      if (!this.CheckIfRightSize(files[i], true)) {
        return;
      }
    }

    let data = new FormData();

    for (let i = 0; i < files.length; ++i) {
      data.append('images', files[i]);
      data.append('ChatId', chatId);
    }

    const req = new HttpRequest('POST', '/Files/UploadImages', data, {
      reportProgress: true
    });


    return <Observable<any>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, files)),
      last(),
      map(x => (<HttpResponse<any>>x).body),
      catchError(this.handleError(files))
    );
  }

  public uploadChatPicture(file: File, chatId: number) {
    if (!file) {
      return;
    }

    if (!this.CheckIfRightSize(file, true)) {
      return;
    }

    let data = new FormData();
    data.append('thumbnail', file);
    data.append('conversationId', chatId.toString());

    const req = new HttpRequest('POST', 'api/Conversations/UpdateThumbnail', data, {
      reportProgress: true
    });

    return <Observable<any>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, null, file, true)),
      last(),
      map(x => (<HttpResponse<any>>x).body),
      catchError(this.handleError(null, file, true))
    );
  }

  public uploadUserPicture(file: File) {
    if (!file) {
      return;
    }

    if (!this.CheckIfRightSize(file, true)) {
      return;
    }

    let data = new FormData();
    data.append('picture', file);

    const req = new HttpRequest('POST', 'api/Users/UpdateProfilePicture', data, {
      reportProgress: true
    });

    return <Observable<any>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, null, file, true)),
      last(),
      map(x => (<HttpResponse<any>>x).body),
      catchError(this.handleError(null, file, true))
    );
  }

  public uploadFile(file: File, chatId: string) {
    if (!file) {
      return;
    }

    if (!this.CheckIfRightSize(file, false)) {
      return;
    }

    let data = new FormData();
    data.append('file', file);
    data.append('ChatId', chatId);

    const req = new HttpRequest('POST', 'Files/UploadFile', data, {
      reportProgress: true
    });

    return <Observable<any>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, null, file, true)),
      last(),
      map(x => (<HttpResponse<any>>x).body),
      catchError(this.handleError(null, file, true))
    );
  }

  public CheckIfRightSize(file: File, isImage: boolean) {

    if (isImage) {
      if (((file.size / 1024) / 1024) > UploaderService.maxUploadImageSizeMb) {
        this.logger.openSnackBar("Some of the files were larger than " + UploaderService.maxUploadImageSizeMb + "MB");
        return false;
      }
    }
    else {
      if (((file.size / 1024) / 1024) > UploaderService.maxFileUploadSizeMb) {
        this.logger.openSnackBar("Some of the files were larger than " + UploaderService.maxFileUploadSizeMb + "MB");
        return false;
      }
    }

    return true;
  }

  /** Return distinct message for sent, upload progress, & response events */
  private getEventMessage(event: HttpEvent<any>, files: FileList, file: File = null, isOneFile: boolean = false) {

    if (!isOneFile) {

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
    } else {

      switch (event.type) {
        case HttpEventType.Sent:
          return `Uploading "${file.name}"`;

        case HttpEventType.UploadProgress:
          // Compute and show the % done:
          const percentDone = Math.round(100 * event.loaded / event.total);
          return `Upload progress is ${percentDone}% .`;

        case HttpEventType.Response:
          return `"${file.name}" was successfully uploaded!`;
      }

    }
  }

  /**
   * Returns a function that handles Http upload failures.
   * @param file - File object for file being uploaded
   *
   * When no `UploadInterceptor` and no server,
   * you'll end up here in the error handler.
   */
  private handleError(files: FileList, file: File = null, isOneFile: boolean = false) {
    const userMessage = isOneFile ? `Failed to upload ${file.name} .` : `Failed to upload ${files.length} files.`;

    return (error: HttpErrorResponse) => {

      console.error(error); 

      const message = (error.error instanceof Error) ?
        error.error.message :
        `server returned code ${error.status} with error "${error.error}"`;

      this.logger.openSnackBar(message);

      // Let app keep running but indicate failure.
      return of(userMessage);
    };
  }

  private showProgress(event: HttpEvent<any>, files: FileList, file: File = null, isOneFile: boolean = false) {
    this.logger.openSnackBar(this.getEventMessage(event, files, file, isOneFile), 0.5);
  }
}


/*
Copyright Google LLC. All Rights Reserved.
Use of this source code is governed by an MIT-style license that
can be found in the LICENSE file at http://angular.io/license
*/
