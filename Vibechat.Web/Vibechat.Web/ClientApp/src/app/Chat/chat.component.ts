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
import { element } from "protractor";

@Component({
  selector: 'chat-root',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent {

  //This user conversations

  public Conversations: Array<ConversationTemplate>;

  public CurrentConversation: ConversationTemplate;

  //pop-up that will inform user of errors.

  protected snackbar: SnackBarHelper;

  protected router: Router;

  protected requestsBuilder: ApiRequestsBuilder;

  public formatter: ConversationsFormatter;

  public static MessagesBufferLength: number = 50;

  public IsMessagesLoading: boolean = false;

  public IsAuthenticated: boolean;

  @ViewChild(CdkVirtualScrollViewport) viewport: CdkVirtualScrollViewport;


  constructor(requestsBuilder: ApiRequestsBuilder, snackbar: MatSnackBar, router: Router, formatter: ConversationsFormatter) {

    this.snackbar = new SnackBarHelper(snackbar);

    this.router = router;

    this.requestsBuilder = requestsBuilder;

    this.Conversations = new Array<ConversationTemplate>();

    this.formatter = formatter;

    this.IsAuthenticated = Cache.IsAuthenticated;

    if (this.IsAuthenticated) {

      this.UpdateConversations();
    } 

  }

  public OnMessagesScrolled(messageIndex: number) {
    // user scrolled to last loaded message, load more messages

    if (this.IsMessagesLoading) {
      return;
    }

    if (messageIndex == 0) {
      this.IsMessagesLoading = true;

      this.requestsBuilder.GetConversationMessages(this.CurrentConversation.messages.length, ChatComponent.MessagesBufferLength, this.CurrentConversation.conversationID, Cache.JwtToken)
        .subscribe((result) => {

          if (!result) {
            this.snackbar.openSnackBar("Failed to update messages for " + this.CurrentConversation.name);
            this.IsMessagesLoading = false;
            return;
          }

          result.response.messages = result.response.messages.sort(this.MessagesSortFunc);

          if (result.response.messages == null) {
            this.IsMessagesLoading = false;
            return;
          }

          //append old messages to new ones.
          this.CurrentConversation.messages = result.response.messages.concat(this.CurrentConversation.messages);
          this.IsMessagesLoading = false;
        }
      )
    }
  }

  public OnMessageAdded(): void {
    requestAnimationFrame(() => {
      this.viewport.scrollToIndex(this.CurrentConversation.messages.length, 'smooth');
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

    this.Conversations = response.response.conversations;
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

  public IsImage(attachment: MessageAttachment) {

    return attachment.attachmentKind == AttachmentKinds.Image;

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
