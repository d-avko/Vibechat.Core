import * as signalR from "@aspnet/signalr";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ChatMessage } from "../Data/ChatMessage";
import { Cache } from "../Auth/Cache";
import { Injectable, EventEmitter } from "@angular/core";
import { MessageReceivedModel } from "../Shared/MessageReceivedModel";
import { AddedToGroupModel } from "../Shared/AddedToGroupModel";
import { RemovedFromGroupModel } from "../Shared/RemovedFromGroupModel";
import { EmptyModel } from "../Shared/EmptyModel";
import { ConversationIdsFactory } from "../Data/ConversationIdsFactory";
import { ChatComponent } from "../Chat/chat.component";
import { MessageReceivedDelegate } from "../Delegates/MessageReceivedDelegate";

@Injectable({
  providedIn: 'root'
})

export class ConnectionManager {
  private connection: signalR.HubConnection;

  private convIdsFactory: ConversationIdsFactory;

  private onMessageReceived: MessageReceivedDelegate;

  constructor() {

  }

  public Start(
    convIdsFactory: ConversationIdsFactory,
    onMessageReceived: MessageReceivedDelegate): void {

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", { accessTokenFactory: () => Cache.JwtToken })
      .build();

    this.connection.start().then(() => this.InitiateConnections(convIdsFactory()));

    this.onMessageReceived = onMessageReceived;

   // this.OnConnectingNotify.emit(null);
    
   // this.connection.onclose(() => this.OnDisconnected());

    this.convIdsFactory = convIdsFactory;

    this.connection.on("ReceiveMessage", (senderId: string, message: ChatMessage, conversationId: number) => {
      this.onMessageReceived(new MessageReceivedModel({ senderId: senderId, message: message, conversationId: conversationId }));
    });

    //this.connection.on("AddedToGroup", (conversationId: number, userId: string) => {
    //  this.OnAddedToGroup.emit(new AddedToGroupModel({ conversationId: conversationId, userId: userId}));
    //});

    //this.connection.on("RemovedFromGroup", (conversationId: number, userId: string) => {
    //  this.OnRemovedFromGroup.emit(new RemovedFromGroupModel({ conversationId: conversationId, userId: userId }));
    //});

  }

  public OnConnected() : void {
    this.connection.send("OnConnected");
  }

  //public OnDisconnected(): void {
  //  this.OnDisconnectedNotify.emit(null);

  //  this.connection.send("OnDisconnected");

  //  this.Start(this.convIdsFactory);
  //}

  public SendMessage(message: ChatMessage, conversation: ConversationTemplate, whoSentId: string) : void {

    if (conversation.isGroup) {

      this.connection.send("SendMessageToGroup", message, whoSentId, conversation.conversationID);

    } else {

      this.connection.send("SendMessageToUser", message, whoSentId, conversation.dialogueUser.id, conversation.conversationID);

    }
  }

  public InitiateConnections(conversationIds: Array<number>) : void {
    this.connection.send("ConnectToGroups", conversationIds);
  }
}
