import {Observable} from "rxjs";
import {ServerResponse} from "../ApiModels/ServerResponse";
import {LoginResponse} from "../ApiModels/LoginResponse";
import {LoginRequest} from "../ApiModels/LoginRequest";
import {HttpClient, HttpHeaders} from "@angular/common/http";
import {ChangeUserInfoRequest} from "../ApiModels/RegisterRequest";
import {Inject, Injectable} from "@angular/core";
import {ChatMessage} from "../Data/ChatMessage";
import {Chat} from "../Data/Chat";
import {FoundUsersResponse} from "../Data/FoundUsersResponse";
import {UpdateThumbnailResponse} from "../ApiModels/UpdateThumbnailResponse";
import {UserInfo} from "../Data/UserInfo";
import {UploaderService} from "../uploads/upload.service";
import {SnackBarHelper} from "../Snackbar/SnackbarHelper";
import {MessageAttachment} from "../Data/MessageAttachment";
import {AttachmentKind} from "../Data/AttachmentKinds";
import {ChatState} from "./ChatState";

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

  public LoginRequest(credentials: LoginRequest): Promise<ServerResponse<LoginResponse>>{
    return this.MakeUnauthorizedCall<LoginResponse>(
      credentials,
      'api/login'
    ).toPromise();
  }

  public ChangeUserInfo(credentials: ChangeUserInfoRequest): Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      credentials,
      'api/Users/ChangeInfo'
    ).toPromise();
  }

  public GetChats(deviceId: string, localState: Array<ChatState> = null): Promise<ServerResponse<Array<Chat>>> {

    return this.MakeCall<Array<Chat>>(
      { deviceId: deviceId, localState: localState},
      'api/Conversations/GetAll'
    ).toPromise();
  }

  public async DownloadFile(url: string) {
   let result = await this.httpClient.get(url, { responseType: 'arraybuffer'})
      .toPromise();

    return result;
  }

  public GetConversationMessages(offset: number, count: number, conversationId: number, maxMessageId: number): Promise<ServerResponse<Array<ChatMessage>>> {

    return this.MakeCall<Array<ChatMessage>>(
        {
          Count: count,
          ConversationID: conversationId,
          MessagesOffset: offset,
          MaxMessageId: maxMessageId
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

  public UploadImages(files: FileList, progress: (value: number) => void, chatId: number): Promise<ServerResponse<any>> {
    return this.uploader.uploadImagesToChat(files, progress, chatId.toString()).toPromise();
  }

  public UploadConversationThumbnail(thumbnail: File, progress: (value: number) => void, conversationId: number): Promise<ServerResponse<UpdateThumbnailResponse>> {
    return this.uploader.uploadChatPicture(thumbnail, progress, conversationId).toPromise();
  }

  public UploadUserProfilePicture(picture: File, progress: (value: number) => void): Promise<ServerResponse<UpdateThumbnailResponse>> {
    return this.uploader.uploadUserPicture(picture, progress).toPromise();
  }

  public UploadFile(file: File, progress: (value: number) => void, chatId: number) : Promise<ServerResponse<MessageAttachment>> {
    return this.uploader.uploadFile(file, progress, chatId.toString()).toPromise();
  }

  public GetUserById(userId: string): Promise<ServerResponse<UserInfo>> {
    return this.MakeCall<UserInfo>(
      { Id: userId },
      'api/Users/GetById'
    ).toPromise();
  }

  public GetConversationById(conversationId: number, updateRoles: boolean): Promise<ServerResponse<Chat>> {
    return this.MakeCall<Chat>(
      { conversationId: conversationId, updateRoles: updateRoles },
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

  public ChangeUsername(newName: string): Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      { newName: newName },
      'api/Users/ChangeUsername'
    ).toPromise();
  }

  public FindUsersByUsername(username: string): Promise<ServerResponse<FoundUsersResponse>> {
    return this.MakeCall<FoundUsersResponse>(
      { UsernameToFind: username },
      'api/Users/FindByNickname'
    ).toPromise();
  }

  public FindUsersInChat(username: string, chatId: number): Promise<ServerResponse<FoundUsersResponse>> {
    return this.MakeCall<FoundUsersResponse>(
      { UsernameToFind: username, ChatId: chatId },
      'api/Conversations/FindUsersInChat'
    ).toPromise();
  }

  public GetAttachmentsForConversation(conversationId: number, kind: AttachmentKind, offset: number, count: number) {
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
  : Promise<ServerResponse<Chat>>
  {
    return this.MakeCall<Chat>(
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

  public AddToContacts(userId: string) : Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      { userId: userId },
      'api/Users/AddToContacts'
    ).toPromise();
  }

  public RemoveFromContacts(userId: string): Promise<ServerResponse<boolean>> {
    return this.MakeCall<boolean>(
      { userId: userId },
      'api/Users/RemoveFromContacts'
    ).toPromise();
  }

  public GetContacts(): Promise<ServerResponse<Array<UserInfo>>> {
    return this.MakeCall<Array<UserInfo>>(
      null,
      'api/Users/GetContacts'
    ).toPromise();
  }

  public UpdateAuthKeyId(authKeyId: string, chatId: number, deviceId: string) {
    return this.MakeCall<boolean>(
      { chatId: chatId, AuthKeyId: authKeyId, deviceId: deviceId },
      'api/Conversations/UpdateAuthKey'
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

  public SearchForGroups(searchstring: string): Promise<ServerResponse<Array<Chat>>>{

    return this.MakeCall<Array<Chat>>(
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

  private MakeUnauthorizedCall<T>(data: any, url: string): Observable<ServerResponse<T>> {
    let headers = new HttpHeaders();
    headers = headers.append('unauthorized', '1');

    return this.httpClient.post<ServerResponse<T>>(
      this.baseUrl + url,
      data,
      { headers: headers });
  }
}
