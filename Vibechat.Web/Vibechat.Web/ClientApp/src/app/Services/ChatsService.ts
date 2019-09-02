import {Chat} from "../Data/Chat";
import {MessageReceivedModel} from "../Shared/MessageReceivedModel";
import {AuthService} from "./AuthService";
import {AddedToGroupModel} from "../Shared/AddedToGroupModel";
import {Message} from "../Data/Message";
import {Api} from "./Api/api.service";
import {BanEvent, SignalrConnection} from "../Connections/signalr-connection.service";
import {RemovedFromGroupModel} from "../Shared/RemovedFromGroupModel";
import {MessageState} from "../Shared/MessageState";
import {AppUser} from "../Data/AppUser";
import {Attachment} from "../Data/Attachment";
import {Injectable} from "@angular/core";
import {SecureChatsService} from "../Encryption/SecureChatsService";
import {E2EencryptionService} from "../Encryption/E2EencryptionService";
import {DHServerKeyExchangeService} from "../Encryption/DHServerKeyExchange";
import {MessageReportingService} from "./MessageReportingService";
import {ImageScalingService} from "./ImageScalingService";
import {DeviceService} from "./DeviceService";
import {ChatRole} from "../Roles/ChatRole";
import {AttachmentKind} from "../Data/AttachmentKinds";
import {ChatRoleDto} from "../Roles/ChatRoleDto";
import {AsyncArray} from "../Shared/AsyncArray";
import {MessageType} from "../Data/MessageType";
import {BehaviorSubject} from "rxjs";
import {UploadsApi} from "./Api/UploadsApi";

@Injectable({
  providedIn: 'root'
})
export class ChatsService {
  constructor(
    private authService: AuthService,
    private api: Api,
    private connectionManager: SignalrConnection,
    private secureChatsService: SecureChatsService,
    private encryptionService: E2EencryptionService,
    private messagesService: MessageReportingService,
    private dh: DHServerKeyExchangeService,
    private images: ImageScalingService,
    private device: DeviceService,
    private uploads: UploadsApi)
  {
    this.connectionManager.setChatsService(this);
    this.PendingReadMessages = new Array<number>();
    this.dh.setChatService(this);
  }

  public Chats: Array<Chat>;
  public _currentChat = new BehaviorSubject<Chat>(null);
  public CurrentChat: Chat;
  private PendingReadMessages: Array<number>;
  private didDisconnect: boolean;
  public static MinGroupNameLength = 5;
  public static MaxGroupNameLength = 128;
  public isUpdatingCurrentChat: boolean = false;

  public IsConversationSelected(): boolean {
    return this.CurrentChat != null;
  }

  public OnDisconnected(){
    this.didDisconnect = true;
  }

  //method updates local state of an app after disconnect.
  public async OnConnected(){
    if(!this.didDisconnect){
      return;
    }

    let response = await this.api.GetChats(this.device.GetDeviceId());

    if (!response.isSuccessfull) {
      return;
    }

    if(!response.response){
      this.Chats = new Array<Chat>();
      this.CurrentChat = null;
      return;
    }

    //sort to assign messages by index, find should work too.
    response.response.sort(ChatsService.ChatsSortByIdFunc);

    //remove chats where we've been kicked from
    this.Chats = this.Chats.filter(chat => {
      response.response.find(x => x.id == chat.id)
    }).sort(ChatsService.ChatsSortByIdFunc);


    for(let i = 0; i < this.Chats.length - 1; ++i){
      response.response[i].messages = this.Chats[i].messages;
    }

    response.response = response.response.sort(ChatsService.ChatsSortByLastMessageFunc);

    response.response.forEach(chat => {
      if(chat.isSecure){
       let decrypted = this.DecryptMessage(chat, chat.lastMessage);
       if(decrypted){
         chat.lastMessage = decrypted;
       }
      }
    });

    this.Chats = [...response.response];

    if(this.CurrentChat){
      await this.ChangeChat(this.Chats.find(x => x.id == this.CurrentChat.id));
    }
  }

  public IsAnyUnreadMessagesInCurrentChat(){
    if(!this.CurrentChat || !this.CurrentChat.messages){
      return false;
    }

    return this.CurrentChat.messages.find(msg =>
      (msg.state == MessageState.Delivered) && (this.authService.User.id != msg.user.id)) != null;
  }

