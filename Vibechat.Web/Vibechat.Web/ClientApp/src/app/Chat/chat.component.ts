import { Component, ViewChildren, QueryList, ElementRef, ViewChild } from "@angular/core";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ConversationResponse } from "../ApiModels/ConversationResponse";
import { Cache } from "../Auth/Cache";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { MatSnackBar } from "@angular/material";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { Router } from "@angular/router";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { ConversationsFormatter } from "../Data/ConversationsFormatter";
import { MessageAttachment } from "../Data/MessageAttachment";
import { AttachmentKinds } from "../Data/AttachmentKinds";
import { ChatMessage } from "../Data/ChatMessage";
import { CdkVirtualScrollViewport } from "@angular/cdk/scrolling";
import { trigger, state, style, transition, animate } from "@angular/animations";

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
export class ChatComponent {

  //This user conversations

  public Conversations: Array<ConversationTemplate>;

  public CurrentConversation: ConversationTemplate;

  //pop-up that will inform user of errors.

  public SelectedMessages: Array<ChatMessage>;

  protected snackbar: SnackBarHelper;

  protected router: Router;

  protected requestsBuilder: ApiRequestsBuilder;

  public formatter: ConversationsFormatter;

  public static MessagesBufferLength: number = 50;

  public static MessagesToScrollForGoBackButtonToShowUp: number = 60;

  public IsMessagesLoading: boolean = false;

  public IsAuthenticated: boolean;

  public IsConversationHistoryEnd: boolean = false;

  public IsScrollingAssistNeeded: boolean = false;

  @ViewChild(CdkVirtualScrollViewport) viewport: CdkVirtualScrollViewport;


  constructor(requestsBuilder: ApiRequestsBuilder, snackbar: MatSnackBar, router: Router, formatter: ConversationsFormatter) {

    this.snackbar = new SnackBarHelper(snackbar);

    this.router = router;

    this.requestsBuilder = requestsBuilder;

    this.Conversations = new Array<ConversationTemplate>();

    this.formatter = formatter;

    this.IsAuthenticated = Cache.IsAuthenticated;

    this.SelectedMessages = new Array<ChatMessage>();

    if (this.IsAuthenticated) {

      this.UpdateConversations();
    } 

  }

  public OnMessagesScrolled(messageIndex: number) {
    // user scrolled to last loaded message, load more messages

    if (messageIndex == 0 && !this.IsMessagesLoading) {
      this.IsMessagesLoading = true;

      this.requestsBuilder.GetConversationMessages(this.CurrentConversation.messages.length, ChatComponent.MessagesBufferLength, this.CurrentConversation.conversationID, Cache.JwtToken)
        .subscribe((result) => {

          if (!result) {
            this.snackbar.openSnackBar("Failed to update messages for " + this.CurrentConversation.name);
            this.IsMessagesLoading = false;
            return;
          }

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
          this.ScrollToMessage(result.response.messages.length);
        }
      )
    }

    // if user scrolled <see cref="MessagesToScrollForGoBackButtonToShowUp"/> messages, show 'Go to start' button

    if (this.CurrentConversation.messages.length - messageIndex > ChatComponent.MessagesToScrollForGoBackButtonToShowUp) {
      this.IsScrollingAssistNeeded = true;
    } else {
      this.IsScrollingAssistNeeded = false;
    }
  }

  public DeleteMessages() {
    this.requestsBuilder.DeleteMessages(this.SelectedMessages, this.CurrentConversation.conversationID, Cache.JwtToken)
    .subscribe((result) => this.OnMessagesDeleted(result))
  }

  public OnMessagesDeleted(response: ServerResponse<string>) {
    if (!response.isSuccessfull) {
      this.snackbar.openSnackBar("Failed to delete messages. Reason: " + response.errorMessage);
      return;
    }

    //delete messages locally

    this.CurrentConversation.messages = this.CurrentConversation
      .messages
      .filter(msg => this.SelectedMessages.findIndex(selected => selected.id == msg.id) == -1);

    this.SelectedMessages.splice(0, this.SelectedMessages.length);
  }

