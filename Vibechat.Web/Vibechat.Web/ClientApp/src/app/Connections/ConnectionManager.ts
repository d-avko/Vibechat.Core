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
import { AddedToConversationDelegate } from "../Delegates/AddedToConversationDelegate";
import { UserInfo } from "../Data/UserInfo";
import { ErrorDelegate } from "../Delegates/ErrorDelegate";
import { RemovedFromGroupDelegate } from "../Delegates/RemovedFromGroupDelegate";

@Injectable({
  providedIn: 'root'
})

export class ConnectionManager {
  private connection: signalR.HubConnection;

  private convIdsFactory: ConversationIdsFactory;

  private onMessageReceived: MessageReceivedDelegate;

  private onAddedToGroup: AddedToConversationDelegate;

  private onError: ErrorDelegate;

  private OnRemovedFromGroupDelegate: RemovedFromGroupDelegate;

  constructor(
    convIdsFactory: ConversationIdsFactory,
    onMessageReceived: MessageReceivedDelegate,
    onAddedToGroup: AddedToConversationDelegate,
    onError: ErrorDelegate,
    OnRemovedFromGroupDelegate : RemovedFromGroupDelegate) {

    this.onMessageReceived = onMessageReceived;
    this.onAddedToGroup = onAddedToGroup;
    this.convIdsFactory = convIdsFactory;
    this.onError = onError;
    this.OnRemovedFromGroupDelegate = OnRemovedFromGroupDelegate;
  }

  public Start(): void {

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", { accessTokenFactory: () => Cache.JwtToken })
      .build();

    this.connection.start().then(
      () => {
        this.InitiateConnections(this.convIdsFactory());
        this.OnConnected();
      });
  
    this.connection.onclose(() => this.OnDisconnected());


    this.connection.on("ReceiveMessage", (senderId: string, message: ChatMessage, conversationId: number) => {
      this.onMessageReceived(new MessageReceivedModel({ senderId: senderId, message: message, conversationId: conversationId }));
    });

    this.connection.on("AddedToGroup", (conversation: ConversationTemplate, user: UserInfo) => {
      this.onAddedToGroup(new AddedToGroupModel({ conversation: conversation, user: user}));
    });

    this.connection.on("Error", (error: string) => {
      this.onError(error);
    });

    this.connection.on("RemovedFromGroup", (userId: string, conversationId: number) => {
      this.OnRemovedFromGroupDelegate(new RemovedFromGroupModel({ userId: userId, conversationId: conversationId }));
    });

    //this.connection.on("RemovedFromGroup", (conversationId: number, userId: string) => {
    //  this.OnRemovedFromGroup.emit(new RemovedFromGroupModel({ conversationId: conversationId, userId: userId }));
    //});

  }

  public OnConnected() : void {
    this.connection.send("OnConnected");
  }

  public OnDisconnected(): void {
    this.connection.send("OnDisconnected");

    this.Start();
  }

  public SendMessage(message: ChatMessage, conversation: ConversationTemplate) : void {

    if (conversation.isGroup) {

      this.connection.send("SendMessageToGroup", message, conversation.conversationID);

    } else {

      this.connection.send("SendMessageToUser", message, conversation.dialogueUser.id, conversation.conversationID);

    }
  }

  public AddUserToConversation(userId: string, whoAddsId: string, conversation: ConversationTemplate) {
    this.connection.send("AddToGroup", userId, whoAddsId, conversation);
  }

  public RemoveUserFromConversation(userId: string, conversationId: number) {
    this.connection.send("RemoveFromGroup", userId, conversationId);
  }

  public InitiateConnections(conversationIds: Array<number>) : void {
    this.connection.send("ConnectToGroups", conversationIds);
  }
}