  public GetLastUnreadMessageIndexInCurrentChat(){
    if(!this.CurrentChat || !this.CurrentChat.messages){
      return 0;
    }

    //array is sorted by ids ascending.
    for(let i = this.CurrentChat.messages.length - 1; i > 0; --i){
      if(this.CurrentChat.messages[i].state == MessageState.Delivered
      && this.CurrentChat.messages[i].user.id != this.authService.User.id){
        return i;
      }
    }

    return 0;
  }

  public IsCurrentChatDialog(){
    return !this.CurrentChat.isGroup;
  }

  public IsUptoDate(){
    if(!this.CurrentChat){
      return false;
    }

    if(!this.CurrentChat.lastMessage){
      return true;
    }

    return this.CurrentChat.lastMessage.id == this.CurrentChat.clientLastMessageId;
  }

  public GetConversationsIds() {
    return this.Chats.map(x => x.id);
  }

  /**
   * "reads" messages -
      decrements unread count and updates lastMessageId if necessary.
   * @constructor
   */
  public async ReadExistingMessagesInGroup(){
    if(!this.CurrentChat.isGroup){
      return;
    }
    //no messages to read, return.
    if(!this.CurrentChat.messagesUnread
      || this.CurrentChat.clientLastMessageId == this.CurrentChat.lastMessage.id){
      return;
    }

    let messagesToRead = this.GetUnreadLocalMessagesCount();

    this.CurrentChat.messagesUnread = Math.max(0, this.CurrentChat.messagesUnread - messagesToRead);

    let index = this.CurrentChat.messages.length - 1;
    //find biggest id excluding unsent messages.
    while(!this.CurrentChat.messages[index].id){
      --index;
    }

    let lastMsgId = this.CurrentChat.messages[index].id;

    if(this.CurrentChat.clientLastMessageId != lastMsgId){
      let response = await this.api.SetLastMessageId(lastMsgId, this.CurrentChat.id);

      if(response.isSuccessfull){
        this.CurrentChat.clientLastMessageId = lastMsgId;
      }
    }

  }

  public GetUnreadLocalMessagesCount() : number{
    let initialIndex = this.CurrentChat.messages.length - 1;
    let index ;

    for(index = initialIndex;
        (this.CurrentChat.messages[index].id > this.CurrentChat.clientLastMessageId) && index > 0; --index) { }
    return initialIndex - index;
  }

  public async ChangeChat(chat: Chat, updateLastMessageId: boolean = true) {
    try {
      if (chat == this.CurrentChat) {
        this.CurrentChat = null;
        return;
      }

      if (!chat) {
        this.CurrentChat = chat;
        return;
      }

      //secure dialog might not even exist on second user side, so update will fail
      if (!(chat.isSecure && !chat.authKeyId) && updateLastMessageId) {
        //update is needed.
        let initialLastMsgId = chat.clientLastMessageId;

        this.isUpdatingCurrentChat = true;

        this.CurrentChat = chat;

        await this.UpdateExisting(chat, true);

        if(chat.clientLastMessageId != initialLastMsgId){
          let newLastMsgExists = chat.messages.find(msg => msg.id == chat.clientLastMessageId);

          if(!newLastMsgExists){
            chat.messages = null;
          }else{
            chat.clientLastMessageId = initialLastMsgId;
          }
        }

        this.isUpdatingCurrentChat = false;
      }

      if(!this.CurrentChat || this.CurrentChat.id != chat.id){
        this.CurrentChat = chat;
      }

      //creator should wait until other user is online, and initiate key exchange.
      if (chat.isSecure && !chat.authKeyId && chat.chatRole.role == ChatRole.Creator) {

        if (!chat.dialogueUser.isOnline) {

          if (!this.connectionManager.SubscribeToUserOnlineStatusChanges(chat.dialogueUser.id)) {
            this.messagesService.OnFailedToSubsribeToUserStatusChanges();
          } else {
            this.messagesService.OnWaitingForUserToComeOnline()
          }

        } else {

          this.dh.InitiateKeyExchange(chat);

        }

      }
    }
    finally {
      this._currentChat.next(this.CurrentChat);
    }
  }

