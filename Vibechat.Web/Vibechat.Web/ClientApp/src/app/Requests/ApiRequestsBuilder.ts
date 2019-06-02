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

@Injectable({
  providedIn: 'root'
})
export class ApiRequestsBuilder {

  public static maxUploadImageSizeMb: number = 5;

  private baseUrl: string;

  private httpClient: HttpClient;

  private uploader: UploaderService;

  constructor(httpClient: HttpClient, @Inject('BASE_URL') baseUrl: string, uploader: UploaderService) {
    this.baseUrl = baseUrl;
    this.httpClient = httpClient;
    this.uploader = uploader;
  }

  public LoginRequest(credentials: LoginRequest): Observable<ServerResponse<LoginResponse>>{
    return this.MakeNonAuthorizedCall<LoginResponse>(
      credentials,
      'api/login'
    );
  }

  public RegisterRequest(credentials: RegisterRequest): Observable<ServerResponse<string>> {
    return this.MakeNonAuthorizedCall<string>(
      credentials,
      'api/register'
    );
  }

  public UpdateConversationsRequest(): Observable<ServerResponse<Array<ConversationTemplate>>> {

    return this.MakeAuthorizedCall<Array<ConversationTemplate>>(
      null,
      'api/Conversations/GetAll'
    );
  }

  public GetConversationMessages(offset: number, count: number, conversationId: number): Observable<ServerResponse<ConversationMessagesResponse>> {

    return this.MakeAuthorizedCall<ConversationMessagesResponse>(
        {
          Count: count,
          ConversationID: conversationId,
          MesssagesOffset: offset
        },
      'api/Conversations/GetMessages'
    );
  }

  public DeleteMessages(messages: Array<ChatMessage>, conversationId: number) : Observable<ServerResponse<string>> {

    return this.MakeAuthorizedCall<string>(
        {
          MessagesId: messages.map(x => x.id),
          ConversationId: conversationId
        },
      'api/Conversations/DeleteMessages'
    );

  }

  public UploadImages(files: FileList): Observable<HttpEvent<any>> {
   return this.uploader.uploadImages(files);
  }

  public UploadConversationThumbnail(thumbnail: File, conversationId: number): Observable<ServerResponse<UpdateThumbnailResponse>> {
    let data = new FormData();
    data.append('thumbnail', thumbnail);
    data.append('conversationId', conversationId.toString());

    return this.MakeAuthorizedCall<UpdateThumbnailResponse>(
      data,
      'api/Conversations/UpdateThumbnail'
    );
  }

  public UploadUserProfilePicture(picture: File): Observable<ServerResponse<UpdateThumbnailResponse>> {
    let data = new FormData();
    data.append('picture', picture);

    return this.MakeAuthorizedCall<UpdateThumbnailResponse>(
      data,
      'api/Users/UpdateProfilePicture'
    );
  }

  public GetUserById(userId: string): Observable<ServerResponse<UserInfo>> {
    return this.MakeAuthorizedCall<UserInfo>(
      { Id: userId },
      'api/Users/GetById'
    );
  }

  public GetConversationById(conversationId: number): Observable<ServerResponse<ConversationTemplate>> {
    return this.MakeAuthorizedCall<ConversationTemplate>(
      { conversationId: conversationId },
      'api/Conversations/GetById'
    );
  }

  public ChangeCurrentUserName(newName: string): Observable<ServerResponse<boolean>> {
    return this.MakeAuthorizedCall<boolean>(
      { newName: newName },
      'api/Users/ChangeName'
    );
  }

  public ChangeCurrentUserLastName(newName: string): Observable<ServerResponse<boolean>> {
    return this.MakeAuthorizedCall<boolean>(
      { newName: newName },
      'api/Users/ChangeLastName'
    );
  }

  public FindUsersByUsername(username: string): Observable<ServerResponse<FoundUsersResponse>> {
    return this.MakeAuthorizedCall<FoundUsersResponse>(
      { UsernameToFind: username },
      'api/Users/FindByNickname'
    );
  }

  public GetAttachmentsForConversation(conversationId: number, kind: string, offset: number, count: number) {
    return this.MakeAuthorizedCall<Array<ChatMessage>>(
      {
        conversationId: conversationId,
        kind: kind,
        offset: offset,
        count: count
      },
      'api/Conversations/GetAttachments'
    );
  }

  public CreateConversation(
    name: string,
    whoCreatedId: string,
    dialogUserId: string,
    thumbnailUrl: string,
    isGroup: boolean,
    isPublic: boolean)
  : Observable<ServerResponse<ConversationTemplate>>
  {
    return this.MakeAuthorizedCall<ConversationTemplate>(
      {
        ConversationName: name,
        CreatorId: whoCreatedId,
        DialogUserId: dialogUserId,
        ImageUrl: thumbnailUrl,
        IsGroup: isGroup,
        IsPublic: isPublic
      },
      'api/Conversations/Create'
    );

  }

  public ChangeConversationName(newName: string, conversationId: number): Observable<ServerResponse<boolean>> {

    return this.MakeAuthorizedCall<boolean>(
      { ConversationId: conversationId, Name: newName },
      'api/Conversations/ChangeName'
    );

  }

  public RefreshJwtToken(refreshToken: string, userId: string): Observable<ServerResponse<string>>{
    return this.MakeNonAuthorizedCall<string>(
      { RefreshToken: refreshToken, userId: userId },
      'api/Tokens/Refresh'
    );
  }

  public SearchForGroups(searchstring: string): Observable<ServerResponse<Array<ConversationTemplate>>>{

    return this.MakeAuthorizedCall<Array<ConversationTemplate>>(
      { SearchString: searchstring },
      'api/Conversations/SearchGroups'
    );

  }

  public UnbanUser(userId: string): Observable<ServerResponse<boolean>> {

    return this.MakeAuthorizedCall<boolean>(
      { userId: userId },
      'api/Users/Unban'
    );

  }

  public BanUser(userId: string, conversationId: number): Observable<ServerResponse<boolean>> {
    return this.MakeAuthorizedCall<boolean>(
      { userId: userId, conversationId: conversationId == 0 ? null : conversationId },
      'api/Users/Block'
    );
  }

  public BanFromConversation(userId: string, conversationId: number): Observable<ServerResponse<boolean>> {
    return this.MakeAuthorizedCall<boolean>(
      { userId: userId, conversationId: conversationId },
      'api/Conversations/BanFrom'
    );
  }

  private MakeAuthorizedCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data);
  }

  private MakeNonAuthorizedCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data);
  }

}
