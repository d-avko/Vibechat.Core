import { Observable } from "rxjs";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { LoginResponse } from "../ApiModels/LoginResponse";
import { LoginRequest } from "../ApiModels/LoginRequest";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { RegisterRequest } from "../ApiModels/RegisterRequest";
import { Injectable, Inject } from "@angular/core";
import { ConversationResponse } from "../ApiModels/ConversationResponse";
import { ConversationsRequest } from "../ApiModels/ConversationRequest";
import { ConversationMessagesRequest } from "../ApiModels/ConversationMessagesRequest";
import { ConversationMessagesResponse } from "../ApiModels/ConversationMessagesResponse";
import { ChatMessage } from "../Data/ChatMessage";
import { DeleteMessagesRequest } from "../Data/DeleteMessagesRequest";
import { UploadFilesResponse } from "../Data/UploadFilesResponse";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { FoundUsersResponse } from "../Data/FoundUsersResponse";
import { forEach } from "@angular/router/src/utils/collection";
import { UpdateThumbnailResponse } from "../ApiModels/UpdateThumbnailResponse";
import { UserInfo } from "../Data/UserInfo";

@Injectable({
  providedIn: 'root'
})
export class ApiRequestsBuilder {

  public static maxUploadImageSizeMb: number = 5;

  private baseUrl: string;

  private httpClient: HttpClient;

  constructor(httpClient: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.httpClient = httpClient;
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

  public UpdateConversationsRequest(token: string, userId: string): Observable<ServerResponse<ConversationResponse>> {

    return this.MakeAuthorizedCall<ConversationResponse>(
      token,
      {
        UserId: userId
      },
      'api/Conversations/GetInfo'
    );
  }

  public GetConversationMessages(offset: number, count: number, conversationId: number, token: string): Observable<ServerResponse<ConversationMessagesResponse>> {

    return this.MakeAuthorizedCall<ConversationMessagesResponse>(
      token,
        {
          Count: count,
          ConversationID: conversationId,
          MesssagesOffset: offset
        },
      'api/Conversations/GetMessages'
    );
  }

  public DeleteMessages(messages: Array<ChatMessage>, conversationId: number, token: string) : Observable<ServerResponse<string>> {

    return this.MakeAuthorizedCall<string>(
      token,
        {
          MessagesId: messages.map(x => x.id),
          ConversationId: conversationId
        },
      'api/Conversations/DeleteMessages'
    );

  }

  public UploadImages(files: FileList, token: string): Observable<ServerResponse<UploadFilesResponse>> {
    let data = new FormData();
    for (let i = 0; i < files.length; ++i) {
      data.append('images', files[i]);
    }

    return this.MakeAuthorizedCall<UploadFilesResponse>(
      token,
      data,
      'Files/UploadImages'
    );
  }

  public UploadConversationThumbnail(thumbnail: File, conversationId: number, token: string): Observable<ServerResponse<UpdateThumbnailResponse>> {
    let data = new FormData();
    data.append('thumbnail', thumbnail);
    data.append('conversationId', conversationId.toString());

    return this.MakeAuthorizedCall<UpdateThumbnailResponse>(
      token,
      data,
      'api/Conversations/UpdateThumbnail'
    );
  }

  public FindUsersByUsername(token: string, username: string): Observable<ServerResponse<FoundUsersResponse>> {
    return this.MakeAuthorizedCall<FoundUsersResponse>(
      token,
      { UsernameToFind: username },
      'api/Users/FindByNickname'
    );
  }

  public CreateConversation(
    name: string,
    whoCreatedId: string,
    dialogUserId: string,
    thumbnailUrl: string,
    isGroup: boolean,
    token: string,
    isPublic: boolean)
  : Observable<ServerResponse<ConversationTemplate>>
  {
    return this.MakeAuthorizedCall<ConversationTemplate>(
      token,
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

  public ChangeConversationName(newName: string, conversationId: number, token: string): Observable<ServerResponse<boolean>> {

    return this.MakeAuthorizedCall<boolean>(
      token,
      { ConversationId: conversationId, Name: newName },
      'api/Conversations/ChangeName'
    );

  }

  public RefreshJwtToken(oldToken: string, userId: string): Observable<ServerResponse<string>>{
    return this.MakeNonAuthorizedCall<string>(
      { OldToken: oldToken, UserId: userId },
      'api/Tokens/Refresh'
    );
  }

  public SearchForGroups(token: string, searchstring: string): Observable<ServerResponse<Array<ConversationTemplate>>>{

    return this.MakeAuthorizedCall<Array<ConversationTemplate>>(
      token,
      { SearchString: searchstring },
      'api/Conversations/SearchGroups'
    );

  }

  public IsConversationsBanned(token: string, conversationids: Array<number>): Observable<ServerResponse<Array<boolean>>> {

    return this.MakeAuthorizedCall<Array<boolean>>(
      token,
      { conversationIds: conversationids },
      'api/Conversations/isBannedFrom'
    );

  }

  public IsUserBlocked(token: string, userId: string, byWhom: string): Observable<ServerResponse<boolean>> {

    return this.MakeAuthorizedCall<boolean>(
      token,
      { userId: userId, byWhom: byWhom },
      'api/Users/isBanned'
    );

  }

  public UnbanUser(token: string, userId: string): Observable<ServerResponse<boolean>> {

    return this.MakeAuthorizedCall<boolean>(
      token,
      { userId: userId },
      'api/Users/Unban'
    );

  }

  public BanUser(token: string, userId: string): Observable<ServerResponse<boolean>> {
    return this.MakeAuthorizedCall<boolean>(
      token,
      { userId: userId },
      'api/Users/Block'
    );
  }

  public BanFromConversation(token: string, userId: string, conversationId: number): Observable<ServerResponse<boolean>> {
    return this.MakeAuthorizedCall<boolean>(
      token,
      { userId: userId, conversationId: conversationId },
      'api/Conversations/BanFrom'
    );
  }

  private MakeAuthorizedCall<T>(token: string, data: any, url: string): Observable<ServerResponse<T>> {
    let headers = new HttpHeaders();
    headers = headers.append('Authorization', 'Bearer ' + token);

    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data,
      { headers: headers });
  }

  private MakeNonAuthorizedCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data);
  }

}