  //someone blocked this user.
  public OnBlocked(userId: string, banType: BanEvent) {
    let dialog = this.FindDialogWithById(userId);

    if (!dialog) {
      return;
    }

    switch (banType) {
      case BanEvent.Banned: {
        dialog.isMessagingRestricted = true;
        dialog.dialogueUser.isMessagingRestricted = true;
      }
      break;
      case BanEvent.Unbanned: {
        dialog.isMessagingRestricted = false;
        dialog.dialogueUser.isMessagingRestricted = false;
      }
    }
  }

  public OnUserRoleChanged(chatId: number, userId: string, newRole: ChatRole) {
    let chat = this.Chats.find(x => x.id == chatId);

    if (!chat) {
      return;
    }

    if (userId == this.authService.User.id) {
      chat.chatRole.role = newRole;
      return;
    }

    let user = chat.participants.find(x => x.id == userId);

    if (!user) {
      return;
    }

    user.chatRole.role = newRole;
  }

  public async MakeUserModerator(chatId: number, userId: string) {
    return this.ChangeUserChatRole(chatId, userId, ChatRole.Moderator);
  }

  public async RemoveModerator(chatId: number, userId: string) {
    return this.ChangeUserChatRole(chatId, userId, ChatRole.NoRole);
  }

  private async ChangeUserChatRole(chatId: number, userId: string, newRole: ChatRole) {
    return this.connectionManager.ChangeUserRole(chatId, userId, newRole);
  }

  public OnBannedInChat(chatId: number, userId: string, banType: BanEvent) {
    let chat = this.Chats.find(x => x.id == chatId);

    if (!chat) {
      return;
    }

    switch (banType) {
      case BanEvent.Banned: {
        if (userId == this.authService.User.id) {

          chat.isMessagingRestricted = true;
        } else {
          //moderator banned someone, not us
          let bannedUser = chat.participants.find(x => x.id == userId);
          bannedUser.isBlockedInConversation = true;
        }
      }
      break;
      case BanEvent.Unbanned: {
        if (userId == this.authService.User.id) {

          chat.isMessagingRestricted = false;
        } else {
          //moderator banned someone, not us
          let bannedUser = chat.participants.find(x => x.id == userId);
          bannedUser.isBlockedInConversation = false;
        }
      }
    }
  }

  public SetTyping(chatId: number) {
    this.connectionManager.SendTyping(chatId);
  }

  public OnLogOut() {
    this.Chats = new Array<Chat>();
    this.CurrentChat = null;
    this.PendingReadMessages = new Array<number>();
  }

  public OnUserOnline(user: string) {
    let dialog = this.FindDialogWithById(user);

    if (!dialog) {
      return;
    }

    this.dh.InitiateKeyExchange(dialog);
    this.connectionManager.UnsubscribeFromUserOnlineStatusChanges(user);
  }

  public async FindGroupsByName(name: string) {
    let result = await this.api.SearchForGroups(name);

    if (!result.isSuccessfull) {
      return null;
    }

    if (result.response == null) {
      return new Array<Chat>();

    } else {
      let resultArr = new Array<Chat>();

      for (let i = 0; i < result.response.length; ++i) {
        //we'll send only global(non-local groups as a result.)

        if (!this.Chats.find(x => x.id == result.response[i].id)) {
          resultArr.push(result.response[i]);
        }
      }
      return [...resultArr];
    }
  }

  public CreateSecureChat(user: AppUser) : boolean {
    let dialog = this.FindDialogWithSecurityCheck(user, true);

    if (dialog) {
      return false;
    }

    this.CreateDialogWith(user, true);
    return true;
  }

  public ResendMessages(messages: Array<Message>, chat: Chat) {
    messages.forEach(async (msg) => {
      await this.SendChatMessage(msg, chat);
    });

  }

  /**
   * To calculate the offset for messages,
   * considers chat.clientLastMessageId or customLastMessageId
   */
  public async UpdateMessagesHistory(count: number, chat: Chat, customLastMessageId: number = -1) {
    let lastMessageId = customLastMessageId == -1 ? chat.clientLastMessageId : customLastMessageId;

    let offset = chat.messages.filter(msg => msg.id < lastMessageId).length;

    let result = await this.api.GetChatMessages(
      offset,
      count,
      chat.id,
      lastMessageId,
      true);

    if (!result.isSuccessfull) {
      return;
    }

    //server sent zero messages, we reached end of our history.

    if (result.response == null || result.response.length == 0) {
      return result.response;
    }

    if (chat.isSecure) {
      let decryptedMessages = [];

      result.response.forEach(x => {

        let decrypted = this.DecryptMessage(chat, x);

        if(decrypted){
          decryptedMessages.push(decrypted);
        }
      });

      result.response = decryptedMessages;
    }

    result.response = result.response.sort(ChatsService.MessagesSortAscFunc);

    //apply scaling to images
    this.images.ScaleImages(result.response);

    //append old messages to new ones.
    chat.messages = [...result.response.concat(chat.messages)];

    chat.messages.forEach(x => {
      if(x.type == MessageType.Event){
        console.log(`event: ${JSON.stringify(x)}`);
      }
    });

    return result.response;
  }

