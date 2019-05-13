import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { UploadFilesResponse } from "../Data/UploadFilesResponse";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class FileUploader{
  private http: HttpClient;

  constructor(http: HttpClient) {
    this.http = http;
  }

  public Upload(files: FileList): Observable<ServerResponse<UploadFilesResponse>> {
    return this.http.post<ServerResponse<UploadFilesResponse>>("Files/Upload", files);
  }
}
