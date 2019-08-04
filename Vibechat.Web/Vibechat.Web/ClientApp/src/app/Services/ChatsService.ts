import {Chat} from "../Data/Chat";
import {MessageReceivedModel} from "../Shared/MessageReceivedModel";
import {AuthService} from "../Auth/AuthService";
import {AddedToGroupModel} from "../Shared/AddedToGroupModel";
import {ChatMessage} from "../Data/ChatMessage";
import {ApiRequestsBuilder} from "../Requests/ApiRequestsBuilder";
import {BanEvent, ConnectionManager} from "../Connections/ConnectionManager";
import {RemovedFromGroupModel} from "../Shared/RemovedFromGroupModel";
import {MessageState} from "../Shared/MessageState";
import {UserInfo} from "../Data/UserInfo";
import {MessageAttachment} from "../Data/MessageAttachment";
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
    private connectionManager: ConnectionManager,
    private secureChatsService: SecureChatsService,
    private encryptionService: E2EencryptionService,
    private messagesService: MessageReportingService,
    private dh: DHServerKeyExchangeService,
    private images: ImageScalingService,
    private device: DeviceService)
  {
    this.connectionManager.setConversationsService(this);
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

        if (!this.connectionManager.SubsribeToUserOnlineStatusChanges(conversation.dialogueUser.id)) {
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
    this.connectionManager.UnsubsribeFromUserOnlineStatusChanges(user);
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

  public CreateSecureChat(user: UserInfo) : boolean {
    let dialog = this.FindDialogWithSecurityCheck(user, true);

    if (dialog) {
      return false;
    }

    this.CreateDialogWith(user, true);
    return true;
  }

  public ResendMessages(messages: Array<ChatMessage>, chat: Chat) {
    messages.forEach((msg) => {
      this.SendChatMessage(msg, chat);
    });

  }

  public async UpdateMessagesForConversation(count: number, chat: Chat) {
    let result = await this.requestsBuilder.GetConversationMessages(
      chat.messages.length,
      count,
      chat.id)

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

  private DecryptedMessageToLocalMessage(container: ChatMessage, decrypted: ChatMessage) {
    decrypted.id = container.id;
    decrypted.state = container.state;
    decrypted.timeReceived = container.timeReceived;
    decrypted.user = container.user;
  }

  private MessagesSortFunc(left: ChatMessage, right: ChatMessage): number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }

  public async DeleteMessages(messages: Array<ChatMessage>, from: Chat) {
    let notLocalMessages = messages.filter(x => x.state != MessageState.Pending)

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
          conversation.messages = new Array<ChatMessage>();
        }

      })

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


  public OnMessageReceived(data: MessageReceivedModel): void {
    let conversation = this.Conversations
      .find(x => x.id == data.conversationId);

    if (!conversation) {
      return;
    }

    if (data.secure) {
      let decrypted = this.encryptionService.Decrypt(data.senderId, data.message.encryptedPayload);

      if (!decrypted) {
        this.OnAuthKeyLost(conversation);
        return;
      }

      this.DecryptedMessageToLocalMessage(data.message, decrypted);

      data.message = decrypted;
    }

    this.images.ScaleImage(data.message);

    conversation.messages.push(data.message);

    conversation.messages = [...conversation.messages];

    if (data.senderId != this.authService.User.id) {
      ++conversation.messagesUnread;
    }
  }

  public BuildForwardedMessage(whereTo: number, forwarded: ChatMessage): ChatMessage {
    var min = 1;
    var max = 100000;
    var clientMessageId = Math.floor(Math.random() * (+max - +min) + +min);

    return new ChatMessage(
      {
        id: clientMessageId,
        isAttachment: false,
        user: this.authService.User,
        conversationID: whereTo,
        state: MessageState.Pending,
        timeReceived: new Date(),
        forwardedMessage: forwarded
      });
  }

  public BuildMessage(message: string, whereTo: number, isAttachment = false, AttachmentInfo: MessageAttachment = null): ChatMessage {
    var min = 1;
    var max = 100000;
    var clientMessageId = Math.floor(Math.random() * (+max - +min) + +min);

    return new ChatMessage(
      {
        id: clientMessageId,
        messageContent: message,
        isAttachment: isAttachment,
        attachmentInfo: AttachmentInfo,
        user: this.authService.User,
        conversationID: whereTo,
        state: MessageState.Pending,
        timeReceived: new Date()
      });
  }

  public async BanFromConversation(userToBan: UserInfo, from: Chat) : Promise<boolean> {
    return await this.connectionManager.BlockUserInChat(userToBan.id, from.id, BanEvent.Banned);
  }

  public async UnbanFromConversation(userToUnban: UserInfo, from: Chat): Promise<boolean> {
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

  public SendMessage(message: string, to: Chat) {
    let messageToSend = this.BuildMessage(message, to.id);

    if (this.IsSecureChatAndNoAuthKey(to)) {
      return;
    }

    to.messages.push(
      messageToSend
    );

    this.SendChatMessage(messageToSend, to);
  }

  public async FindUsersInChat(username: string, chatId: number) : Promise<Array<UserInfo>> {
    let result = await this.requestsBuilder.FindUsersInChat(username, chatId);

    if (!result.isSuccessfull) {
      return null;
    }

    return result.response.usersFound;
  }

  public async ReadMessage(message: ChatMessage) {

    if (message.user.id == this.authService.User.id) {
      return;
    }

    if (this.PendingReadMessages.find(x => x == message.id)) {
      return;
    }

    this.PendingReadMessages.push(message.id);

    let result = await this.connectionManager.ReadMessage(message.id, message.conversationID);

    if (!result) {
      return;
    }

    message.state = MessageState.Read;
  }

  public async CreateGroup(name: string, isPublic: boolean) {
    let result = await this.requestsBuilder.CreateConversation(name, this.authService.User.id, null, null, true, isPublic);

    if (!result.isSuccessfull) {
      return;
    }

    this.connectionManager.InitiateConnections(new Array<number>(1).fill(result.response.id));

    result.response.messages = new Array<ChatMessage>();

    this.Conversations = [...this.Conversations, result.response];
  }

  public CreateDialogWith(user: UserInfo, secure: boolean) {
    this.connectionManager.CreateDialog(user, secure);
  }

  public RemoveDialogWith(user: UserInfo) {
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

  public KickUser(user: UserInfo, from: Chat) {
    this.connectionManager.RemoveUserFromConversation(user.id, from.id, false);
  }

  public ExistsIn(id: number) {
    return this.Conversations.find(x => x.id == id) != null;
  }

  public FindDialogWith(user: UserInfo) {
    return this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == user.id);
  }

  public FindDialogWithById(userId: string) {
    return this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == userId);
  }

  public FindDialogWithSecurityCheck(user: UserInfo, secure: boolean) {
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

  public ForwardMessagesTo(destination: Array<Chat>, messages: Array<ChatMessage>) {
    if (destination == null || destination.length == 0) {
      return;
    }

    destination.forEach(
      (conversation) => {

        if (this.IsSecureChatAndNoAuthKey(conversation)) {
          return;
        }

        messages.forEach(msg => {
          let messageToSend = this.BuildForwardedMessage(conversation.id, msg.forwardedMessage ? msg.forwardedMessage : msg);

          this.connectionManager.SendMessage(messageToSend, conversation);

          conversation.messages.push(messageToSend);

        })
      }
    )

    messages.splice(0, messages.length);
  }

  public async RemoveAllMessages(group: Chat) : Promise<void> {
    let response = await this.requestsBuilder.DeleteMessages(group.messages, group.id)

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

  public InviteUsersToGroup(users: Array<UserInfo>, group: Chat) {
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


  // // if the client received an answer to an API call to remove secure chats with lost
  ////keys quickly enough, do not delete chats twice.
  //  if (this.Conversations[chatIndex].isSecure) {
  //    return;
  //  }

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

    if (conversation == null) {
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

    if (message.user.id != this.authService.User.id) {
      //this is to prevent values like "-1" (could happen in some rare cases)
      conversation.messagesUnread = Math.max(0, conversation.messagesUnread - 1);
    }

    let pendingIndex = this.PendingReadMessages.findIndex(x => x == message.id);

    if (pendingIndex != -1) {
      this.PendingReadMessages.splice(pendingIndex, 1);
    }
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

  private SendChatMessage(message: ChatMessage, to: Chat) {
    if (to.isSecure) {

      let encrypted = this.encryptionService.Encrypt(to.dialogueUser.id, message);

      if (!encrypted) {
        this.OnAuthKeyLost(to);
        return;
      }

      this.connectionManager.SendMessageToSecureChat(
        encrypted,
        message.id,
        to.dialogueUser.id,
        to.id);

    } else {
      this.connectionManager.SendMessage(message, to);
    }
  }

  public async UploadFile(file: File, progress: (value: number) => void, to: Chat) {

    let response = await this.requestsBuilder.UploadFile(file, progress, to.id)

    if (!response.isSuccessfull) {
      return;
    }

    let message = this.BuildMessage(null, to.id, true, response.response);

    this.SendChatMessage(message, to);

    this.images.ScaleImage(message);
    to.messages.push(message);
  }

  public async UploadImages(files: FileList, progress: (value: number) => void, to: Chat) {
    if (files.length == 0) {
      return;
    }

    let conversationToSend = to.id;

    let response = await this.requestsBuilder.UploadImages(files, progress, to.id)

    if (!response.isSuccessfull) {
      this.messagesService.DisplayMessage("Some files were not uploaded.");
    }

    response.response.uploadedFiles.forEach(
      (file) => {
        let message = this.BuildMessage(null, conversationToSend, true, file);

        this.SendChatMessage(message, to);

        this.images.ScaleImage(message);
        to.messages.push(message);
      })
  }

  public UpdateConversationFields(old: Chat, New: Chat): void {
    old.name = New.name;
    old.fullImageUrl = New.fullImageUrl;
    old.thumbnailUrl = New.thumbnailUrl;
    old.participants = New.participants;
  }
}