  public async UpdateRecentMessages(count: number, chat: Chat, customLastMessageId: number = -1){
    let lastMessageId = customLastMessageId == -1 ? chat.clientLastMessageId : customLastMessageId;

    let offset = chat.messages.filter(msg => msg.id >= lastMessageId).length;

    let result = await this.api.GetChatMessages(
      offset,
      count,
      chat.id,
      lastMessageId,
      false);

    if (!result.isSuccessfull) {
      return;
    }

    //server sent zero messages, we reached end of our history.

    if (!result.response || !result.response.length) {
      return result.response;
    }

    if (chat.isSecure) {
      let decryptedMessages = [];

      result.response.forEach(x => {

        let decrypted = this.DecryptMessage(chat, x);

        if(decrypted){
          decryptedMessages.push(decrypted);
        }
      });

      result.response = decryptedMessages;
    }

    result.response = result.response.sort(ChatsService.MessagesSortAscFunc);

    //apply scaling to images
    this.images.ScaleImages(result.response);

    //append old messages to new ones.
    chat.messages.push(...result.response);

    //for the case of groups, client lastMessageId is updated automatically,
    //for dialogs, they are updated as we read messages.
    if(chat.isGroup){
      chat.messagesUnread = Math.max(0, chat.messagesUnread - result.response.length);

      chat.clientLastMessageId = Math.max(...result.response.map(msg => msg.id));
    }

    chat.messages = [...chat.messages];

    await this.ReadExistingMessagesInGroup();

    return result.response;
  }

  private static DecryptedMessageToLocalMessage(container: Message, decrypted: Message) {
    decrypted.id = container.id;
    decrypted.state = container.state;
    decrypted.timeReceived = container.timeReceived;
    decrypted.user = container.user;
  }

