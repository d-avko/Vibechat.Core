import * as signalR from "@aspnet/signalr";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ChatMessage } from "../Data/ChatMessage";
import { Injectable } from "@angular/core";
import { MessageReceivedModel } from "../Shared/MessageReceivedModel";
import { AddedToGroupModel } from "../Shared/AddedToGroupModel";
import { RemovedFromGroupModel } from "../Shared/RemovedFromGroupModel";
import { UserInfo } from "../Data/UserInfo";
import { ConversationsService } from "../Services/ConversationsService";
import { MessageReportingService } from "../Services/MessageReportingService";
import { AuthService } from "../Auth/AuthService";

@Injectable({
  providedIn: 'root'
})

export class ConnectionManager {
  private connection: signalR.HubConnection;

  private ConversationsService: ConversationsService;

  public setConversationsService(service: ConversationsService) {
    this.ConversationsService = service;
  }

  constructor(private messagesService: MessageReportingService, private auth: AuthService) {}

  public Start(): void {

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", { accessTokenFactory: () => this.auth.token })
      .build();

    this.messagesService.OnConnecting();

    this.connection.start().then(
      () => {
        this.InitiateConnections(this.ConversationsService.GetConversationsIds());
        this.messagesService.OnConnected();
      });
  
    this.connection.onclose(() => this.messagesService.OnDisconnected());


    this.connection.on("ReceiveMessage", (senderId: string, message: ChatMessage, conversationId: number) => {
      this.ConversationsService.OnMessageReceived(new MessageReceivedModel({ senderId: senderId, message: message, conversationId: conversationId }));
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
  }

  public SendMessage(message: ChatMessage, conversation: ConversationTemplate) : void {

    if (conversation.isGroup) {

      this.connection.send("SendMessageToGroup", message, conversation.conversationID);

    } else {

      this.connection.send("SendMessageToUser", message, conversation.dialogueUser.id, conversation.conversationID);

    }
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

  public CreateDialog(user: UserInfo) {
    this.connection.send("CreateDialog", user);
  }

  public ReadMessage(msgId: number, conversationId: number) {
    this.connection.send("MessageRead", msgId, conversationId);
  }

  public InitiateConnections(conversationIds: Array<number>) : void {
    this.connection.send("ConnectToGroups", conversationIds);
  }
}
