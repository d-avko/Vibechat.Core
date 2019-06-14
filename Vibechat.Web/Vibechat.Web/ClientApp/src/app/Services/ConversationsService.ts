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

@Injectable({
  providedIn: 'root'
})
export class ConversationsService {
  constructor(
    private dateParser: MessagesDateParserService,
    private authService: AuthService,
    private requestsBuilder: ApiRequestsBuilder,
    private connectionManager: ConnectionManager,
    private secureChatsService: SecureChatsService,
    private encryptionService: E2EencryptionService)
  {
    this.connectionManager.setConversationsService(this);
    this.PendingReadMessages = new Array<number>();
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

    await this.UpdateExisting(conversation);
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

  public async GetMessagesForCurrentConversation(count: number) {
    let secure = this.CurrentConversation.authKeyId != null;

    let result = await this.requestsBuilder.GetConversationMessages(
      this.CurrentConversation.messages.length,
      count,
      this.CurrentConversation.conversationID)

    if (!result.isSuccessfull) {
      return;
    }

    //server sent zero messages, we reached end of our history.

    if (result.response == null || result.response.length == 0) {
      return result.response;
    }

    if (secure) {
      result.response.forEach(x => {

        this.DecryptedMessageToLocalMessage(
          x,
          this.encryptionService.Decrypt(this.CurrentConversation.dialogueUser.id, x.encryptedPayload));

      })
    }

    result.response = result.response.sort(this.MessagesSortFunc);

    this.dateParser.ParseStringDatesInMessages(result.response);

    //append old messages to new ones.
    this.CurrentConversation.messages = [...result.response.concat(this.CurrentConversation.messages)];
  }

  private DecryptedMessageToLocalMessage(container: ChatMessage, decrypted: ChatMessage) {
    decrypted.id = container.id;
    decrypted.state = container.state;
    decrypted.timeReceived = container.timeReceived;
  }

  private MessagesSortFunc(left: ChatMessage, right: ChatMessage): number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }

  public async DeleteMessages(messages: Array<ChatMessage>) {
    let currentConversationId = this.CurrentConversation.conversationID;

    let notLocalMessages = messages.filter(x => x.state != MessageState.Pending)

    //delete local unsent messages
    this.CurrentConversation.messages = this.CurrentConversation.messages
      .filter(msg => notLocalMessages.findIndex(selected => selected.id == msg.id) == -1);

    if (notLocalMessages.length == 0) {
      return;
    }

    let response = await this.requestsBuilder.DeleteMessages(notLocalMessages, this.CurrentConversation.conversationID);

    if (!response.isSuccessfull) {
      return;
    }

    //delete messages locally

    let conversation = this.Conversations.find(x => x.conversationID == currentConversationId);

    conversation.messages = conversation
      .messages
      .filter(msg => messages.findIndex(selected => selected.id == msg.id) == -1);

    messages.splice(0, messages.length);
  }

  public async UpdateConversations() {
    let response = await this.requestsBuilder.UpdateConversationsRequest();

    if (!response.isSuccessfull) {
      return;
    }

    if (response.response != null) {

      //parse string date to js Date

      response.response
        .forEach((conversation) => {

          if (conversation.messages != null) {
            this.dateParser.ParseStringDatesInMessages(conversation.messages);
          } else {
            conversation.messages = new Array<ChatMessage>();
          }

        })

      let toDeleteIndexes = [];
      response.response.forEach((x, index) => {
        //delete secure chats where we've lost auth keys
        if (x.authKeyId) {

          if (!this.secureChatsService.AuthKeyExists(x.authKeyId)) {
            toDeleteIndexes.push(index);
          } else {

            x.messages.forEach(msg => {
              this.DecryptedMessageToLocalMessage(
                msg,
                this.encryptionService.Decrypt(x.dialogueUser.id, msg.encryptedPayload))
            });

          }
        }
      });

      toDeleteIndexes.forEach(x => response.response.splice(x, 1));

      this.Conversations = response.response;

      this.Conversations.forEach((x) => {

        if (!x.isGroup) {
          x.isMessagingRestricted = x.dialogueUser.isMessagingRestricted;
        }
      });

    } else {
      this.Conversations = new Array<ConversationTemplate>();
    }

    //Initiate signalR group connections

    this.connectionManager.Start();
  }

  public OnMessageReceived(data: MessageReceivedModel): void {
    if (data.secure) {
      let decrypted = this.encryptionService.Decrypt(data.senderId, data.message.encryptedPayload);

      if (!decrypted) {
        return;
      }

      this.DecryptedMessageToLocalMessage(data.message, decrypted);

      data.message = decrypted;
    }

    this.dateParser.ParseStringDateInMessage(data.message);

    let conversation = this.Conversations
      .find(x => x.conversationID == data.conversationId);

    conversation.messages.push(data.message);

    conversation.messages = [...conversation.messages];

    if (data.senderId != this.authService.User.id) {
      ++conversation.messagesUnread;
    }
  }