  public  static MessagesSortAscFunc(left: Message, right: Message): number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }

  public static MessagesSortDescFunc(left: Message, right: Message): number {
    if (left.timeReceived < right.timeReceived) return 1;
    if (left.timeReceived > right.timeReceived) return -1;
    return 0;
  }

  public  static ChatsSortByIdFunc(left: Chat, right: Chat): number {
    if (left.id < right.id) return -1;
    if (left.id > right.id) return 1;
    return 0;
  }

  public  static ChatsSortByLastMessageFunc(x: Chat, y: Chat): number {

    if(!x.lastMessage){
      if(!y.lastMessage){
        return 0;
      }else{
        return 1;
      }
    }else{
      if(!y.lastMessage){
        return 0;
      }else{
        if (x.lastMessage.id == y.lastMessage.id)
        {
          return 1;
        }

        return x.lastMessage.id > y.lastMessage.id ? 0 : 1;
      }
    }
  }

  public async DeleteMessages(messages: Array<Message>, from: Chat) {
    let notLocalMessages = messages.filter(x => x.state != MessageState.Pending);

    //delete local unsent messages
    from.messages = from.messages
      .filter(msg => messages.findIndex(x => x.id == msg.id && x.state == MessageState.Pending));

    if (!notLocalMessages.length) {
      return;
    }

    let response = await this.api.DeleteMessages(notLocalMessages, from.id);

    if (!response.isSuccessfull) {
      return;
    }

    //delete messages locally

    from.messages = from
      .messages
      .filter(msg => messages.findIndex(selected => selected.id == msg.id) == -1);
  }

  public async UpdateChats() {
    let response = await this.api.GetChats(this.device.GetDeviceId());

    if (!response.isSuccessfull) {
      return;
    }

    if (!response.response) {
      this.Chats = new Array<Chat>();
      await this.connectionManager.Start();
      return;
    }

    response.response
      .forEach((conversation) => {

        if (conversation.messages != null) {
          this.images.ScaleImages(conversation.messages);
        } else {
          conversation.messages = new Array<Message>();
        }

      });

    let toDeleteIndexes = [];

    await AsyncArray.asyncForEach(response.response, (async (chat, index) => {
      if (chat.isSecure) {

        //deviceId is not set, fix it.

        if (chat.authKeyId && this.secureChatsService.AuthKeyExists(chat.authKeyId) && !chat.deviceId) {
          let result = await this.api.UpdateAuthKeyId(chat.authKeyId, chat.id, this.device.GetDeviceId());

          if(!result.isSuccessfull){
            toDeleteIndexes.push(index);
            return;
          }

          chat.deviceId = this.device.GetDeviceId();
        }

        //delete secure chats where we've lost auth keys
        if (this.device.GetDeviceId() == chat.deviceId && chat.authKeyId && !this.secureChatsService.AuthKeyExists(chat.authKeyId)) {

          toDeleteIndexes.push(index);

        } else {

          let decryptedMessages = [];
          //decrypt chat messages
          chat.messages.forEach(msg => {

            let decrypted = this.DecryptMessage(chat, msg);

            if(decrypted){
              decryptedMessages.push(decrypted);
            }
          });

          chat.messages = decryptedMessages;

          //decrypt last message

          let decrypted = this.DecryptMessage(chat, chat.lastMessage);

          if(decrypted){
            chat.lastMessage = decrypted;
          }
        }
      } else {

        if (!chat.isGroup) {
          chat.isMessagingRestricted = chat.dialogueUser.isMessagingRestricted;
        }
      }
    }));

    let chatsToDelete = Array<Chat>();

    toDeleteIndexes.forEach(x => {
      chatsToDelete.push(response.response.splice(x, 1)[0]);
    });

    this.Chats = response.response;

    //Initiate signalR group connections

    await this.connectionManager.Start();

    chatsToDelete.forEach(x => {
      this.RemoveGroup(x);
    });
  }

  public DecryptMessage(chat: Chat, msg: Message) : Message{
    if(!msg){
      return null;
    }

    let decrypted = this.encryptionService.Decrypt(chat.dialogueUser.id, msg.encryptedPayload);

    if (!decrypted) {
      this.OnAuthKeyLost(chat);
      return null;
    }

    ChatsService.DecryptedMessageToLocalMessage(
      msg,
      decrypted);

    return decrypted;
  }

  public async OnMessageReceived(data: MessageReceivedModel) {
    let chat = this.Chats
      .find(x => x.id == data.conversationId);

    if (!chat) {
      return;
    }

    if(!chat.messages){
      chat.messages = new Array<Message>();
    }

    if (data.secure) {
      let decrypted = this.encryptionService.Decrypt(data.senderId, data.message.encryptedPayload);

      if (!decrypted) {
        this.OnAuthKeyLost(chat);
        return;
      }

      ChatsService.DecryptedMessageToLocalMessage(data.message, decrypted);

      data.message = decrypted;
    }

    this.images.ScaleImage(data.message);

    //we can accept message only if all messages are loaded
    if(!chat.lastMessage || chat.messages.find(msg => msg.id == chat.lastMessage.id)){
      chat.messages.push(data.message);
      chat.messages = [...chat.messages];

      //if user was in different chat, increment unread counter.
      //if not, UI should automatically update clientLastMessageId.
      if(!this.CurrentChat || chat.id != this.CurrentChat.id){
        ++chat.messagesUnread;
      }

    } else{
      ++chat.messagesUnread;
    }

    chat.lastMessage = data.message;
  }

  public BuildForwardedMessage(whereTo: number, forwarded: Message): Message {
    return new Message(
      {
        id: 0,
        type: MessageType.Forwarded,
        user: this.authService.User,
        conversationID: whereTo,
        state: MessageState.Pending,
        timeReceived: new Date(),
        forwardedMessage: forwarded
      });
  }

  public BuildMessage(message: string, whereTo: number, isAttachment = false, AttachmentInfo: Attachment = null): Message {
    return new Message(
      {
        id: 0,
        messageContent: message,
        type: isAttachment ? MessageType.Attachment : MessageType.Text,
        attachmentInfo: AttachmentInfo,
        user: this.authService.User,
        conversationID: whereTo,
        state: MessageState.Pending,
        timeReceived: new Date()
      });
  }

  public async BanFromConversation(userToBan: AppUser, from: Chat) : Promise<boolean> {
    return await this.connectionManager.BlockUserInChat(userToBan.id, from.id, BanEvent.Banned);
  }

  public async UnbanFromConversation(userToUnban: AppUser, from: Chat): Promise<boolean> {
    return await this.connectionManager.BlockUserInChat(userToUnban.id, from.id, BanEvent.Unbanned);
  }

  public RemoveGroup(group: Chat) {
    this.connectionManager.RemoveConversation(group);
  }

  public async GetAttachmentsFor(groupId: number, attachmentKind: AttachmentKind, offset: number, count: number) {
    let result = await this.api.GetAttachmentsForConversation(
      groupId,
      attachmentKind,
      offset,
      count);

    if (!result.isSuccessfull) {
      return null;
    }

    if (result.response == null || result.response.length == 0) {
      return result.response;
    }

  }

  public OnAuthKeyLost(chat: Chat) {
    this.RemoveGroup(chat);
  }

  public IsSecureChatAndNoAuthKey(chat: Chat) {
    return chat.isSecure && !chat.authKeyId;
  }

  public async SendMessage(message: string, to: Chat) {
    let messageToSend = this.BuildMessage(message, to.id);

    if (this.IsSecureChatAndNoAuthKey(to)) {
      return;
    }

    await this.SendChatMessage(messageToSend, to);
  }

  public async FindUsersInChat(username: string, chatId: number) : Promise<Array<AppUser>> {
    let result = await this.api.FindUsersInChat(username, chatId);

    if (!result.isSuccessfull) {
      return null;
    }

    return result.response.usersFound;
  }

  public async ReadMessage(message: Message, chat: Chat) {

    if (message.user.id == this.authService.User.id) {
      return;
    }

    if (this.PendingReadMessages.find(x => x == message.id)) {
      return;
    }

    let pendingIndex = this.PendingReadMessages.push(message.id) - 1;

    let result = await this.connectionManager.ReadMessage(message.id, message.conversationID);

    if (!result) {
      this.PendingReadMessages.splice(pendingIndex, 1);
      return;
    }

    message.state = MessageState.Read;

    //clientLastMessageId is updated on server automatically, so do it locally.
    chat.clientLastMessageId = message.id;
    chat.messagesUnread = Math.max(0, chat.messagesUnread - 1);
    this.PendingReadMessages.splice(pendingIndex, 1);
  }

  public async CreateGroup(name: string, isPublic: boolean) {
    if(name.length < ChatsService.MinGroupNameLength || name.length > ChatsService.MaxGroupNameLength){
      return;
    }

    let result = await this.api.CreateConversation(
      name, null, null, true, isPublic);

    if (!result.isSuccessfull) {
      return;
    }

    this.connectionManager.InitiateConnections(new Array<number>(1).fill(result.response.id));

    result.response.messages = new Array<Message>();

    this.Chats = [...this.Chats, result.response];
  }

  public CreateDialogWith(user: AppUser, secure: boolean) {
    this.connectionManager.CreateDialog(user, secure);
  }

  public RemoveDialogWith(user: AppUser) {
    this.connectionManager.RemoveConversation(this.FindDialogWith(user));
  }

  public async JoinGroup(group: Chat) {
    let result = await this.connectionManager.AddUserToConversation(this.authService.User.id, group);

    if (!result) {
      return;
    }

    let chat = await this.api.GetConversationById(group.id, true);

    if (!chat.isSuccessfull) {
      return;
    }

    this.Chats = [...this.Chats, chat.response];
  }

  public KickUser(user: AppUser, from: Chat) {
    this.connectionManager.RemoveUserFromConversation(user.id, from.id, false);
  }

  public ExistsIn(id: number) {
    return this.Chats.find(x => x.id == id) != null;
  }

  public FindDialogWith(user: AppUser) {
    return this.Chats.find(x => !x.isGroup && x.dialogueUser.id == user.id);
  }

  public FindDialogWithById(userId: string) {
    return this.Chats.find(x => !x.isGroup && x.dialogueUser.id == userId);
  }

  public FindDialogWithSecurityCheck(user: AppUser, secure: boolean) {
    return this.Chats.find(x =>
        !x.isGroup
        && x.dialogueUser.id == user.id
        && x.isSecure == secure);
  }

  public async SearchMessages(searchFor: string, offset: number, count: number){
    let messages = await this.api.SearchMessages(this.device.GetDeviceId(), searchFor, offset, count);

    if(!messages.isSuccessfull){
      return null;
    }

    return messages.response;
  }

  /**
   * Returns chat by specified id.
   * @param id chat id
   * @param apiRequest make api call if no chat exists?
   * @constructor
   */
  public async GetByIdAsync(id: number, apiRequest: boolean){
    let local = this.Chats.find(chat => chat.id == id);

    if(local){
      return local;
    }

    if(apiRequest){
      let result = await this.api.GetConversationById(id, true);

      if(!result.isSuccessfull){
        return null;
      }

      return result.response;
    }else{
      return null;
    }
  }

  /**
   * Returns local chat by id.
   * @param id
   * @constructor
   */
  public GetById(id: number){
    let local = this.Chats.find(chat => chat.id == id);

    if(local){
      return local;
    }

    return null;
  }

  public async UpdateExisting(target: Chat, updateLastMsgId: boolean = false) : Promise<void> {
    let result = await this.api.GetConversationById(target.id, true);

    if (!result.isSuccessfull) {
      return;
    }

    this.UpdateConversationFields(target, result.response, updateLastMsgId);
  }

  public async ForwardMessagesTo(destination: Array<Chat>, messages: Array<Message>) {
    if (destination == null || destination.length == 0) {
      return;
    }

    //couldn't forward unsent messages.

    messages = messages.filter(msg => msg.id);

    if(!messages || !messages.length){
      this.messagesService.WrongMessagesToForward();
      return;
    }

    await AsyncArray.asyncForEach(destination,
      async (conversation) => {

        if (this.IsSecureChatAndNoAuthKey(conversation)) {
          return;
        }

        await AsyncArray.asyncForEach(messages,async msg => {
          let messageToSend = this.BuildForwardedMessage(conversation.id, msg.forwardedMessage ? msg.forwardedMessage : msg);
          await this.SendChatMessage(messageToSend, conversation);
        });
      }
    );
  }

  public async RemoveAllMessages(group: Chat) : Promise<void> {
    let response = await this.api.DeleteMessages(group.messages, group.id);

    if (!response.isSuccessfull) {
      return;
    }

    //delete messages locally

    if (group.messages.length != 0) {
      group.messages.splice(0, group.messages.length);
    }

    this.CurrentChat = null;
  }

  public Leave(from: Chat) {
    this.connectionManager.RemoveUserFromConversation(this.authService.User.id, from.id, true);
    this.CurrentChat = null;
  }

  public async ChangeThumbnail(file: File, where: Chat, progress: (value: number) => void): Promise<void> {
    let result = await this.uploads.UploadConversationThumbnail(file, progress, where.id);

    if (!result.isSuccessfull) {
      return;
    }

    where.thumbnailUrl = result.response.thumbnailUrl;
    where.fullImageUrl = result.response.fullImageUrl;
  }

  public async ChangeConversationName(name: string, where: Chat): Promise<void> {
    let result = await this.api.ChangeConversationName(name, where.id);

    if (!result.isSuccessfull) {
      return;
    }

    where.name = name;
  }

  public InviteUsersToGroup(users: Array<AppUser>, group: Chat) {
    if (users == null || users.length == 0) {
      return;
    }

    users.forEach(
      async (value) => {
        let result = await this.connectionManager.AddUserToConversation(value.id, group);

        if (result) {
          value.chatRole = new ChatRoleDto();
          value.chatRole.role = ChatRole.NoRole;
          group.participants.push(value);
        }
      }
    )
  }

  public async OnAddedToGroup(data: AddedToGroupModel): Promise<void> {

    let chat = await this.api.GetConversationById(data.chatId, true);

    if (!chat.isSuccessfull) {
      return;
    }

    //when current user was added to group

    if (data.user.id == this.authService.User.id) {
      this.Chats = [...this.Chats, chat.response];
      return;
    }

    //when someone added new user to group.

    chat.response.participants.push(data.user);
  }

  public OnRemovedFromGroup(data: RemovedFromGroupModel) {
    let chatIndex = this.Chats.findIndex(x => x.id == data.conversationId);

    if (chatIndex == -1) {
      return;
    }


    //either this client left or creator removed him.
    if (data.userId == this.authService.User.id) {

      if (this.CurrentChat && this.CurrentChat.id == data.conversationId) {
          this.CurrentChat = null;
      }

      this.Chats.splice(chatIndex, 1);

    } else {

      let participants = this.Chats[chatIndex].participants;

      participants.splice(participants.findIndex(x => x.id == data.userId), 1);
    }
  }

  public OnMessageRead(msgId: number, conversationId: number) {
    let conversation = this.Chats.find(x => x.id == conversationId);

    if (conversation == null || conversation.isGroup) {
      return;
    }

    let message = conversation.messages.find(x => x.id == msgId);

    if (message == null) {
      return;
    }

    if (message.state == MessageState.Read) {
      return;
    }

    message.state = MessageState.Read;
    conversation.messages = [...conversation.messages];
    conversation.messagesUnread = Math.max(0, conversation.messagesUnread - 1);
  }

  public OnMessageDelivered(msgId: number, clientMessageId: number, conversationId: number) {
    let conversation = this.Chats.find(x => x.id == conversationId);

    if (conversation == null) {
      return;
    }

    let message = conversation.messages.find(x => x.id == clientMessageId);

    if (message == null) {
      return;
    }

    message.id = msgId;
    message.state = MessageState.Delivered;
    conversation.messages = [...conversation.messages];
  }

  /**
   * Method is used for sending built message to specified chat.
   * Handles push and pop automatically.
   * @param message
   * @param to
   * @constructor
   */
  private async SendChatMessage(message: Message, to: Chat) : Promise<boolean> {
    let msgIndex = to.messages.push(message) - 1;
    to.messages = [...to.messages];
    let messageId = 0;

    if (to.isSecure) {

      let encrypted = this.encryptionService.Encrypt(to.dialogueUser.id, message);

      if (!encrypted) {
        this.OnAuthKeyLost(to);
        return false;
      }

      messageId = await this.connectionManager.SendMessageToSecureChat(
        encrypted,
        to.dialogueUser.id,
        to.id);

    } else {
      messageId = await this.connectionManager.SendMessage(message, to);
    }

    if(messageId){
      message.state = MessageState.Delivered;
      message.id = messageId;
      to.clientLastMessageId = messageId;
      to.lastMessage = message;

      return true;
    } else{
      to.messages.splice(msgIndex, 1);
      to.messages = [...to.messages];
      return false;
    }
  }

  public async UploadFile(file: File, progress: (value: number) => void, to: Chat) : Promise<boolean> {

    let response = await this.uploads.UploadFile(file, progress, to.id);

    if (!response.isSuccessfull) {
      return false;
    }

    let message = this.BuildMessage(null, to.id, true, response.response);

    let successfull = await this.SendChatMessage(message, to);

    if(successfull){
      this.images.ScaleImage(message);
    }

    return true;
  }

  public async UploadImages(files: FileList, progress: (value: number) => void, to: Chat) {
    if (files.length == 0) {
      return;
    }

    let conversationToSend = to.id;

    let response = await this.uploads.UploadImages(files, progress, to.id);

    if (!response.isSuccessfull) {
      this.messagesService.SomeFilesWereNotUploaded();
    }

    response.response.uploadedFiles.forEach(
      async (file) => {
        let message = this.BuildMessage(null, conversationToSend, true, file);

        let successfull = await this.SendChatMessage(message, to);

        if(successfull){
          this.images.ScaleImage(message);
        }
      })
  }

  public UpdateConversationFields(old: Chat, New: Chat, updateLastMsgId: boolean = false): void {
    old.name = New.name;
    old.fullImageUrl = New.fullImageUrl;
    old.thumbnailUrl = New.thumbnailUrl;
    old.participants = New.participants;
    old.isMessagingRestricted = New.isMessagingRestricted;
    if(updateLastMsgId){
      old.clientLastMessageId = New.clientLastMessageId;
    }
    old.dialogueUser = New.dialogueUser;
  }

  async ChangeChatPublicVisibility(chat: Chat) {
    let res = await this.api.ChangeChatPublicVisibility(chat.id);

    if(res.isSuccessfull){
      chat.isPublic = !chat.isPublic;
    }
  }
}
