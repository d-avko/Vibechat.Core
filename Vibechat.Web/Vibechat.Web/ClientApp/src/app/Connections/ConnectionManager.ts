import * as signalR from "@aspnet/signalr";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ChatMessage } from "../Data/ChatMessage";
import { Cache } from "../Auth/Cache";
import { Injectable, EventEmitter } from "@angular/core";
import { MessageReceivedModel } from "../Shared/MessageReceivedModel";
import { AddedToGroupModel } from "../Shared/AddedToGroupModel";
import { RemovedFromGroupModel } from "../Shared/RemovedFromGroupModel";
import { EmptyModel } from "../Shared/EmptyModel";

@Injectable({
  providedIn: 'root'
})

export class ConnectionManager {
  private connection: signalR.HubConnection;

  public OnMessageReceived: EventEmitter<MessageReceivedModel>;

  public OnAddedToGroup: EventEmitter<AddedToGroupModel>;

  public OnRemovedFromGroup: EventEmitter<RemovedFromGroupModel>;

  public OnDisconnectedNotify: EventEmitter<EmptyModel>;

  public OnConnectingNotify: EventEmitter<EmptyModel>;

  constructor() {
    this.OnMessageReceived = new EventEmitter<MessageReceivedModel>();
    this.OnAddedToGroup = new EventEmitter<AddedToGroupModel>();
    this.OnRemovedFromGroup = new EventEmitter<RemovedFromGroupModel>();
    this.OnDisconnectedNotify = new EventEmitter<EmptyModel>();
    this.OnConnectingNotify = new EventEmitter<EmptyModel>();
  }

  public Start(): void {

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", { accessTokenFactory: () => Cache.JwtToken })
      .build();

    this.connection.start();

    this.OnConnectingNotify.emit(null);
    
    this.connection.onclose(() => this.OnDisconnected())

    this.connection.on("ReceiveMessage", (senderId: string, message: ChatMessage, conversationId: number) => {
      this.OnMessageReceived.emit(new MessageReceivedModel({ senderId: senderId, message: message, conversationId: conversationId }));
    });

    this.connection.on("AddedToGroup", (conversationId: number, userId: string) => {
      this.OnAddedToGroup.emit(new AddedToGroupModel({ conversationId: conversationId, userId: userId}));
    });

    this.connection.on("RemovedFromGroup", (conversationId: number, userId: string) => {
      this.OnRemovedFromGroup.emit(new RemovedFromGroupModel({ conversationId: conversationId, userId: userId }));
    });
  }

  public OnConnected() : void {
    this.connection.send("OnConnected");
  }

  public OnDisconnected(): void {
    this.OnDisconnectedNotify.emit(null);

    this.connection.send("OnDisconnected");

    this.Start();
  }

  public SendMessage(message: ChatMessage, conversation: ConversationTemplate, whoSentId: string) : void {

    if (conversation.isGroup) {

      this.connection.send("SendMessageToGroup", message, whoSentId, conversation.conversationID);

    } else {

      this.connection.send("SendMessageToUser", message, whoSentId, conversation.dialogueUser.id, conversation.conversationID);

    }
  }
}