  public ScrollToStart(): void {
    this.ScrollToMessage(this.CurrentConversation.messages.length);
  }

  public ScrollToMessage(index: number): void {
    requestAnimationFrame(() => {
      this.viewport.scrollToIndex(index, 'smooth');
    });
  }

  public UpdateConversations() {
    this.requestsBuilder.UpdateConversationsRequest(Cache.JwtToken, Cache.UserCache.id)
      .subscribe((result) => this.OnConversationsUpdated(result))
  }

  public OnConversationsUpdated(response: ServerResponse<ConversationResponse>): void {

    if (!response.isSuccessfull) {
      this.snackbar.openSnackBar("Failed to update conversations. Reason: " + response.errorMessage);
      return;
    }

    //parse string date to js Date

    response.response.conversations
    .forEach((conversation) => conversation.messages.forEach(msg => msg.timeReceived = new Date(<string>msg.timeReceived)))

    this.Conversations = response.response.conversations
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

  public IsCurrentConversationGroup(): boolean {
    return this.CurrentConversation.isGroup;
  }

  public IsText(message: ChatMessage) {
    return !message.isAttachment;
  }

  public IsMessageSelected(message: ChatMessage) : boolean {
    return this.SelectedMessages.find(x => x.id == message.id) != null;
  }

  public SelectMessage(message: ChatMessage) : void {

    let messageIndex = this.SelectedMessages.findIndex(x => x.id == message.id);

    if (messageIndex == -1) {

      this.SelectedMessages.push(message);

    } else {

      this.SelectedMessages.splice(messageIndex, 1);
    }
  }

  public IsImage(message: ChatMessage): boolean {

    if (!message.isAttachment) {
      return false;
    }

    return message.attachmentInfo.attachmentKind == AttachmentKinds.Image;

  }

  public IsLastMessage(message: ChatMessage) : boolean {
    return this.CurrentConversation.messages.findIndex((x) => x.id == message.id) == 0;
  }

  public GetCurrentConversationName(): string {

    if (this.CurrentConversation.name == '' || this.CurrentConversation.name == null) {
      return this.CurrentConversation.dialogueUser.userName;
    }

    return this.CurrentConversation.name;
  }

  public GetThisUserImageUrl(): string {
    return Cache.UserCache.imageUrl;
  }

  public SendMessage() {
    //TODO
  }

  public GetCurrentConversationMembersFormatted(): string {
    return this.formatter.GetConversationMembersFormatted(this.CurrentConversation);
  }

  public ChangeConversation(conversation: ConversationTemplate): void {
    if (conversation == this.CurrentConversation) {
      this.CurrentConversation = null;
      return;
    }

    this.CurrentConversation = conversation;

    this.IsConversationHistoryEnd = false;

    // 1 message is sent by server on first request of UpdateConversatoins() in messages field
    //should use bool var there instead.


    if (conversation.messages.length == 1) {
      //forcibly update messages
      this.OnMessagesScrolled(0);
    }
  }

  public IsFirstMessageInSequence(message: ChatMessage) : boolean {
    let messageIndex = this.CurrentConversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == 0) {
      return true;
    }

    return this.CurrentConversation.messages[messageIndex - 1].user.userName != message.user.userName;

  }

  public IsPreviousMessageOnAnotherDay(message: ChatMessage): boolean {
    let messageIndex = this.CurrentConversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == 0) {
      return true;
    }

    let thisMessageDay = new Date(message.timeReceived).getDay();
    let previousMessageDay = new Date(this.CurrentConversation.messages[messageIndex - 1].timeReceived).getDay();

    return thisMessageDay != previousMessageDay;
  }

  public GetMessagesDateStripFormatted(message: ChatMessage) : string {
    let messageIndex = this.CurrentConversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == 0) {
      return this.formatter.GetMessagesDateStripFormatted(message)
    }

    return this.formatter.GetMessagesDateStripFormatted(this.CurrentConversation.messages[messageIndex]);
  }

  private MessagesSortFunc(left: ChatMessage, right: ChatMessage) : number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }
}
