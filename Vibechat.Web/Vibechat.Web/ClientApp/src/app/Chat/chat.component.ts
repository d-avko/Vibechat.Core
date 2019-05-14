import { Component, ViewChild, OnInit } from "@angular/core";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { Cache } from "../Auth/Cache";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { MatSnackBar } from "@angular/material";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { Router } from "@angular/router";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { ChatMessage } from "../Data/ChatMessage";
import { trigger, state, style, transition, animate } from "@angular/animations";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { ConnectionManager } from "../Connections/ConnectionManager";
import { MessageReceivedModel } from "../Shared/MessageReceivedModel";
import { AddedToGroupModel } from "../Shared/AddedToGroupModel";
import { RemovedFromGroupModel } from "../Shared/RemovedFromGroupModel";
import { EmptyModel } from "../Shared/EmptyModel";
import { MessagesComponent } from "../Conversation/Messages/messages.component";
import { UserInfo } from "../Data/UserInfo";

@Component({
  selector: 'chat-root',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css'],
  animations: [
    trigger('slideIn', [
      state('*', style({ 'overflow-y': 'hidden' })),
      state('void', style({ 'overflow-y': 'hidden' })),
      transition('* => void', [
        style({ height: '*' }),
        animate(250, style({ height: 0 }))
      ]),
      transition('void => *', [
        style({ height: '0' }),
        animate(250, style({ height: '*' }))
      ])
    ])
  ]
})
export class ChatComponent implements OnInit {

  //This user conversations

  public Conversations: Array<ConversationTemplate>;

  public CurrentConversation: ConversationTemplate;

  public CurrentUser: UserInfo;

  //pop-up that will inform user of errors.

  protected snackbar: SnackBarHelper;

  protected router: Router;

  protected connectionManager: ConnectionManager;



  protected requestsBuilder: ApiRequestsBuilder

  public formatter: ConversationsFormatter;

  public static MessagesBufferLength: number = 50;



  public IsMessagesLoading: boolean = false;

  public IsAuthenticated: boolean;

  public IsConversationHistoryEnd: boolean = false;

  @ViewChild(MessagesComponent) messages: MessagesComponent;

  constructor(
    requestsBuilder: ApiRequestsBuilder,
    snackbar: MatSnackBar,
    router: Router,
    formatter: ConversationsFormatter,
    connectionManager: ConnectionManager) {

    this.snackbar = new SnackBarHelper(snackbar);

    this.router = router;

    this.requestsBuilder = requestsBuilder;

    this.Conversations = new Array<ConversationTemplate>();

    this.formatter = formatter;

    this.IsAuthenticated = Cache.IsAuthenticated;

    if (this.IsAuthenticated) {

      this.connectionManager = connectionManager;

      this.CurrentUser = Cache.UserCache;

      //this.connectionManager.OnAddedToGroup.subscribe((res) => this.OnAddedToGroup(res));
      //this.connectionManager.OnConnectingNotify.subscribe((res) => this.OnConnecting(res));
      //this.connectionManager.OnMessageReceived.subscribe((res) => this.OnMessageReceived(res));
      //this.connectionManager.OnRemovedFromGroup.subscribe((res) => this.OnRemovedFromGroup(res));
    } 

  }
  ngOnInit(): void {
    if (this.IsAuthenticated) {
      this.UpdateConversations();
    }
  }

  public OnSendMessage(message: string) {

    this.connectionManager.SendMessage(
      new ChatMessage(
        {
          messageContent: message,
          isAttachment: false,
          user: this.CurrentUser,
          conversationID: this.CurrentConversation.conversationID
        }), this.CurrentConversation, this.CurrentUser.id);
  }

  public OnConnecting(data: EmptyModel) {
    this.snackbar.openSnackBar("Connecting...");
  }

  public OnMessageReceived(data: MessageReceivedModel): void {
    data.message.timeReceived = new Date(<string>data.message.timeReceived);

    let conversation = this.Conversations
      .find(x => x.conversationID == data.conversationId);

    let newLength = conversation.messages.push(data.message);

    conversation.messages = [...conversation.messages];

    this.messages.ScrollToMessage(newLength);
  }

  public OnAddedToGroup(data: AddedToGroupModel): void {

  }

  public OnRemovedFromGroup(data: RemovedFromGroupModel) {

  }

  public OnDisconnected(data: EmptyModel) {
    this.snackbar.openSnackBar("Disconnected. Retrying...");
  }

