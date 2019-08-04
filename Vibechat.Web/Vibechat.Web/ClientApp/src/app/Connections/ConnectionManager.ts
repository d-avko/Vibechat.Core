import * as signalR from "@aspnet/signalr";
import {Chat} from "../Data/Chat";
import {Message} from "../Data/Message";
import {Injectable} from "@angular/core";
import {MessageReceivedModel} from "../Shared/MessageReceivedModel";
import {AddedToGroupModel} from "../Shared/AddedToGroupModel";
import {RemovedFromGroupModel} from "../Shared/RemovedFromGroupModel";
import {UserInfo} from "../Data/UserInfo";
import {ChatsService} from "../Services/ChatsService";
import {MessageReportingService} from "../Services/MessageReportingService";
import {AuthService} from "../Auth/AuthService";
import {DHServerKeyExchangeService} from "../Encryption/DHServerKeyExchange";
import {DeviceService} from "../Services/DeviceService";
import {TypingService} from "../Services/TypingService";
import {ChatRole} from "../Roles/ChatRole";

export enum BanEvent {
  Banned = 0,
  Unbanned = 1
}

@Injectable({
  providedIn: 'root'
})

export class ConnectionManager {
  private connection: signalR.HubConnection;

  private chats: ChatsService;

  private DHServerKeyExchangeService: DHServerKeyExchangeService;

  public setConversationsService(service: ChatsService) {
    this.chats = service;
  }

  public setDHServerKeyExchangeService(service: DHServerKeyExchangeService) {
    this.DHServerKeyExchangeService = service;
  }

  constructor(private messagesService: MessageReportingService,
    private auth: AuthService,
    private device: DeviceService,
    private typing: TypingService) { }

  public async Start(){

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl("/hubs/chat", { accessTokenFactory: () => this.auth.token })
      .build();

    this.messagesService.OnConnecting();

    await this.connection.start();

    this.InitiateConnections(this.chats.GetConversationsIds());
    this.messagesService.OnConnected();

    this.connection.onclose(() => {
      this.messagesService.OnDisconnected();

      setTimeout(() => this.Start(), 1000);
    });

    this.connection.on("ReceiveMessage", (senderId: string, message: Message, conversationId: number, secure: boolean) => {
      this.chats.OnMessageReceived(new MessageReceivedModel(
        {
          senderId: senderId,
          message: message,
          conversationId:
          conversationId,
          secure: secure
        }));
    });

    this.connection.on("AddedToGroup", async (chatId: number, user: UserInfo) => {
      await this.chats.OnAddedToGroup(new AddedToGroupModel({ chatId: chatId, user: user}));
    });

    this.connection.on("Error", (error: string) => {
      this.messagesService.OnError(error);
    });

    this.connection.on("RemovedFromGroup", (userId: string, conversationId: number) => {
      this.chats.OnRemovedFromGroup(new RemovedFromGroupModel({ userId: userId, conversationId: conversationId }));
    });

    this.connection.on("MessageDelivered", (msgId: number, clientMessageId: number, conversationId: number) => {
      this.chats.OnMessageDelivered(msgId, clientMessageId, conversationId);
    });

    this.connection.on("MessageRead", (msgId: number, conversationId: number) => {
      this.chats.OnMessageRead(msgId, conversationId);
    });

    this.connection.on("ReceiveDhParam", async (param: string, sentBy: string, chatId: number) => {
      await this.DHServerKeyExchangeService.OnIntermidiateParamsReceived(param, sentBy, chatId);
    });

    this.connection.on("UserOnline", (user: string) => {
      this.chats.OnUserOnline(user);
    });

    this.connection.on("Typing", (userId: string, userFirstName: string, chatId: number) => {
      this.typing.OnTyping(userFirstName, userId, chatId);
    });

    this.connection.on("Blocked", (blockedBy: string, banType: BanEvent) => {
      this.chats.OnBlocked(blockedBy, banType);
    });

    this.connection.on("BlockedInChat", (chatId: number, userId: string, banType: BanEvent) => {
      this.chats.OnBannedInChat(chatId, userId, banType);
    });

    this.connection.on("UserRoleChanged", (userId: string, chatId: number, newRole: ChatRole) => {
      this.chats.OnUserRoleChanged(chatId, userId, newRole);
    });
  }

  public SendMessage(message: Message, conversation: Chat) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return Promise.resolve(0);
    }

    if (conversation.isGroup) {

      return this.connection.invoke<number>("SendMessageToGroup", message, conversation.id);

    } else {

      return this.connection.invoke<number>("SendMessageToUser", message, conversation.dialogueUser.id, conversation.id);

    }
  }

  public async ChangeUserRole(chatId: number, userId: string, newRole: ChatRole) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    return await this.connection.invoke<boolean>("ChangeUserRole", userId, chatId, newRole);
  }

  public SendDhParam(param: string, sendTo: string, chatId: number) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    this.connection.send("SendDhParam", sendTo, param, chatId);
  }

  public SendMessageToSecureChat(encryptedMessage: string, userId: string, chatId: number) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return Promise.resolve(0);
    }

    return this.connection.invoke<number>("SendSecureMessage", encryptedMessage, userId, chatId);
  }

  public SubsribeToUserOnlineStatusChanges(user: string) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return Promise.resolve(false);
    }

    return this.connection.invoke<boolean>("SubsribeToUserOnlineStatusChanges", user);
  }

  public UnsubsribeFromUserOnlineStatusChanges(user: string) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return Promise.resolve(false);
    }

    return this.connection.invoke<boolean>("SubsribeToUserOnlineStatusChanges", user);
  }

  public async AddUserToConversation(userId: string, conversation: Chat) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    return await this.connection.invoke<boolean>("AddToGroup", userId, conversation.id);
  }

  public RemoveConversation(conversation: Chat) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    this.connection.send("RemoveConversation", conversation.id);
  }


  public RemoveUserFromConversation(userId: string, conversationId: number, IsSelf: boolean) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    this.connection.send("RemoveFromGroup", userId, conversationId, IsSelf);
  }

  public CreateDialog(user: UserInfo, secure: boolean) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    if (secure) {
      this.connection.send("CreateDialog", user, secure, this.device.GetDeviceId().toString());
    } else {
      this.connection.send("CreateDialog", user, secure, null);
    }
  }

  public ReadMessage(msgId: number, conversationId: number) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    return this.connection.invoke<boolean>("MessageRead", msgId, conversationId);
  }

  public SendTyping(chatId:number) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    this.connection.send("OnTyping", chatId);
  }

  public async BlockUser(userId: string, banType: BanEvent) : Promise<boolean> {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    return this.connection.invoke<boolean>("BlockUser", userId, banType);
  }

  public async BlockUserInChat(userId: string, chatId: number, banType: BanEvent) {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    return this.connection.invoke<boolean>("BlockUserInChat", userId, chatId, banType);
  }

  public InitiateConnections(conversationIds: Array<number>): void {
    if (this.connection.state != signalR.HubConnectionState.Connected) {
      this.messagesService.OnSendWhileDisconnected();
      return;
    }

    this.connection.send("ConnectToGroups", conversationIds);
  }
}
