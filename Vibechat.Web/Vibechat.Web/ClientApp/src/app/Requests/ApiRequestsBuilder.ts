import {from, Observable, of} from "rxjs";
import {ServerResponse} from "../ApiModels/ServerResponse";
import {LoginResponse} from "../ApiModels/LoginResponse";
import {LoginRequest} from "../ApiModels/LoginRequest";
import {HttpClient, HttpHeaders, HttpResponse} from "@angular/common/http";
import {ChangeUserInfoRequest} from "../ApiModels/RegisterRequest";
import {Inject, Injectable} from "@angular/core";
import {Message} from "../Data/Message";
import {Chat} from "../Data/Chat";
import {FoundUsersResponse} from "../Data/FoundUsersResponse";
import {UpdateThumbnailResponse} from "../ApiModels/UpdateThumbnailResponse";
import {AppUser} from "../Data/AppUser";
import {UploaderService} from "../uploads/upload.service";
import {SnackBarHelper} from "../Snackbar/SnackbarHelper";
import {Attachment} from "../Data/Attachment";
import {AttachmentKind} from "../Data/AttachmentKinds";
import {ChatState} from "./ChatState";
import {switchMap} from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
export class ApiRequestsBuilder {

  private baseUrl: string;

  private httpClient: HttpClient;

  private uploader: UploaderService;

  private logger: SnackBarHelper;

  constructor(httpClient: HttpClient, @Inject('BASE_URL') baseUrl: string, uploader: UploaderService, logger: SnackBarHelper) {
    this.baseUrl = baseUrl;
    this.httpClient = httpClient;
    this.uploader = uploader;
    this.logger = logger;
  }

  public LoginRequest(credentials: LoginRequest): Promise<ServerResponse<LoginResponse>>{
    return this.MakeUnauthorizedCall<LoginResponse>(
      credentials,
      'api/v1/Login'
    ).toPromise();
  }

  public ChangeUserInfo(credentials: ChangeUserInfoRequest): Promise<ServerResponse<boolean>> {
    return this.MakePostCall<boolean>(
      credentials,
      'api/v1/Users/ChangeInfo'
    ).toPromise();
  }

  public GetChats(deviceId: string, localState: Array<ChatState> = null): Promise<ServerResponse<Array<Chat>>> {

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
          History: history
        },
      'api/v1/Messages/Get'
    ).toPromise();
  }

  public SetLastMessageId(msgId: number, chatId: number){
    return this.MakePostCall<boolean>(
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
    return this.MakePostCall<boolean>(
      { newName: newName },
      'api/v1/Users/ChangeName'
    ).toPromise();
  }

  public ChangeCurrentUserLastName(newName: string): Promise<ServerResponse<boolean>> {
    return this.MakePostCall<boolean>(
      { newName: newName },
      'api/v1/Users/ChangeLastName'
    ).toPromise();
  }

  public ChangeUsername(newName: string): Promise<ServerResponse<boolean>> {
    return this.MakePostCall<boolean>(
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
      { UsernameToFind: username, ChatId: chatId },
      'api/v1/Chats/FindUsersInChat'
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
    whoCreatedId: string,
    dialogUserId: string,
    thumbnailUrl: string,
    isGroup: boolean,
    isPublic: boolean)
  : Promise<ServerResponse<Chat>>
  {
    return this.MakePostCall<Chat>(
      {
        ConversationName: name,
        CreatorId: whoCreatedId,
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
      { userId: userId },
      'api/v1/Users/AddToContacts'
    ).toPromise();
  }

  public RemoveFromContacts(userId: string): Promise<ServerResponse<boolean>> {
    return this.MakePostCall<boolean>(
      { userId: userId },
      'api/v1/Users/RemoveFromContacts'
    ).toPromise();
  }

  public GetContacts(): Promise<ServerResponse<Array<AppUser>>> {
    return this.MakePostCall<Array<AppUser>>(
      null,
      'api/v1/Users/GetContacts'
    ).toPromise();
  }

  public UpdateAuthKeyId(authKeyId: string, chatId: number, deviceId: string) {
    return this.MakePostCall<boolean>(
      { chatId: chatId, AuthKeyId: authKeyId, deviceId: deviceId },
      'api/v1/Chats/UpdateAuthKey'
    ).toPromise();
  }

  public ChangeConversationName(newName: string, conversationId: number): Promise<ServerResponse<boolean>> {

    return this.MakePostCall<boolean>(
      { ConversationId: conversationId, Name: newName },
      'api/v1/Chats/ChangeName'
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

  public SearchForGroups(searchstring: string): Promise<ServerResponse<Array<Chat>>>{

    return this.MakePostCall<Array<Chat>>(
      { SearchString: searchstring },
      'api/v1/Chats/Search'
    ).toPromise();

  }

  public UnbanUser(userId: string): Promise<ServerResponse<boolean>> {

    return this.MakePostCall<boolean>(
      { userId: userId },
      'api/v1/Users/Unban'
    ).toPromise();

  }

  public BanUser(userId: string): Promise<ServerResponse<boolean>> {
    return this.MakePostCall<boolean>(
      { userId: userId},
      'api/v1/Users/Block'
    ).toPromise();
  }

  public BanFromConversation(userId: string, conversationId: number): Promise<ServerResponse<boolean>> {
    return this.MakePostCall<boolean>(
      { userId: userId, conversationId: conversationId },
      'api/v1/Chats/BanFrom'
    ).toPromise();
  }

  public UnBanFromConversation(userId: string, conversationId: number): Promise<ServerResponse<boolean>> {
    return this.MakePostCall<boolean>(
      { userId: userId, conversationId: conversationId },
      'api/v1/Chats/UnbanFrom'
    ).toPromise();
  }

  private MakePostCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data);
  }

  private MakeGetCall<T>(url: string): Observable<ServerResponse<T>> {
    return this.httpClient.get<ServerResponse<T>>(
      this.baseUrl + url);
  }

  private MakeDeleteCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    return from(this.httpClient.delete<ServerResponse<T>>(
      this.baseUrl + url,
      data)).pipe(
        switchMap(
          response => {
            if(response instanceof HttpResponse){
              return of(response.body);
            }
          }
        )
    );
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
