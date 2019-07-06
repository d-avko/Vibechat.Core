import { ConversationTemplate } from "../Data/ConversationTemplate";
import { MessageReceivedModel } from "../Shared/MessageReceivedModel";
import { MessagesDateParserService } from "./MessagesDateParserService";
import { AuthService } from "../Auth/AuthService";
import { AddedToGroupModel } from "../Shared/AddedToGroupModel";
import { ChatMessage } from "../Data/ChatMessage";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { ConnectionManager } from "../Connections/ConnectionManager";
import { RemovedFromGroupModel } from "../Shared/RemovedFromGroupModel";
import { MessageState } from "../Shared/MessageState";
import { UserInfo } from "../Data/UserInfo";
import { MessageAttachment } from "../Data/MessageAttachment";
import { UploadFilesResponse } from "../Data/UploadFilesResponse";
import { HttpResponse } from "@angular/common/http";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { Injectable } from "@angular/core";
import { SecureChatsService } from "../Encryption/SecureChatsService";
import { E2EencryptionService } from "../Encryption/E2EencryptionService";
import { DHServerKeyExchangeService } from "../Encryption/DHServerKeyExchange";
import { MessageReportingService } from "./MessageReportingService";
import { ImageScalingService } from "./ImageScalingService";

@Injectable({
  providedIn: 'root'
})
export class ChatsService {
  constructor(
    private dateParser: MessagesDateParserService,
    private authService: AuthService,
    private requestsBuilder: ApiRequestsBuilder,
    private connectionManager: ConnectionManager,
    private secureChatsService: SecureChatsService,
    private encryptionService: E2EencryptionService,
    private messagesService: MessageReportingService,
    private dh: DHServerKeyExchangeService,
    private images: ImageScalingService)
  {
    this.connectionManager.setConversationsService(this);
    this.PendingReadMessages = new Array<number>();
    this.dh.setChatService(this);
  }

  public Conversations: Array<ConversationTemplate>;
  public CurrentConversation: ConversationTemplate;
  private PendingReadMessages: Array<number>;

  public IsConversationSelected(): boolean {
    return this.CurrentConversation != null;
  }

  public GetConversationsIds() {
    return this.Conversations.map(x => x.conversationID);
  }

