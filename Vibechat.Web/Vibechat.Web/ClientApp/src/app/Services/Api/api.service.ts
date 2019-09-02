import {Observable} from "rxjs";
import {ServerResponse} from "../../ApiModels/ServerResponse";
import {LoginResponse} from "../../ApiModels/LoginResponse";
import {LoginRequest} from "../../ApiModels/LoginRequest";
import {HttpClient, HttpHeaders, HttpRequest} from "@angular/common/http";
import {ChangeUserInfoRequest} from "../../ApiModels/RegisterRequest";
import {Inject, Injectable} from "@angular/core";
import {Message} from "../../Data/Message";
import {Chat} from "../../Data/Chat";
import {FoundUsersResponse} from "../../Data/FoundUsersResponse";
import {AppUser} from "../../Data/AppUser";
import {AttachmentKind} from "../../Data/AttachmentKinds";

@Injectable({
  providedIn: 'root'
})
export class Api {


  constructor(private httpClient: HttpClient, @Inject('BASE_URL')
              private baseUrl: string) {
  }

  public LoginRequest(credentials: LoginRequest): Promise<ServerResponse<LoginResponse>>{
    return this.MakeUnauthorizedCall<LoginResponse>(
      credentials,
      'api/v1/Login'
    ).toPromise();
  }

  public ChangeUserInfo(credentials: ChangeUserInfoRequest): Promise<ServerResponse<boolean>> {
    return this.MakePutCall<boolean>(
      credentials,
      'api/v1/Users/ChangeInfo'
    ).toPromise();
  }

  public GetChats(deviceId: string): Promise<ServerResponse<Array<Chat>>> {

    return this.MakeGetCall<Array<Chat>>(
      'api/v1/Chats/GetAll/' + deviceId
    ).toPromise();
  }

  public async DownloadFile(url: string) {
   let result = await this.httpClient.get(url, { responseType: 'arraybuffer'})
      .toPromise();

    return result;
  }

  public GetChatMessages(offset: number, count: number, conversationId: number, maxMessageId: number,
                         history: boolean, setLastMessage: boolean = true): Promise<ServerResponse<Array<Message>>> {
    return this.MakePostCall<Array<Message>>(
        {
          Count: count,
          ConversationID: conversationId,
          MessagesOffset: offset,
          MaxMessageId: maxMessageId,
          SetLastMessage: setLastMessage,
          History: history,
        },
      'api/v1/Messages/Get'
    ).toPromise();
  }

  public SetLastMessageId(msgId: number, chatId: number){
    return this.MakePutCall<boolean>(
      { chatId: chatId, messageId: msgId },
      'api/v1/Messages/SetLast'
    ).toPromise();
  }

  public DeleteMessages(messages: Array<Message>, conversationId: number) : Promise<ServerResponse<string>> {

    return this.MakePostCall<string>(
        {
          MessagesId: messages.map(x => x.id),
          ConversationId: conversationId
        },
      'api/v1/Messages/Delete'
    ).toPromise();

  }

  public SearchMessages(deviceId: string, searchString: string, offset: number, count: number){
    return this.MakePostCall<Array<Message>>(
      {
        deviceId: deviceId,
        searchString: searchString,
        offset: offset,
        count: count
      },
      'api/v1/Messages/Search'
    ).toPromise();
  }

  public GetUserById(userId: string): Promise<ServerResponse<AppUser>> {
    return this.MakeGetCall<AppUser>(
      'api/v1/Users/' + userId
    ).toPromise();
  }

  public GetConversationById(conversationId: number, updateRoles: boolean): Promise<ServerResponse<Chat>> {
    return this.MakeGetCall<Chat>(
      'api/v1/Chats/' + conversationId.toString()
    ).toPromise();
  }

  public ChangeCurrentUserName(newName: string): Promise<ServerResponse<boolean>> {
    return this.MakePatchCall<boolean>(
      { newName: newName },
      'api/v1/Users/ChangeName'
    ).toPromise();
  }

  public ChangeCurrentUserLastName(newName: string): Promise<ServerResponse<boolean>> {
    return this.MakePatchCall<boolean>(
      { newName: newName },
      'api/v1/Users/ChangeLastName'
    ).toPromise();
  }

  public UploadImagesRequest(data: any){
    let headers = new HttpHeaders();
    headers = headers.append("ngsw-bypass", "");

    return new HttpRequest('POST', '/api/v1/Files/UploadImages', data, {
      reportProgress: true,
      headers: headers
    });
  }

  public UpdateChatThumbnail(chatId: number, data: any){
    let headers = new HttpHeaders();
    headers = headers.append("ngsw-bypass", "");

    return new HttpRequest('PATCH', `api/v1/Chats/${chatId}/UpdateThumbnail`, data, {
      reportProgress: true,
      headers: headers
    });
  }

  public UploadUserProfilePicture(data: any){
    let headers = new HttpHeaders();
    headers = headers.append("ngsw-bypass", "");

    return new HttpRequest('PATCH', 'api/v1/Users/UpdateProfilePicture', data, {
      reportProgress: true,
      headers: headers
    });
  }

