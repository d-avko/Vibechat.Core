import {Injectable} from '@angular/core';
import {
  HttpClient,
  HttpErrorResponse,
  HttpEvent,
  HttpEventType,
  HttpResponse
} from '@angular/common/http';

import {Observable, of} from 'rxjs';
import {catchError, last, map, tap} from 'rxjs/operators';
import {SnackBarHelper} from '../../../Snackbar/SnackbarHelper';
import {Api} from "../api.service";
import {MessageReportingService} from "../../MessageReportingService";

@Injectable()
export class UploaderService {

  public static maxUploadImageSizeMb: number = 5;

  public static maxFileUploadSizeMb: number = 25;

  constructor(
    private http: HttpClient,
    private logger: SnackBarHelper,
    private api: Api,
    private messages: MessageReportingService) { }

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

    const req = this.api.UploadImagesRequest(data);

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

    const req = this.api.UpdateChatThumbnail(chatId, data);

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

    const req = this.api.UploadUserProfilePicture(data);

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

    const req = this.api.UploadFile(data);

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
        this.messages.FilesTooBig(UploaderService.maxUploadImageSizeMb);
        return false;
      }
    }
    else {
      if (((file.size / 1024) / 1024) > UploaderService.maxFileUploadSizeMb) {
        this.messages.FilesTooBig(UploaderService.maxFileUploadSizeMb);
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
        return Math.round(100 * event.loaded / event.total);

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

      this.messages.ServerReturnedError(error.status, error.error);

      // Let app keep running but indicate failure.
      return of(userMessage);
    };
  }

  private showProgress(event: HttpEvent<any>, progress: (value: number) => void) {
    progress(this.getEventProgress(event));
  }
}