  public OnUpdateMessages() {

    if (!this.IsMessagesLoading && !this.IsConversationHistoryEnd) {
      this.IsMessagesLoading = true;

      this.requestsBuilder.GetConversationMessages(
        this.CurrentConversation.messages.length,
        ChatComponent.MessagesBufferLength,
        this.CurrentConversation.conversationID, Cache.JwtToken)

        .subscribe((result) => {

          if (!result) {
            this.snackbar.openSnackBar("Failed to update messages for " + this.CurrentConversation.name);
            this.IsMessagesLoading = false;
            return;
          }

          //server sent zero messages, we reached end of our history.

          if (result.response.messages == null || result.response.messages.length == 0) {
            this.IsMessagesLoading = false;
            this.IsConversationHistoryEnd = true;
            return;
          }

          result.response.messages = result.response.messages.sort(this.MessagesSortFunc);

          result.response.messages.forEach((x) => x.timeReceived = new Date(<string>x.timeReceived));

          //append old messages to new ones.
          this.CurrentConversation.messages = result.response.messages.concat(this.CurrentConversation.messages);
          this.IsMessagesLoading = false;

          this.messages.ScrollToMessage(result.response.messages.length);
        }
      )
    }

  }

  public OnDeleteMessages(SelectedMessages: Array<ChatMessage>) {
    this.requestsBuilder.DeleteMessages(SelectedMessages, this.CurrentConversation.conversationID, Cache.JwtToken)
      .subscribe((result) => this.OnMessagesDeleted(result, SelectedMessages))
  }

  public OnMessagesDeleted(response: ServerResponse<string>, SelectedMessages: Array<ChatMessage>) {
    if (!response.isSuccessfull) {
      this.snackbar.openSnackBar("Failed to delete messages. Reason: " + response.errorMessage);
      return;
    }

    //delete messages locally

    this.CurrentConversation.messages = this.CurrentConversation
      .messages
      .filter(msg => SelectedMessages.findIndex(selected => selected.id == msg.id) == -1);

    this.messages.ScrollToStart();

    SelectedMessages.splice(0, SelectedMessages.length);
  }

  public UpdateConversations() {
    this.requestsBuilder.UpdateConversationsRequest(Cache.JwtToken, this.CurrentUser.id)
      .subscribe(
        (response) => {

          if (!response.isSuccessfull) {
            this.snackbar.openSnackBar("Failed to update conversations. Reason: " + response.errorMessage);
            return;
          }

          //parse string date to js Date

          response.response.conversations
            .forEach((conversation) => conversation.messages.forEach(msg => msg.timeReceived = new Date(<string>msg.timeReceived)))

          this.Conversations = response.response.conversations;

          //Initiate signalR group connection

          this.connectionManager.Start(
            () => this.Conversations.map((x) => x.conversationID),
            (data) => this.OnMessageReceived(data));
        }
      )
  }

  public IsCurrentConversation(conversation: ConversationTemplate): boolean {
    if (this.CurrentConversation == null)
      return false;

    return this.CurrentConversation.conversationID == conversation.conversationID;
  }

  public GetCurrentDialogueUserImageUrl() : string {
    return this.CurrentConversation.dialogueUser.imageUrl;
  }

  public GetCurrentConversationImageUrl(): string {
    return this.CurrentConversation.imageUrl;
  }

  public IsConversationSelected(): boolean {
    return this.CurrentConversation != null;
  }


  public GetThisUserImageUrl(): string {
    return this.CurrentUser.imageUrl;
  }


  public ChangeConversation(conversation: ConversationTemplate): void {
    if (conversation == this.CurrentConversation) {
      this.CurrentConversation = null;
      return;
    }

    this.CurrentConversation = conversation;

    this.IsConversationHistoryEnd = false;
  }

  public OnUploadImages(files: FileList) {
    let conversationToSend = this.CurrentConversation.conversationID;

    this.requestsBuilder.UploadImages(files, Cache.JwtToken).subscribe((result) =>
    {
      if (!result.isSuccessfull) {
        this.snackbar.openSnackBar(result.errorMessage);
        return;
      }

      result.response.uploadedFiles.forEach(
        (file) => this.connectionManager.SendMessage(
          new ChatMessage(
            {
              messageContent: null,
              isAttachment: true,
              user: this.CurrentUser,
              conversationID: conversationToSend,
              attachmentInfo: file
            }), this.CurrentConversation, this.CurrentUser.id)
      )
    }
    )
  }

  private MessagesSortFunc(left: ChatMessage, right: ChatMessage): number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }

}