  public async ChangeConversation(conversation: ConversationTemplate) {
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
    if (conversation.isSecure && !conversation.authKeyId && conversation.creator.id == this.authService.User.id) {

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

  public OnLogOut() {
    this.Conversations = new Array<ConversationTemplate>();
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
      return new Array<ConversationTemplate>();

    } else {
      return [...result.response];
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

  public async GetMessagesForConversation(count: number, chat: ConversationTemplate) {
    let result = await this.requestsBuilder.GetConversationMessages(
      chat.messages.length,
      count,
      chat.conversationID)

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

    this.dateParser.ParseStringDatesInMessages(result.response);

    //apply scaling to images
    this.images.ScaleImages(result.response);

    //append old messages to new ones.
    chat.messages = [...result.response.concat(chat.messages)];
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

  public async DeleteMessages(messages: Array<ChatMessage>, from: ConversationTemplate) {
    let notLocalMessages = messages.filter(x => x.state != MessageState.Pending)

    //delete local unsent messages
    from.messages = from.messages
      .filter(msg => messages.findIndex(x => x.id == msg.id && x.state == MessageState.Pending));

    if (!notLocalMessages.length) {
      return;
    }

    let response = await this.requestsBuilder.DeleteMessages(notLocalMessages, from.conversationID);

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
    let response = await this.requestsBuilder.UpdateConversationsRequest();

    if (!response.isSuccessfull) {
      return;
    }

    if (!response.response) {
      this.Conversations = new Array<ConversationTemplate>();
      await this.connectionManager.Start();
      return;
    }

    response.response
      .forEach((conversation) => {

        if (conversation.messages != null) {
          this.dateParser.ParseStringDatesInMessages(conversation.messages);
          this.images.ScaleImages(conversation.messages);
        } else {
          conversation.messages = new Array<ChatMessage>();
        }

      })

    let toDeleteIndexes = [];
    response.response.forEach((x, index) => {
      //delete secure chats where we've lost auth keys
      if (x.isSecure) {

        if (x.authKeyId && !this.secureChatsService.AuthKeyExists(x.authKeyId)) {
          toDeleteIndexes.push(index);
        } else {

          let decryptedMessages = [];

          x.messages.forEach(msg => {

            let decrypted = this.encryptionService.Decrypt(x.dialogueUser.id, msg.encryptedPayload);

            if (!decrypted) {
              this.OnAuthKeyLost(x);
              return;
            }

            this.DecryptedMessageToLocalMessage(
              msg,
              decrypted);

            decryptedMessages.push(decrypted);
          });

          x.messages = decryptedMessages;
        }
      }
    });

    let chatsToDelete = Array<ConversationTemplate>();

    toDeleteIndexes.forEach(x => {
      chatsToDelete.push(response.response.splice(x, 1)[0]);
    });

    this.Conversations = response.response;

    this.Conversations.forEach((x) => {

      if (!x.isGroup) {
        x.isMessagingRestricted = x.dialogueUser.isMessagingRestricted;
      }
    });

    //Initiate signalR group connections

    await this.connectionManager.Start();

    chatsToDelete.forEach(x => {
      this.RemoveGroup(x);
    });

  }

  public OnMessageReceived(data: MessageReceivedModel): void {
    let conversation = this.Conversations
      .find(x => x.conversationID == data.conversationId);

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

    this.dateParser.ParseStringDateInMessage(data.message);

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

  public async BanFromConversation(userToBan: UserInfo, from: ConversationTemplate) : Promise<void> {
    let result = await this.requestsBuilder.BanFromConversation(userToBan.id, from.conversationID);

    if (!result.isSuccessfull) {
      return;
    }

    userToBan.isBlockedInConversation = true;
  }

  public async UnbanFromConversation(userToUnban: UserInfo, from: ConversationTemplate): Promise<void> {
    let result = await this.requestsBuilder.UnBanFromConversation(userToUnban.id, from.conversationID);

    if (!result.isSuccessfull) {
      return;
    }

    userToUnban.isBlockedInConversation = false;
  }

  public RemoveGroup(group: ConversationTemplate) {
    this.connectionManager.RemoveConversation(group);
  }

  public async GetAttachmentsFor(groupId: number, attachmentKind: string, offset: number, count: number) {
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

    this.dateParser.ParseStringDatesInMessages(result.response);
  }

  public OnAuthKeyLost(chat: ConversationTemplate) {
    this.RemoveGroup(chat);
  }

  public SendMessage(message: string, to: ConversationTemplate) {
    let messageToSend = this.BuildMessage(message, to.conversationID);

    to.messages.push(
      messageToSend
    );

    //secure chat
    if (to.isSecure) {

      let encrypted = this.encryptionService.Encrypt(to.dialogueUser.id, messageToSend);

      if (!encrypted) {
        this.OnAuthKeyLost(to);
        return;
      }

      this.connectionManager.SendMessageToSecureChat(
        encrypted,
        messageToSend.id,
        to.dialogueUser.id,
        to.conversationID);

    } else {
      //non-secure chat

      this.connectionManager.SendMessage(messageToSend, to);
    }
  }

  public ReadMessage(message: ChatMessage) {

    if (message.user.id == this.authService.User.id) {
      return;
    }

    if (this.PendingReadMessages.find(x => x == message.id)) {
      return;
    }

    this.PendingReadMessages.push(message.id);

    this.connectionManager.ReadMessage(message.id, message.conversationID);
  }

  public async CreateGroup(name: string, isPublic: boolean) {
    let result = await this.requestsBuilder.CreateConversation(name, this.authService.User.id, null, null, true, isPublic);

    if (!result.isSuccessfull) {
      return;
    }

    this.connectionManager.InitiateConnections(new Array<number>(1).fill(result.response.conversationID));

    result.response.messages = new Array<ChatMessage>();

    this.Conversations = [...this.Conversations, result.response];
  }

  public CreateDialogWith(user: UserInfo, secure: boolean) {
    this.connectionManager.CreateDialog(user, secure);
  }

  public RemoveDialogWith(user: UserInfo) {
    this.connectionManager.RemoveConversation(this.FindDialogWith(user));
  }

  public JoinGroup(group: ConversationTemplate) {
    this.connectionManager.AddUserToConversation(this.authService.User.id, group);
  }

  public KickUser(user: UserInfo, from: ConversationTemplate) {
    this.connectionManager.RemoveUserFromConversation(user.id, from.conversationID, false);
  }

  public ExistsIn(id: number) {
    return this.Conversations.find(x => x.conversationID == id) != null;
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

  public async UpdateExisting(target: ConversationTemplate) : Promise<void> {
    let result = await this.requestsBuilder.GetConversationById(target.conversationID);

    if (!result.isSuccessfull) {
      return;
    }

    this.UpdateConversationFields(target, result.response);
  }

  public ForwardMessagesTo(destination: Array<ConversationTemplate>, messages: Array<ChatMessage>) {
    if (destination == null || destination.length == 0) {
      return;
    }

    destination.forEach(
      (conversation) => {

        messages.forEach(msg => {
          let messageToSend = this.BuildForwardedMessage(conversation.conversationID, msg.forwardedMessage ? msg.forwardedMessage : msg);

          this.connectionManager.SendMessage(messageToSend, conversation);

          conversation.messages.push(messageToSend);

        })
      }
    )

    messages.splice(0, messages.length);
  }

  public async RemoveAllMessages(group: ConversationTemplate) : Promise<void> {
    let response = await this.requestsBuilder.DeleteMessages(group.messages, group.conversationID)

    if (!response.isSuccessfull) {
      return;
    }

    //delete messages locally

    if (group.messages.length != 0) {
      group.messages.splice(0, group.messages.length);
    }

    this.CurrentConversation = null;     
  }

  public Leave(from: ConversationTemplate) {
    this.connectionManager.RemoveUserFromConversation(this.authService.User.id, from.conversationID, true);
    this.CurrentConversation = null;
  }

  public async ChangeThumbnail(file: File, where: ConversationTemplate): Promise<void> {
    let result = await this.requestsBuilder.UploadConversationThumbnail(file, where.conversationID);

    if (!result.isSuccessfull) {
      return;
    }

    where.thumbnailUrl = result.response.thumbnailUrl;
    where.fullImageUrl = result.response.fullImageUrl;
  }

  public async ChangeConversationName(name: string, where: ConversationTemplate): Promise<void> {
    let result = await this.requestsBuilder.ChangeConversationName(name, where.conversationID);

    if (!result.isSuccessfull) {
      return;
    }

    where.name = name;     
  }

  public InviteUsersToGroup(users: Array<UserInfo>, group: ConversationTemplate) {
    if (users == null || users.length == 0) {
      return;
    }

    users.forEach(
      (value) => {
        this.connectionManager.AddUserToConversation(value.id, group);
      }
    )

    //Now add users locally

    users.forEach(
      (user) => {

        //sort of sanitization of input

        if (user.id == this.authService.User.id) {
          return;
        }

        group.participants.push(user);
        group.participants = [...group.participants];

      }
    )
  }

  public async OnAddedToGroup(data: AddedToGroupModel): Promise<void> {

    if (data.conversation.messages != null) {
      this.dateParser.ParseStringDatesInMessages(data.conversation.messages);
    } else {
      data.conversation.messages = new Array<ChatMessage>();
    }

    if (data.conversation.isGroup) {

      //we created new group.

      if (data.user.id == this.authService.User.id) {

        this.Conversations = [...this.Conversations, data.conversation];
      } else {

        //someone added new user to existing group

        let conversation = this.Conversations.find(x => x.conversationID == data.conversation.conversationID);
        conversation.participants.push(data.user);
        conversation.participants = [...conversation.participants];
      }
    } else {

      this.Conversations = [...this.Conversations, data.conversation];

    }

    //updating info about dialog is pointless.

    if (data.conversation.isGroup) {
      await this.UpdateExisting(data.conversation);
    }
  }

  public OnRemovedFromGroup(data: RemovedFromGroupModel) {
    let chatIndex = this.Conversations.findIndex(x => x.conversationID == data.conversationId);
    
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

      if (this.CurrentConversation && this.CurrentConversation.conversationID == data.conversationId) {
          this.CurrentConversation = null;
      }

      this.Conversations.splice(chatIndex, 1);

    } else {

      let participants = this.Conversations[chatIndex].participants;

      participants.splice(participants.findIndex(x => x.id == data.userId), 1);
    }
  }

  public OnMessageRead(msgId: number, conversationId: number) {
    let conversation = this.Conversations.find(x => x.conversationID == conversationId);

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
    let conversation = this.Conversations.find(x => x.conversationID == conversationId);

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

  public async UploadFile(file: File, to: ConversationTemplate) {

    let response = await this.requestsBuilder.UploadFile(file, to.conversationID)

    if (!response.isSuccessfull) {
      return;
    }

    let message = this.BuildMessage(null, to.conversationID, true, response.response);

    //secure chat
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
        to.conversationID);

    } else {
      this.connectionManager.SendMessage(message, to);
    }

    this.images.ScaleImage(message);
    to.messages.push(message);
  }

  public async UploadImages(files: FileList, to: ConversationTemplate) {
    if (files.length == 0) {
      return;
    }

    let conversationToSend = to.conversationID;

    let response = await this.requestsBuilder.UploadImages(files, to.conversationID)

    if (!response.isSuccessfull) {
      return;
    }

    response.response.uploadedFiles.forEach(
      (file) => {
        let message = this.BuildMessage(null, conversationToSend, true, file);

        //secure chat
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
            to.conversationID);

        } else {
          this.connectionManager.SendMessage(message, to);
        }

        this.images.ScaleImage(message);
        to.messages.push(message);
      })
  }

  public UpdateConversationFields(old: ConversationTemplate, New: ConversationTemplate): void {
    old.isMessagingRestricted = New.isMessagingRestricted;
    old.name = New.name;
    old.fullImageUrl = New.fullImageUrl;
    old.thumbnailUrl = New.thumbnailUrl;
  }
}
