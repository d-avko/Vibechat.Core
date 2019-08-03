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

  public uploadImagesToChat(files: FileList, progress: (value: number) => void, chatId: string) : Observable<any> {
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

    let headers = new HttpHeaders();
    headers = headers.append("ngsw-bypass", "");

    const req = new HttpRequest('POST', '/Files/UploadImages', data, {
      reportProgress: true,
      headers: headers
    });

    return <Observable<any>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, progress)),
      last(),
      map(x => (<HttpResponse<any>>x).body),
      catchError(this.handleError(files))
    );
  }

  public uploadChatPicture(file: File, progress: (value: number) => void, chatId: number) {
    if (!file) {
      return;
    }

    if (!this.CheckIfRightSize(file, true)) {
      return;
    }

    let data = new FormData();
    data.append('thumbnail', file);
    data.append('conversationId', chatId.toString());

    let headers = new HttpHeaders();
    headers = headers.append("ngsw-bypass", "");

    const req = new HttpRequest('POST', 'api/Conversations/UpdateThumbnail', data, {
      reportProgress: true,
      headers: headers
    });

    return <Observable<any>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, progress)),
      last(),
      map(x => (<HttpResponse<any>>x).body),
      catchError(this.handleError(null, file, true))
    );
  }

  public uploadUserPicture(file: File, progress: (value: number) => void) {
    if (!file) {
      return;
    }

    if (!this.CheckIfRightSize(file, true)) {
      return;
    }

    let data = new FormData();
    data.append('picture', file);

    let headers = new HttpHeaders();
    headers = headers.append("ngsw-bypass", "");

    const req = new HttpRequest('POST', 'api/Users/UpdateProfilePicture', data, {
      reportProgress: true,
      headers: headers
    });

    return <Observable<any>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, progress)),
      last(),
      map(x => (<HttpResponse<any>>x).body),
      catchError(this.handleError(null, file, true))
    );
  }

  public uploadFile(file: File, progress: (value: number) => void, chatId: string) {
    if (!file) {
      return;
    }

    if (!this.CheckIfRightSize(file, false)) {
      return;
    }

    let data = new FormData();
    data.append('file', file);
    data.append('ChatId', chatId);

    let headers = new HttpHeaders();
    headers = headers.append("ngsw-bypass", "");

    const req = new HttpRequest('POST', 'Files/UploadFile', data, {
      reportProgress: true,
      headers: headers
    });

    return <Observable<any>>this.http.request(req).pipe(
      tap(event => this.showProgress(event, progress)),
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
  private getEventProgress(event: HttpEvent<any>): number {

    switch (event.type) {
      case HttpEventType.Sent:
        return 1;

      case HttpEventType.UploadProgress:
        // Compute and show the % done:
        const percentDone = Math.round(100 * event.loaded / event.total);
        return percentDone;

      case HttpEventType.Response:
        return 100;
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

  private showProgress(event: HttpEvent<any>, progress: (value: number) => void) {
    progress(this.getEventProgress(event));
  }
}
