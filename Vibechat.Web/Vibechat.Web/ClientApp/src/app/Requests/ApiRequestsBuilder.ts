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

@Injectable({
  providedIn: 'root'
})
export class ApiRequestsBuilder {

  private baseUrl: string;

  private httpClient: HttpClient;

  constructor(httpClient: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.httpClient = httpClient;
  }

  public LoginRequest(credentials: LoginRequest): Observable<ServerResponse<LoginResponse>>{
    return this.httpClient.post<ServerResponse<LoginResponse>>(this.baseUrl + "api/login", credentials);
  }

  public RegisterRequest(credentials: RegisterRequest): Observable<ServerResponse<string>> {
    return this.httpClient.post<ServerResponse<string>>(this.baseUrl + "api/register", credentials);
  }

  public UpdateConversationsRequest(token: string, userId: string): Observable<ServerResponse<ConversationResponse>> {

    let headers = new HttpHeaders();
    headers = headers.append('Authorization', 'Bearer ' + token);

    return this.httpClient.post<ServerResponse<ConversationResponse>>(
      this.baseUrl + 'api/Conversations/GetConversationInfo',
      new ConversationsRequest(
        {
          UserId: userId
        }),
      { headers: headers });
  }

  public GetConversationMessages(offset: number, count: number, conversationId: number, token: string): Observable<ServerResponse<ConversationMessagesResponse>> {
    let headers = new HttpHeaders();
    headers = headers.append('Authorization', 'Bearer ' + token);

    return this.httpClient.post<ServerResponse<ConversationMessagesResponse>>(
      this.baseUrl + 'api/Conversations/GetConversationMessages',
      new ConversationMessagesRequest(
        {
          Count: count,
          ConversationID: conversationId,
          MesssagesOffset: offset
        }),
      { headers: headers });
  }

  public DeleteMessages(messages: Array<ChatMessage>, conversationId: number, token: string) : Observable<ServerResponse<string>> {
    let headers = new HttpHeaders();
    headers = headers.append('Authorization', 'Bearer ' + token);

    return this.httpClient.post<ServerResponse<string>>(
      this.baseUrl + 'api/Conversations/DeleteMessages',
      new DeleteMessagesRequest(
        {
          MessagesId: messages.map(x => x.id),
          ConversationId: conversationId
        }),
      { headers: headers });
  }

  public UploadImages(files: FileList, token: string): Observable<ServerResponse<UploadFilesResponse>> {
    let headers = new HttpHeaders();
    headers = headers.append('Authorization', 'Bearer ' + token);

    let data = new FormData();

    for (let i = 0; i < files.length; ++i) {
      data.append('images', files[i]);
    }

    return this.httpClient.post<ServerResponse<UploadFilesResponse>>("Files/UploadImages",
      data,
      { headers: headers });
  }

  public CreateConversation(
    name: string,
    whoCreatedId: string,
    dialogUserId: string,
    thumbnailUrl: string,
    isGroup: boolean,
    token: string)
  : Observable<ServerResponse<ConversationTemplate>>
  {
    let headers = new HttpHeaders();
    headers = headers.append('Authorization', 'Bearer ' + token);

    return this.httpClient.post<ServerResponse<ConversationTemplate>>(
      this.baseUrl + 'api/Conversations/CreateConversation',
      { ConversationName: name, CreatorId: whoCreatedId, DialogUserId: dialogUserId, ImageUrl: thumbnailUrl, IsGroup: isGroup },
      { headers: headers });
  }
}
