import {Chat} from "../Data/Chat";
import {MessageReceivedModel} from "../Shared/MessageReceivedModel";
import {AuthService} from "../Auth/AuthService";
import {AddedToGroupModel} from "../Shared/AddedToGroupModel";
import {Message} from "../Data/Message";
import {ApiRequestsBuilder} from "../Requests/ApiRequestsBuilder";
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

@Injectable({
  providedIn: 'root'
})
export class ChatsService {
  constructor(
    private authService: AuthService,
    private requestsBuilder: ApiRequestsBuilder,
    private connectionManager: SignalrConnection,
    private secureChatsService: SecureChatsService,
    private encryptionService: E2EencryptionService,
    private messagesService: MessageReportingService,
    private dh: DHServerKeyExchangeService,
    private images: ImageScalingService,
    private device: DeviceService)
  {
    this.connectionManager.setChatsService(this);
    this.PendingReadMessages = new Array<number>();
    this.dh.setChatService(this);
  }

  public Conversations: Array<Chat>;
  public CurrentConversation: Chat;
  private PendingReadMessages: Array<number>;

  public IsConversationSelected(): boolean {
    return this.CurrentConversation != null;
  }

  public GetConversationsIds() {
    return this.Conversations.map(x => x.id);
  }

  /**
   * "reads" messages -
      decrements unread count and updates lastMessageId if necessary.
   * @constructor
   */
  public async ReadExistingMessagesInGroup(maximumToRead: number){
    if(!this.CurrentConversation.isGroup){
      return;
    }

    this.CurrentConversation.messagesUnread = Math.max(0, this.CurrentConversation.messagesUnread - maximumToRead);

    let index = this.CurrentConversation.messages.length - 1;

    while(!this.CurrentConversation.messages[index].id){
      --index;
    }
    let lastMsgId = this.CurrentConversation.messages[index].id;

    if(this.CurrentConversation.clientLastMessageId != lastMsgId){
      let response = await this.requestsBuilder.SetLastMessageId(lastMsgId, this.CurrentConversation.id);

      if(response.isSuccessfull){
        this.CurrentConversation.clientLastMessageId = lastMsgId;
      }
    }

  }