  public OnSecureMessageReceived(groupId: number, message: string, senderId: string): void {
    let conversation = this.Conversations
      .find(x => x.conversationID == groupId);

    if (!conversation) {
      return;
    }

    let decrypted = this.encryptionService.Decrypt(conversation.dialogueUser.id, message);

    conversation.messages.push(decrypted);

    conversation.messages = [...conversation.messages];

    if (senderId != this.authService.User.id) {
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

  public async BanFromConversation(userToBan: UserInfo) : Promise<void> {
    let result = await this.requestsBuilder.BanFromConversation(userToBan.id, this.CurrentConversation.conversationID);

    if (!result.isSuccessfull) {
      return;
    }

    userToBan.isBlockedInConversation = true;
  }

  public async UnbanFromConversation(userToUnban: UserInfo): Promise<void> {
    let result = await this.requestsBuilder.UnBanFromConversation(userToUnban.id, this.CurrentConversation.conversationID);

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

  public SendMessage(message: string) {
    let messageToSend = this.BuildMessage(message, this.CurrentConversation.conversationID);

    this.CurrentConversation.messages.push(
      messageToSend
    );

    //secure chat
    if (this.CurrentConversation.authKeyId) {

      this.connectionManager.SendMessageToSecureChat(
        this.encryptionService.Encrypt(this.CurrentConversation.dialogueUser.id, messageToSend),
        messageToSend.id,
        this.CurrentConversation.dialogueUser.id,
        this.CurrentConversation.conversationID);

    } else {
      //non-secure chat

      this.connectionManager.SendMessage(messageToSend, this.CurrentConversation);
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

  public CreateDialogWith(user: UserInfo) {
    this.connectionManager.CreateDialog(user);
  }

  public RemoveDialogWith(user: UserInfo) {
    this.connectionManager.RemoveConversation(this.FindDialogWith(user));
    this.CurrentConversation = null;
  }

  public JoinGroup(group: ConversationTemplate) {
    this.connectionManager.AddUserToConversation(this.authService.User.id, group);
  }

  public KickUser(user: UserInfo) {
    this.connectionManager.RemoveUserFromConversation(user.id, this.CurrentConversation.conversationID, false);
  }

  public ExistsIn(id: number) {
    return this.Conversations.find(x => x.conversationID == id) != null;
  }

  public FindDialogWith(user: UserInfo) {
    return this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == user.id);
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

  public async ChangeThumbnail(file: File): Promise<void> {
    let currentConversationId = this.CurrentConversation.conversationID;

    let result = await this.requestsBuilder.UploadConversationThumbnail(file, this.CurrentConversation.conversationID);

    if (!result.isSuccessfull) {
      return;
    }

    let conversation = this.Conversations.find(x => x.conversationID == currentConversationId);

    conversation.thumbnailUrl = result.response.thumbnailUrl;
    conversation.fullImageUrl = result.response.fullImageUrl;
  }

  public async ChangeConversationName(name: string): Promise<void> {
    let currentConversationId = this.CurrentConversation.conversationID;

    let result = await this.requestsBuilder.ChangeConversationName(name, this.CurrentConversation.conversationID);

    if (!result.isSuccessfull) {
      return;
    }

    this.Conversations.find(x => x.conversationID == currentConversationId).name = name;     
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
    }

    if (data.conversation.isGroup) {

      //we created new group.

      if (data.user.id == this.authService.User.id) {

        if (data.conversation.messages == null) {
          data.conversation.messages = new Array<ChatMessage>();
        }

        this.Conversations = [...this.Conversations, data.conversation];
      } else {

        //someone added new user to existing group

        let conversation = this.Conversations.find(x => x.conversationID == data.conversation.conversationID);
        conversation.participants.push(data.user);
        conversation.participants = [...conversation.participants];
      }
    } else {

      //we created dialog with someone;

      if (data.user.id == this.authService.User.id) {

        if (data.conversation.messages == null) {
          data.conversation.messages = new Array<ChatMessage>();
        }

        this.Conversations = [...this.Conversations, data.conversation];

      } else {

        //someone created dialog with us.

        data.conversation.dialogueUser = data.user;
        this.Conversations = [...this.Conversations, data.conversation];
      }

    }

    //update data about this conversation.

    await this.UpdateExisting(data.conversation);
  }

  public OnRemovedFromGroup(data: RemovedFromGroupModel) {
    //either this client left or creator removed him.

    if (data.userId == this.authService.User.id) {

      this.Conversations.splice(this.Conversations.findIndex(x => x.conversationID == data.conversationId), 1);
    } else {

      let participants = this.Conversations.find(x => x.conversationID == data.conversationId).participants;

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
      --conversation.messagesUnread;
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

  public async UploadImages(files: FileList) {
    if (files.length == 0) {
      return;
    }

    let conversationToSend = this.CurrentConversation.conversationID;

    let result = await this.requestsBuilder.UploadImages(files)

    let response = (<HttpResponse<ServerResponse<UploadFilesResponse>>>result).body;

    if (!response.isSuccessfull) {
      return;
    }

    response.response.uploadedFiles.forEach(
      (file) => {
        let message = this.BuildMessage(null, conversationToSend, true, file);

        //secure chat
        if (this.CurrentConversation.authKeyId) {

          this.connectionManager.SendMessageToSecureChat(
            this.encryptionService.Encrypt(this.CurrentConversation.dialogueUser.id, message),
            message.id,
            this.CurrentConversation.dialogueUser.id, conversationToSend);

        } else {
          this.connectionManager.SendMessage(message, this.CurrentConversation);
        }

        this.CurrentConversation.messages.push(message);
      })
  }

  public UpdateConversationFields(old: ConversationTemplate, New: ConversationTemplate): void {
    old.isMessagingRestricted = New.isMessagingRestricted;
    old.name = New.name;
    old.fullImageUrl = New.fullImageUrl;
    old.thumbnailUrl = New.thumbnailUrl;
  }
}
