import { Observable } from "rxjs";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { LoginResponse } from "../ApiModels/LoginResponse";
import { LoginRequest } from "../ApiModels/LoginRequest";
import { HttpClient, HttpHeaders, HttpEvent } from "@angular/common/http";
import { RegisterRequest } from "../ApiModels/RegisterRequest";
import { Injectable, Inject } from "@angular/core";
import { ConversationMessagesResponse } from "../ApiModels/ConversationMessagesResponse";
import { ChatMessage } from "../Data/ChatMessage";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { FoundUsersResponse } from "../Data/FoundUsersResponse";
import { UpdateThumbnailResponse } from "../ApiModels/UpdateThumbnailResponse";
import { UserInfo } from "../Data/UserInfo";
import { UploaderService } from "../uploads/upload.service";
import {
} from "@angular/material";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";

@Injectable({
  providedIn: 'root'
})
export class ApiRequestsBuilder {

  private baseUrl: string;

  private httpClient: HttpClient;

  private uploader: UploaderService;

  private logger: SnackBarHelper

  constructor(httpClient: HttpClient, @Inject('BASE_URL') baseUrl: string, uploader: UploaderService, logger: SnackBarHelper) {
    this.baseUrl = baseUrl;
    this.httpClient = httpClient;
    this.uploader = uploader;
    this.logger = logger;
  }

  public LoginRequest(credentials: LoginRequest): Observable<ServerResponse<LoginResponse>>{
    return this.MakeCall<LoginResponse>(
      credentials,
      'api/login'
    );
  }

  public RegisterRequest(credentials: RegisterRequest): Observable<ServerResponse<string>> {
    return this.MakeCall<string>(
      credentials,
      'api/register'
    );
  }

  public UpdateConversationsRequest(): Promise<ServerResponse<Array<ConversationTemplate>>> {

    return this.MakeCall<Array<ConversationTemplate>>(
      null,
      'api/Conversations/GetAll'
    ).toPromise();
  }

  public GetConversationMessages(offset: number, count: number, conversationId: number): Promise<ServerResponse<Array<ChatMessage>>> {

    return this.MakeCall<Array<ChatMessage>>(
        {
          Count: count,
          ConversationID: conversationId,
          MesssagesOffset: offset
        },
      'api/Conversations/GetMessages'
    ).toPromise();
  }

  public DeleteMessages(messages: Array<ChatMessage>, conversationId: number) : Promise<ServerResponse<string>> {

    return this.MakeCall<string>(
        {
          MessagesId: messages.map(x => x.id),
          ConversationId: conversationId
        },
      'api/Conversations/DeleteMessages'
    ).toPromise();

  }

  public UploadImages(files: FileList): Promise<HttpEvent<any>> {
   return this.uploader.uploadImages(files).toPromise();
  }

  public UploadConversationThumbnail(thumbnail: File, conversationId: number): Promise<ServerResponse<UpdateThumbnailResponse>> {
    let data = new FormData();
    data.append('thumbnail', thumbnail);
    data.append('conversationId', conversationId.toString());

    return this.MakeCall<UpdateThumbnailResponse>(
      data,
      'api/Conversations/UpdateThumbnail'
    ).toPromise();
  }

  public UploadUserProfilePicture(picture: File): Promise<ServerResponse<UpdateThumbnailResponse>> {
    let data = new FormData();
    data.append('picture', picture);

    return this.MakeCall<UpdateThumbnailResponse>(
      data,
      'api/Users/UpdateProfilePicture'
    ).toPromise();
  }

  public GetUserById(userId: string): Promise<ServerResponse<UserInfo>> {
    return this.MakeCall<UserInfo>(
      { Id: userId },
      'api/Users/GetById'
    ).toPromise();
  }

  public GetConversationById(conversationId: number): Promise<ServerResponse<ConversationTemplate>> {
    return this.MakeCall<ConversationTemplate>(
      { conversationId: conversationId },
      'api/Conversations/GetById'
    ).toPromise();
  }

  public ChangeCurrentUserName(newName: string): Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      { newName: newName },
      'api/Users/ChangeName'
    ).toPromise();
  }

  public ChangeCurrentUserLastName(newName: string): Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      { newName: newName },
      'api/Users/ChangeLastName'
    ).toPromise();
  }

  public FindUsersByUsername(username: string): Promise<ServerResponse<FoundUsersResponse>> {
    return this.MakeCall<FoundUsersResponse>(
      { UsernameToFind: username },
      'api/Users/FindByNickname'
    ).toPromise();
  }

  public GetAttachmentsForConversation(conversationId: number, kind: string, offset: number, count: number) {
    return this.MakeCall<Array<ChatMessage>>(
      {
        conversationId: conversationId,
        kind: kind,
        offset: offset,
        count: count
      },
      'api/Conversations/GetAttachments'
    ).toPromise();
  }

  public CreateConversation(
    name: string,
    whoCreatedId: string,
    dialogUserId: string,
    thumbnailUrl: string,
    isGroup: boolean,
    isPublic: boolean)
  : Promise<ServerResponse<ConversationTemplate>>
  {
    return this.MakeCall<ConversationTemplate>(
      {
        ConversationName: name,
        CreatorId: whoCreatedId,
        DialogUserId: dialogUserId,
        ImageUrl: thumbnailUrl,
        IsGroup: isGroup,
        IsPublic: isPublic
      },
      'api/Conversations/Create'
    ).toPromise();

  }

  public ChangeConversationName(newName: string, conversationId: number): Promise<ServerResponse<boolean>> {

    return this.MakeCall<boolean>(
      { ConversationId: conversationId, Name: newName },
      'api/Conversations/ChangeName'
    ).toPromise();

  }

  public RefreshJwtToken(refreshToken: string, userId: string): Promise<ServerResponse<string>>{
    let headers = new HttpHeaders();
    headers = headers.append('refreshtoken', '1');

    return this.httpClient.post<ServerResponse<string>>(
      this.baseUrl + 'api/Tokens/Refresh',
      { RefreshToken: refreshToken, userId: userId },
      { headers: headers}).toPromise();
  }

  public SearchForGroups(searchstring: string): Promise<ServerResponse<Array<ConversationTemplate>>>{

    return this.MakeCall<Array<ConversationTemplate>>(
      { SearchString: searchstring },
      'api/Conversations/SearchGroups'
    ).toPromise();

  }

  public UnbanUser(userId: string): Promise<ServerResponse<boolean>> {

    return this.MakeCall<boolean>(
      { userId: userId },
      'api/Users/Unban'
    ).toPromise();

  }

  public BanUser(userId: string): Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      { userId: userId},
      'api/Users/Block'
    ).toPromise();
  }

  public BanFromConversation(userId: string, conversationId: number): Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      { userId: userId, conversationId: conversationId },
      'api/Conversations/BanFrom'
    ).toPromise();
  }

  public UnBanFromConversation(userId: string, conversationId: number): Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      { userId: userId, conversationId: conversationId },
      'api/Conversations/UnbanFrom'
    ).toPromise();
  }

  private MakeCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data);
  }

}