  public async ChangeConversation(conversation: Chat) {
    if (conversation == this.CurrentConversation) {
      this.CurrentConversation = null;
      return;
    }

    this.CurrentConversation = conversation;

    if (!this.CurrentConversation) {
      return;
    }

    //secure dialog might not even exist on second user side, so update will fail
    if (!(conversation.isSecure && !conversation.authKeyId)) {
      await this.UpdateExisting(conversation);
    }

    //creator should wait until other user is online, and initiate key exchange.
    if (conversation.isSecure && !conversation.authKeyId && conversation.chatRole.role == ChatRole.Creator) {

      if (!conversation.dialogueUser.isOnline) {

        if (!this.connectionManager.SubscribeToUserOnlineStatusChanges(conversation.dialogueUser.id)) {
          this.messagesService.OnFailedToSubsribeToUserStatusChanges();
        } else {
          this.messagesService.OnWaitingForUserToComeOnline()
        }

      } else {

        this.dh.InitiateKeyExchange(conversation);

      }

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
    let chat = this.Conversations.find(x => x.id == chatId);

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
    return await this.ChangeUserChatRole(chatId, userId, ChatRole.Moderator);
  }

  public async RemoveModerator(chatId: number, userId: string) {
    return await this.ChangeUserChatRole(chatId, userId, ChatRole.NoRole);
  }

  private async ChangeUserChatRole(chatId: number, userId: string, newRole: ChatRole) {
    return await this.connectionManager.ChangeUserRole(chatId, userId, newRole);
  }

  public OnBannedInChat(chatId: number, userId: string, banType: BanEvent) {
    let chat = this.Conversations.find(x => x.id == chatId);

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
    this.Conversations = new Array<Chat>();
    this.CurrentConversation = null;
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
    let result = await this.requestsBuilder.SearchForGroups(name);

    if (!result.isSuccessfull) {
      return null;
    }

    if (result.response == null) {
      return new Array<Chat>();

    } else {
      let resultArr = new Array<Chat>();

      for (let i = 0; i < result.response.length; ++i) {
        //we'll send only global(non-local groups as a result.)

        if (!this.Conversations.find(x => x.id == result.response[i].id)) {
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
   * considers recentMessagesMaxCount and length of local messages array.
   */
  public async UpdateMessagesHistory(count: number, recentMessagesMaxCount: number, chat: Chat) {
    let offset;
    //recent messages will be loaded, do not include them in offset.
    if(chat.messagesUnread <= recentMessagesMaxCount){
      offset = chat.messages.length;
    }else{
      offset = chat.messages.length + chat.messagesUnread;
    }

    let result = await this.requestsBuilder.GetConversationMessages(
      offset,
      count,
      chat.id,
      -1);

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

        let decrypted = this.encryptionService.Decrypt(chat.dialogueUser.id, x.encryptedPayload);

        if (!decrypted) {
          this.OnAuthKeyLost(chat);
          return;
        }

        this.DecryptedMessageToLocalMessage(
          x,
          decrypted);

        decryptedMessages.push(decrypted);
      });

      result.response = decryptedMessages;
    }

    result.response = result.response.sort(this.MessagesSortFunc);

    //apply scaling to images
    this.images.ScaleImages(result.response);

    //append old messages to new ones.
    chat.messages = [...result.response.concat(chat.messages)];

    return result.response;
  }

  public async UpdateRecentMessages(count: number, chat: Chat){
    let offset = chat.messages.filter(msg => msg.id > chat.clientLastMessageId).length;

    let result = await this.requestsBuilder.GetConversationMessages(
      offset,
      count,
      chat.id,
      chat.clientLastMessageId);

    if (!result.isSuccessfull) {
      return;
    }

    //server sent zero messages, we reached end of our history.

    if (!result.response || !result.response.length) {
      //only allow receiving messages, if client loaded all recent messages.
      chat.canReceiveMessages = true;
      return result.response;
    }

    if (chat.isSecure) {
      let decryptedMessages = [];

      result.response.forEach(x => {

        let decrypted = this.encryptionService.Decrypt(chat.dialogueUser.id, x.encryptedPayload);

        if (!decrypted) {
          this.OnAuthKeyLost(chat);
          return;
        }

        this.DecryptedMessageToLocalMessage(
          x,
          decrypted);

        decryptedMessages.push(decrypted);
      });

      result.response = decryptedMessages;
    }

    result.response = result.response.sort(this.MessagesSortFunc);

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

    return result.response;
  }

  private DecryptedMessageToLocalMessage(container: Message, decrypted: Message) {
    decrypted.id = container.id;
    decrypted.state = container.state;
    decrypted.timeReceived = container.timeReceived;
    decrypted.user = container.user;
  }

  private MessagesSortFunc(left: Message, right: Message): number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }

  public async DeleteMessages(messages: Array<Message>, from: Chat) {
    let notLocalMessages = messages.filter(x => x.state != MessageState.Pending);

    //delete local unsent messages
    from.messages = from.messages
      .filter(msg => messages.findIndex(x => x.id == msg.id && x.state == MessageState.Pending));

    if (!notLocalMessages.length) {
      return;
    }

    let response = await this.requestsBuilder.DeleteMessages(notLocalMessages, from.id);

    if (!response.isSuccessfull) {
      return;
    }

    //delete messages locally

    from.messages = from
      .messages
      .filter(msg => messages.findIndex(selected => selected.id == msg.id) == -1);

    messages.splice(0, messages.length);
  }

  public async UpdateConversations() {
    let response = await this.requestsBuilder.GetChats(this.device.GetDeviceId());

    if (!response.isSuccessfull) {
      return;
    }

    if (!response.response) {
      this.Conversations = new Array<Chat>();
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

    response.response.forEach(async (chat, index) => {
      if (chat.isSecure) {

        //deviceId is not set, fix it.

        if (chat.authKeyId && this.secureChatsService.AuthKeyExists(chat.authKeyId) && !chat.deviceId) {
          await this.requestsBuilder.UpdateAuthKeyId(chat.authKeyId, chat.id, this.device.GetDeviceId());
          chat.deviceId = this.device.GetDeviceId();
        }

        //delete secure chats where we've lost auth keys
        if (this.device.GetDeviceId() == chat.deviceId && chat.authKeyId && !this.secureChatsService.AuthKeyExists(chat.authKeyId)) {

          toDeleteIndexes.push(index);

        } else {

          let decryptedMessages = [];

          chat.messages.forEach(msg => {

            let decrypted = this.encryptionService.Decrypt(chat.dialogueUser.id, msg.encryptedPayload);

            if (!decrypted) {
              this.OnAuthKeyLost(chat);
              return;
            }

            this.DecryptedMessageToLocalMessage(
              msg,
              decrypted);

            decryptedMessages.push(decrypted);
          });

          chat.messages = decryptedMessages;
        }
      } else {

        if (!chat.isGroup) {
          chat.isMessagingRestricted = chat.dialogueUser.isMessagingRestricted;
        }
      }
    });

    let chatsToDelete = Array<Chat>();

    toDeleteIndexes.forEach(x => {
      chatsToDelete.push(response.response.splice(x, 1)[0]);
    });

    this.Conversations = response.response;

    //Initiate signalR group connections

    await this.connectionManager.Start();

    chatsToDelete.forEach(x => {
      this.RemoveGroup(x);
    });
  }


  public async OnMessageReceived(data: MessageReceivedModel) {
    let chat = this.Conversations
      .find(x => x.id == data.conversationId);

    if (!chat) {
      return;
    }

    if (data.secure) {
      let decrypted = this.encryptionService.Decrypt(data.senderId, data.message.encryptedPayload);

      if (!decrypted) {
        this.OnAuthKeyLost(chat);
        return;
      }

      this.DecryptedMessageToLocalMessage(data.message, decrypted);

      data.message = decrypted;
    }

    this.images.ScaleImage(data.message);

    //we can accept message only if chat.canAcceptMessages is true.
    if(chat.canReceiveMessages){
      chat.messages.push(data.message);
      chat.messages = [...chat.messages];

      //if user was in different chat, increment unread counter.
      //if not, UI should automatically update clientLastMessageId.
      if(!this.CurrentConversation || chat.id != this.CurrentConversation.id){
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
        isAttachment: false,
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
        isAttachment: isAttachment,
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
    let result = await this.requestsBuilder.GetAttachmentsForConversation(
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
    let result = await this.requestsBuilder.FindUsersInChat(username, chatId);

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
    chat.clientLastMessageId = Math.max(0, chat.clientLastMessageId - 1);
    this.PendingReadMessages.splice(pendingIndex, 1);
  }

  public async CreateGroup(name: string, isPublic: boolean) {
    let result = await this.requestsBuilder.CreateConversation(
      name, this.authService.User.id, null, null, true, isPublic);

    if (!result.isSuccessfull) {
      return;
    }

    this.connectionManager.InitiateConnections(new Array<number>(1).fill(result.response.id));

    result.response.messages = new Array<Message>();

    this.Conversations = [...this.Conversations, result.response];
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

    let chat = await this.requestsBuilder.GetConversationById(group.id, true);

    if (!chat.isSuccessfull) {
      return;
    }

    this.Conversations = [...this.Conversations, chat.response];
  }

  public KickUser(user: AppUser, from: Chat) {
    this.connectionManager.RemoveUserFromConversation(user.id, from.id, false);
  }

  public ExistsIn(id: number) {
    return this.Conversations.find(x => x.id == id) != null;
  }

  public FindDialogWith(user: AppUser) {
    return this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == user.id);
  }

  public FindDialogWithById(userId: string) {
    return this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == userId);
  }

  public FindDialogWithSecurityCheck(user: AppUser, secure: boolean) {
    return this.Conversations.find(x =>
        !x.isGroup
        && x.dialogueUser.id == user.id
        && x.isSecure == secure);
  }

  public async UpdateExisting(target: Chat) : Promise<void> {
    let result = await this.requestsBuilder.GetConversationById(target.id, true);

    if (!result.isSuccessfull) {
      return;
    }

    this.UpdateConversationFields(target, result.response);
  }

  public ForwardMessagesTo(destination: Array<Chat>, messages: Array<Message>) {
    if (destination == null || destination.length == 0) {
      return;
    }

    destination.forEach(
      (conversation) => {

        if (this.IsSecureChatAndNoAuthKey(conversation)) {
          return;
        }

        messages.forEach(async msg => {
          let messageToSend = this.BuildForwardedMessage(conversation.id, msg.forwardedMessage ? msg.forwardedMessage : msg);
          await this.SendChatMessage(messageToSend, conversation);
        });
      }
    );

    messages.splice(0, messages.length);
  }

  public async RemoveAllMessages(group: Chat) : Promise<void> {
    let response = await this.requestsBuilder.DeleteMessages(group.messages, group.id);

    if (!response.isSuccessfull) {
      return;
    }

    //delete messages locally

    if (group.messages.length != 0) {
      group.messages.splice(0, group.messages.length);
    }

    this.CurrentConversation = null;
  }

  public Leave(from: Chat) {
    this.connectionManager.RemoveUserFromConversation(this.authService.User.id, from.id, true);
    this.CurrentConversation = null;
  }

  public async ChangeThumbnail(file: File, where: Chat, progress: (value: number) => void): Promise<void> {
    let result = await this.requestsBuilder.UploadConversationThumbnail(file, progress, where.id);

    if (!result.isSuccessfull) {
      return;
    }

    where.thumbnailUrl = result.response.thumbnailUrl;
    where.fullImageUrl = result.response.fullImageUrl;
  }

  public async ChangeConversationName(name: string, where: Chat): Promise<void> {
    let result = await this.requestsBuilder.ChangeConversationName(name, where.id);

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

    let chat = await this.requestsBuilder.GetConversationById(data.chatId, true);

    if (!chat.isSuccessfull) {
      return;
    }

    //when current user was added to group

    if (data.user.id == this.authService.User.id) {
      this.Conversations = [...this.Conversations, chat.response];
      return;
    }

    //when someone added new user to group.

    chat.response.participants.push(data.user);
  }

  public OnRemovedFromGroup(data: RemovedFromGroupModel) {
    let chatIndex = this.Conversations.findIndex(x => x.id == data.conversationId);

    if (chatIndex == -1) {
      return;
    }


    //either this client left or creator removed him.
    if (data.userId == this.authService.User.id) {

      if (this.CurrentConversation && this.CurrentConversation.id == data.conversationId) {
          this.CurrentConversation = null;
      }

      this.Conversations.splice(chatIndex, 1);

    } else {

      let participants = this.Conversations[chatIndex].participants;

      participants.splice(participants.findIndex(x => x.id == data.userId), 1);
    }
  }

  public OnMessageRead(msgId: number, conversationId: number) {
    let conversation = this.Conversations.find(x => x.id == conversationId);

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
    let conversation = this.Conversations.find(x => x.id == conversationId);

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

  public async UploadFile(file: File, progress: (value: number) => void, to: Chat) {

    let response = await this.requestsBuilder.UploadFile(file, progress, to.id);

    if (!response.isSuccessfull) {
      return;
    }

    let message = this.BuildMessage(null, to.id, true, response.response);

    let successfull = await this.SendChatMessage(message, to);

    if(successfull){
      this.images.ScaleImage(message);
    }
  }

  public async UploadImages(files: FileList, progress: (value: number) => void, to: Chat) {
    if (files.length == 0) {
      return;
    }

    let conversationToSend = to.id;

    let response = await this.requestsBuilder.UploadImages(files, progress, to.id);

    if (!response.isSuccessfull) {
      this.messagesService.DisplayMessage("Some files were not uploaded.");
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

  public UpdateConversationFields(old: Chat, New: Chat): void {
    old.name = New.name;
    old.fullImageUrl = New.fullImageUrl;
    old.thumbnailUrl = New.thumbnailUrl;
    old.participants = New.participants;
    old.isMessagingRestricted = New.isMessagingRestricted;
  }
}
