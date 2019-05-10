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

@Component({
  selector: 'chat-root',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent {

  public Conversations: Array<ConversationTemplate>;

  public Messages: Array<ChatMessage>;

  protected snackbar: SnackBarHelper;

  protected router: Router;

  protected requestsBuilder: ApiRequestsBuilder;

  public formatter: ConversationsFormatter;

  public static MessagesBufferLength: number = 50;

  @ViewChild(CdkVirtualScrollViewport) viewport: CdkVirtualScrollViewport;


  constructor(requestsBuilder: ApiRequestsBuilder, snackbar: MatSnackBar, router: Router, formatter: ConversationsFormatter) {

    this.snackbar = new SnackBarHelper(snackbar);
    this.router = router;
    this.requestsBuilder = requestsBuilder;
    this.Conversations = new Array<ConversationTemplate>();
    this.formatter = formatter;
    this.Messages = new Array<ChatMessage>();
    
    if (!Cache.IsAuthenticated) {

      this.router.navigateByUrl('/login');

    } else {
      this.UpdateConversations();
    }

  }

  public OnMessageAdded(): void {
    requestAnimationFrame(() => {
      this.viewport.scrollToIndex(this.Messages.length, 'smooth');
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
    if (Cache.CurrentConversation == null)
      return false;

    return Cache.CurrentConversation.conversationID == conversation.conversationID;
  }

  public GetCurrentDialogueUserImageUrl() : string {
    return Cache.CurrentConversation.dialogueUser.imageUrl;
  }

  public GetCurrentConversationImageUrl(): string {
    return Cache.CurrentConversation.imageUrl;
  }

  public IsConversationSelected(): boolean {
    return Cache.CurrentConversation != null;
  }

  public IsCurrentConversationGroup(): boolean {
    return Cache.CurrentConversation.isGroup;
  }

  public IsImage(attachment: MessageAttachment) {

    return attachment.attachmentKind == AttachmentKinds.Image;

  }

  public GetCurrentConversationName(): string {

    if (Cache.CurrentConversation.name == '' || Cache.CurrentConversation.name == null) {
      return Cache.CurrentConversation.dialogueUser.userName;
    }

    return Cache.CurrentConversation.name;
  }

  public GetThisUserImageUrl(): string {
    return Cache.UserCache.imageUrl;
  }

  public SendMessage() {
    //TODO
  }

  public GetCurrentConversationMembersFormatted(): string {
    return this.formatter.GetConversationMembersFormatted(Cache.CurrentConversation);
  }

  public ChangeConversation(conversation: ConversationTemplate): void {
    if (conversation == Cache.CurrentConversation) {
      Cache.CurrentConversation = null;
      return;
    }

    // 1 message is sent by server on first request of UpdateConversatoins() in messages field

    if (conversation.messages.length == 1) {
      this.requestsBuilder.GetConversationMessages(0, ChatComponent.MessagesBufferLength, conversation.conversationID, Cache.JwtToken)
        .subscribe((result) => {

          if (!result) {
            this.snackbar.openSnackBar("Failed to update messages for " + conversation.name);
            return;
          }

          result.response.messages = result.response.messages.sort(this.MessagesSortFunc);

          conversation.messages = result.response.messages;

          this.Messages = this.Messages.concat(result.response.messages);

          this.OnMessageAdded();
        }
      )
    }

    Cache.CurrentConversation = conversation;
  }

  public IsFirstMessageInSequence(message: ChatMessage) : boolean {
    let messageIndex = Cache.CurrentConversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == 0) {
      return true;
    }

    return Cache.CurrentConversation.messages[messageIndex - 1].user.userName != message.user.userName;

  }

  public IsPreviousMessageOnAnotherDay(message: ChatMessage): boolean {
    let messageIndex = Cache.CurrentConversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == 0) {
      return true;
    }

    let thisMessageDay = new Date(message.timeReceived).getDay();
    let previousMessageDay = new Date(Cache.CurrentConversation.messages[messageIndex - 1].timeReceived).getDay();

    return thisMessageDay != previousMessageDay;
  }

  public GetMessagesDateStripFormatted(message: ChatMessage) : string {
    let messageIndex = Cache.CurrentConversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == 0) {
      return this.formatter.GetMessagesDateStripFormatted(message)
    }

    return this.formatter.GetMessagesDateStripFormatted(Cache.CurrentConversation.messages[messageIndex]);
  }

  private MessagesSortFunc(left: ChatMessage, right: ChatMessage) : number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }
}