  public UploadFile(data: any){
    let headers = new HttpHeaders();
    headers = headers.append("ngsw-bypass", "");

    return new HttpRequest('POST', '/api/v1/Files/UploadFile', data, {
      reportProgress: true,
      headers: headers
    });
  }

  public ChangeUsername(newName: string): Promise<ServerResponse<boolean>> {
    return this.MakePatchCall<boolean>(
      { newName: newName },
      'api/v1/Users/ChangeUsername'
    ).toPromise();
  }

  public FindUsersByUsername(username: string): Promise<ServerResponse<FoundUsersResponse>> {
    return this.MakePostCall<FoundUsersResponse>(
      { UsernameToFind: username },
      'api/v1/Users/FindByNickname'
    ).toPromise();
  }

  public FindUsersInChat(username: string, chatId: number): Promise<ServerResponse<FoundUsersResponse>> {
    return this.MakePostCall<FoundUsersResponse>(
      { UsernameToFind: username },
      `api/v1/Chats/${chatId}/FindUsers`
    ).toPromise();
  }

  public GetAttachmentsForConversation(conversationId: number, kind: AttachmentKind, offset: number, count: number) {
    return this.MakePostCall<Array<Message>>(
      {
        conversationId: conversationId,
        kind: kind,
        offset: offset,
        count: count
      },
      'api/v1/Messages/GetAttachments'
    ).toPromise();
  }

  public CreateConversation(
    name: string,
    dialogUserId: string,
    thumbnailUrl: string,
    isGroup: boolean,
    isPublic: boolean)
  : Promise<ServerResponse<Chat>>
  {
    return this.MakePostCall<Chat>(
      {
        ConversationName: name,
        DialogUserId: dialogUserId,
        ImageUrl: thumbnailUrl,
        IsGroup: isGroup,
        IsPublic: isPublic
      },
      'api/v1/Chats/Create'
    ).toPromise();

  }

  public AddToContacts(userId: string) : Promise<ServerResponse<boolean>> {
    return this.MakePostCall<boolean>(
      null,
      `api/v1/Contacts/${userId}/Add`
    ).toPromise();
  }

  public RemoveFromContacts(userId: string): Promise<ServerResponse<boolean>> {
    return this.MakeDeleteCall<boolean>(
      `api/v1/Contacts/${userId}/Remove`,
    ).toPromise();
  }

  public GetContacts(): Promise<ServerResponse<Array<AppUser>>> {
    return this.MakePostCall<Array<AppUser>>(
      null,
      'api/v1/Contacts/Get'
    ).toPromise();
  }

  public UpdateAuthKeyId(authKeyId: string, chatId: number, deviceId: string) {
    return this.MakePatchCall<boolean>(
      { AuthKeyId: authKeyId, deviceId: deviceId },
      `api/v1/Chats/${chatId}/UpdateAuthKey`
    ).toPromise();
  }

  public ChangeConversationName(newName: string, conversationId: number): Promise<ServerResponse<boolean>> {

    return this.MakePatchCall<boolean>(
      { Name: newName },
      `api/v1/Chats/${conversationId}/ChangeName`
    ).toPromise();

  }

  public RefreshJwtToken(refreshToken: string, userId: string): Promise<ServerResponse<string>>{
    let headers = new HttpHeaders();
    headers = headers.append('refreshtoken', '1');

    return this.httpClient.post<ServerResponse<string>>(
      this.baseUrl + 'api/v1/Tokens/Refresh',
      { RefreshToken: refreshToken, userId: userId },
      { headers: headers}).toPromise();
  }

  public SearchForGroups(searchstring: string): Promise<ServerResponse<Array<Chat>>>{

    return this.MakePostCall<Array<Chat>>(
      { SearchString: searchstring },
      'api/v1/Chats/Search'
    ).toPromise();

  }

  public ChangeChatPublicVisibility(chatId: number) {
    return this.MakePatchCall<boolean>(
      null,
      `api/v1/Chats/${chatId}/ChangePublicState`
    ).toPromise();
  }

  public ChangeUserPublicVisibility() {
    return this.MakePatchCall<boolean>(
      null,
      `api/v1/Users/ChangePublicState`
    ).toPromise();
  }

  private MakePostCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    console.log(`post api call : ${url}, data: ${JSON.stringify(data)}`);

    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data);
  }

  private MakeGetCall<T>(url: string): Observable<ServerResponse<T>> {
    console.log(`get api call : ${url}`);
    return this.httpClient.get<ServerResponse<T>>(
      this.baseUrl + url);
  }

  private MakePatchCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    console.log(`patch api call : ${url}, data: ${JSON.stringify(data)}`);

    return this.httpClient.patch<ServerResponse<T>>(
      this.baseUrl + url, data);
  }

  private MakePutCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    console.log(`put api call : ${url}, data: ${JSON.stringify(data)}`);

    return this.httpClient.put<ServerResponse<T>>(
      this.baseUrl + url, data);
  }

  private MakeDeleteCall<T>(url: string): Observable<ServerResponse<T>> {
    console.log(`delete api call : ${url}`);

    return this.httpClient.delete<ServerResponse<T>>(
      this.baseUrl + url);
  }

  private MakeUnauthorizedCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    let headers = new HttpHeaders();
    headers = headers.append('unauthorized', '1');

    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data,
      { headers: headers });
  }
}
