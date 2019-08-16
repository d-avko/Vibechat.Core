import {ServerResponse} from "../../ApiModels/ServerResponse";
import {Inject} from "@angular/core";
import {UploaderService} from "./uploads/upload.service";
import {UpdateThumbnailResponse} from "../../ApiModels/UpdateThumbnailResponse";
import {Attachment} from "../../Data/Attachment";

export class UploadsApi {

  constructor(@Inject('BASE_URL') private baseUrl: string,
              private uploader: UploaderService){

  }

  public UploadImages(files: FileList, progress: (value: number) => void, chatId: number): Promise<ServerResponse<any>> {
    return this.uploader.uploadImagesToChat(files, progress, chatId.toString()).toPromise();
  }

  public UploadConversationThumbnail(thumbnail: File, progress: (value: number) => void, conversationId: number): Promise<ServerResponse<UpdateThumbnailResponse>> {
    return this.uploader.uploadChatPicture(thumbnail, progress, conversationId).toPromise();
  }

  public UploadUserProfilePicture(picture: File, progress: (value: number) => void): Promise<ServerResponse<UpdateThumbnailResponse>> {
    return this.uploader.uploadUserPicture(picture, progress).toPromise();
  }

  public UploadFile(file: File, progress: (value: number) => void, chatId: number) : Promise<ServerResponse<Attachment>> {
    return this.uploader.uploadFile(file, progress, chatId.toString()).toPromise();
  }

}
