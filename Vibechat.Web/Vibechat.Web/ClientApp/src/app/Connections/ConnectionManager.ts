import * as signalR from "@aspnet/signalr";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ChatMessage } from "../Data/ChatMessage";
import { Injectable } from "@angular/core";
import { MessageReceivedModel } from "../Shared/MessageReceivedModel";
import { AddedToGroupModel } from "../Shared/AddedToGroupModel";
import { RemovedFromGroupModel } from "../Shared/RemovedFromGroupModel";
import { UserInfo } from "../Data/UserInfo";
import { ChatsService } from "../Services/ChatsService";
import { MessageReportingService } from "../Services/MessageReportingService";
import { AuthService } from "../Auth/AuthService";
import { DHServerKeyExchangeService } from "../Encryption/DHServerKeyExchange";
import { ChatComponent } from "../Chat/chat.component";
import { UAParser } from "ua-parser-js";
import { DeviceService } from "../Services/DeviceService";

@Injectable({
  providedIn: 'root'
})

export class ConnectionManager {
  private connection: signalR.HubConnection;

  private ConversationsService: ChatsService;

  private DHServerKeyExchangeService: DHServerKeyExchangeService;

  public setConversationsService(service: ChatsService) {
    this.ConversationsService = service;
  }

  public setDHServerKeyExchangeService(service: DHServerKeyExchangeService) {
    this.DHServerKeyExchangeService = service;
  }

  constructor(private messagesService: MessageReportingService,
    private auth: AuthService,
    private device: DeviceService) { }

  public async Start(){

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", { accessTokenFactory: () => this.auth.token })
      .build();

    this.messagesService.OnConnecting();

    await this.connection.start();

    this.InitiateConnections(this.ConversationsService.GetConversationsIds());
    this.messagesService.OnConnected();
  
    this.connection.onclose(() => this.messagesService.OnDisconnected());

    this.connection.on("ReceiveMessage", (senderId: string, message: ChatMessage, conversationId: number, secure: boolean) => {
      this.ConversationsService.OnMessageReceived(new MessageReceivedModel(
        {
          senderId: senderId,
          message: message,
          conversationId:
          conversationId,
          secure: secure
        }));
    });

    this.connection.on("AddedToGroup", (conversation: ConversationTemplate, user: UserInfo) => {
      this.ConversationsService.OnAddedToGroup(new AddedToGroupModel({ conversation: conversation, user: user}));
    });

    this.connection.on("Error", (error: string) => {
      this.messagesService.OnError(error);
    });

    this.connection.on("RemovedFromGroup", (userId: string, conversationId: number) => {
      this.ConversationsService.OnRemovedFromGroup(new RemovedFromGroupModel({ userId: userId, conversationId: conversationId }));
    });

    this.connection.on("MessageDelivered", (msgId: number, clientMessageId: number, conversationId: number) => {
      this.ConversationsService.OnMessageDelivered(msgId, clientMessageId, conversationId);
    });

    this.connection.on("MessageRead", (msgId: number, conversationId: number) => {
      this.ConversationsService.OnMessageRead(msgId, conversationId);
    });

    if (this.device.isSecureChatsSupported) {

      this.connection.on("ReceiveDhParam", async (param: string, sentBy: string, chatId: number) => {
        await this.DHServerKeyExchangeService.OnIntermidiateParamsReceived(param, sentBy, chatId);
      });

      this.connection.on("UserOnline", (user: string) => {
        this.ConversationsService.OnUserOnline(user);
      });
    }
  }

  public SendMessage(message: ChatMessage, conversation: ConversationTemplate) : void {

    if (conversation.isGroup) {

      this.connection.send("SendMessageToGroup", message, conversation.conversationID);

    } else {

      this.connection.send("SendMessageToUser", message, conversation.dialogueUser.id, conversation.conversationID);

    }
  }

  public SendDhParam(param: string, sendTo: string, chatId: number) {
    this.connection.send("SendDhParam", sendTo, param, chatId);
  }

  public SendMessageToSecureChat(encryptedMessage: string, generatedId: number, userId: string, chatId: number) {
    this.connection.send("SendSecureMessage", encryptedMessage, generatedId, userId, chatId);
  }

  public SubsribeToUserOnlineStatusChanges(user: string) {
    return this.connection.invoke<boolean>("SubsribeToUserOnlineStatusChanges", user);
  }

  public UnsubsribeFromUserOnlineStatusChanges(user: string) {
    return this.connection.invoke<boolean>("SubsribeToUserOnlineStatusChanges", user);
  }

  public AddUserToConversation(userId: string, conversation: ConversationTemplate) {
    this.connection.send("AddToGroup", userId, conversation);
  }

  public RemoveConversation(conversation: ConversationTemplate) {
    this.connection.send("RemoveConversation", conversation);
  }


  public RemoveUserFromConversation(userId: string, conversationId: number, IsSelf: boolean) {
    this.connection.send("RemoveFromGroup", userId, conversationId, IsSelf);
  }

  public CreateDialog(user: UserInfo, secure: boolean) {
    if (secure) {
      this.connection.send("CreateDialog", user, secure, this.device.GetDeviceId().toString());
    } else {
      this.connection.send("CreateDialog", user, secure, null);
    }
  }

  public ReadMessage(msgId: number, conversationId: number) {
    this.connection.send("MessageRead", msgId, conversationId);
  }

  public InitiateConnections(conversationIds: Array<number>) : void {
    this.connection.send("ConnectToGroups", conversationIds);
  }
}
