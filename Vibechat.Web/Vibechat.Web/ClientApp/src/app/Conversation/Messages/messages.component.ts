import { Component, Output, EventEmitter, Input, ViewChild, OnInit, AfterViewInit, OnChanges, AfterContentInit, AfterViewChecked, SimpleChanges } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { trigger, state, style, transition, animate } from "@angular/animations";
import { ConversationTemplate } from '../../Data/ConversationTemplate';
import { ChatMessage } from '../../Data/ChatMessage';
import { ConversationsFormatter } from '../../Formatters/ConversationsFormatter';
import { AttachmentKinds } from '../../Data/AttachmentKinds';
import { UserInfo } from '../../Data/UserInfo';
import { retry } from 'rxjs/operators';
import { MatDialog } from '@angular/material';
import { ForwardMessagesDialogComponent } from '../../Dialogs/ForwardMessagesDialog';
import { MessageState } from '../../Shared/MessageState';
import { ViewportScroller } from '@angular/common';
import { ApiRequestsBuilder } from '../../Requests/ApiRequestsBuilder';
import { MessagesDateParserService } from '../../Services/MessagesDateParserService';
import { ConnectionManager } from '../../Connections/ConnectionManager';

export class ForwardMessagesModel {
  public forwardTo: Array<number>;
  public Messages: Array<ChatMessage>;
}

@Component({
  selector: 'messages-view',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
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
export class MessagesComponent implements AfterViewChecked, AfterViewInit, OnChanges {

  constructor(
    public formatter: ConversationsFormatter,
    public dialog: MatDialog,
    public requestsBuilder: ApiRequestsBuilder,
    public dateParser: MessagesDateParserService) {
    this.SelectedMessages = new Array<ChatMessage>();
  }

  @Output() public OnMessageRead = new EventEmitter<ChatMessage>();

  @Output() public OnViewUserInfo = new EventEmitter<UserInfo>();

  @Output() public OnForwardMessages = new EventEmitter<Array<ChatMessage>>();

  @Input() public Conversation: ConversationTemplate;

  @Input() public Conversations: Array<ConversationTemplate>;

  @Input() public User: UserInfo;

  public MessagesLoading: boolean;

  public IsConversationHistoryEnd: boolean = false;

  public IsScrollingAssistNeeded: boolean = false;

  public static MessagesToScrollForGoBackButtonToShowUp: number = 20;

  public static MessageMinSize: number = 50;

  public static MessagesBufferLength: number = 50;

  @ViewChild(CdkVirtualScrollViewport) viewport: CdkVirtualScrollViewport;
  
  public SelectedMessages: Array<ChatMessage>;

  ngAfterViewInit() {
    this.ScrollToMessage(this.Conversation.messages.length - 1);
  }

  ngOnChanges(changes: SimpleChanges) {

    if (changes.Conversation != undefined) {
      this.IsScrollingAssistNeeded = false;
      this.IsConversationHistoryEnd = false;
      this.SelectedMessages = new Array<ChatMessage>();
      this.UpdateMessagesIfNotUpdated();
      this.ScrollToMessage(this.Conversation.messages.length - 1);
    }
  }

  ngAfterViewChecked() {

    if (!this.Conversation.messages || this.Conversation.messages.length == 0) {
      return;
    }

    this.ReadMessagesInViewport();
  }

  public UpdateMessages() {

    if (!this.MessagesLoading && !this.IsConversationHistoryEnd) {
      this.MessagesLoading = true;

      if (this.Conversation.messages == null) {
        return;
      }

      this.requestsBuilder.GetConversationMessages(
        this.Conversation.messages.length,
        MessagesComponent.MessagesBufferLength,
        this.Conversation.conversationID)

        .subscribe((result) => {

          if (!result.isSuccessfull) {
            this.MessagesLoading = false;
            return;
          }

          //server sent zero messages, we reached end of our history.

          if (result.response == null || result.response.length == 0) {
            this.MessagesLoading = false;
            this.IsConversationHistoryEnd = true;

            return;
          }


          result.response = result.response.sort(this.MessagesSortFunc);

          this.dateParser.ParseStringDatesInMessages(result.response);

          //append old messages to new ones.
          this.Conversation.messages = [...result.response.concat(this.Conversation.messages)];
          this.MessagesLoading = false;

          this.ScrollToMessage(result.response.length);
        }
        )
    }

  }

  public DeleteMessages() {

    let currentConversationId = this.Conversation.conversationID;

    let notLocalMessages = this.SelectedMessages.filter(x => x.state != MessageState.Pending)

    //delete local unsent messages
    this.Conversation.messages = this.Conversation.messages
      .filter(msg => notLocalMessages.findIndex(selected => selected.id == msg.id) == -1);

    if (notLocalMessages.length == 0) {
      return;
    }

    let conversationId = this.Conversation.conversationID;

    this.requestsBuilder.DeleteMessages(notLocalMessages, conversationId)
      .subscribe(
        (response) => {

          if (!response.isSuccessfull) {
            return;
          }

          //delete messages locally

          let conversation = this.Conversations.find(x => x.conversationID == conversationId);

          conversation.messages = conversation
            .messages
            .filter(msg => this.SelectedMessages.findIndex(selected => selected.id == msg.id) == -1);

          this.ScrollToStart();

          this.SelectedMessages.splice(0, this.SelectedMessages.length);
        }
      );
  }

  public ForwardMessages() {
    this.OnForwardMessages.emit(this.SelectedMessages);
  }

  private MessagesSortFunc(left: ChatMessage, right: ChatMessage): number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }


  public UpdateMessagesIfNotUpdated() {

    if (this.Conversation.messages == null) {
      return;
    }
    
    if (this.Conversation.messages.length <= 1) {
      this.UpdateMessages();
    }
  }

  public ScrollToStart() {
    this.ScrollToMessage(this.Conversation.messages.length);
  }

  public ViewUserInfo(event: any, user: UserInfo) {
  // do not highlight the message, just show user profile.
    event.stopPropagation();
    this.OnViewUserInfo.emit(user);
  }

  public ReadMessagesInViewport() {
    let boundaries = this.CalculateMessagesViewportBoundaries();

    for (let i = boundaries[0]; i < boundaries[1] + 1; ++i) {
      if (this.Conversation.messages[i].state == MessageState.Delivered) {
        this.OnMessageRead.emit(this.Conversation.messages[i]);
      }
    }
  }

  public CalculateOffsetToMessage(index: number) {
    let messages = this.viewport.elementRef.nativeElement.children.item(0).children;
    let offset = 0;
    for (let i = 0; i < index; ++i) {
      offset += messages.item(i).clientHeight;
    }

    return offset;
  }

  public ScrollToMessage(scrollToMessageIndex: number) {
      requestAnimationFrame(() => {
      
        this.viewport.scrollToOffset(this.CalculateOffsetToMessage(scrollToMessageIndex));
    });
  }

  public CalculateCurrentMessageIndex() : number {
    let currentOffset = this.viewport.measureScrollOffset();

    if (currentOffset == 0) {
      return 0;
    }

    let messages = this.viewport.elementRef.nativeElement.children.item(0).children;
    let offset = 0;
    let index = 0;

    for (index = 0; offset < currentOffset; ++index) {
      offset += messages.item(index).clientHeight;
    }
    return index - 1;
  }

  public CalculateMessagesViewportBoundaries(): Array<number> {

    let currentOffset = this.viewport.measureScrollOffset();
    let viewPortSize = this.viewport.getViewportSize();
    let messages = this.viewport.elementRef.nativeElement.children.item(0).children;
    let startBoundary = 0;
    let endBoundary = 0;
    let offset = 0;
    let index = 0;

    for (index = 0; index < messages.length && offset < currentOffset; ++index) {
      offset += messages.item(index).clientHeight;
    }
    startBoundary = Math.max(index - 1, 0);

    for (index = startBoundary + 1; index < messages.length && offset < currentOffset + viewPortSize; ++index) {
      offset += messages.item(index).clientHeight;
    }

    endBoundary = Math.max(index - 1, 0);

    return new Array<number>(startBoundary, endBoundary);
  }

  public OnMessagesScrolled(messageIndex: number): void {
    // user scrolled to last loaded message, load more messages

    if (this.Conversation.messages == null) {
      return;
    }

    if (messageIndex == 0) {
      this.UpdateMessages();
      return;
    }

    let currentMessageIndex = this.CalculateCurrentMessageIndex();

    if (this.Conversation.messages.length - currentMessageIndex > MessagesComponent.MessagesToScrollForGoBackButtonToShowUp) {
      this.IsScrollingAssistNeeded = true;
    } else {
      this.IsScrollingAssistNeeded = false;
    }

    //read the message if it's not read

    this.ReadMessagesInViewport();
  }

  public SelectMessage(message: ChatMessage): void {

    let messageIndex = this.SelectedMessages.findIndex(x => x.id == message.id);

    if (messageIndex == -1) {

      this.SelectedMessages.push(message);

    } else {

      this.SelectedMessages.splice(messageIndex, 1);
    }
  }

  public IsPreviousMessageOnAnotherDay(message: ChatMessage): boolean {
    if (this.Conversation.messages == null) {
      return false;
    }

    let messageIndex = this.Conversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == -1) {
      return false;
    }

    if (messageIndex == 0) {
      return true;
    }

    return (<Date>message.timeReceived).getDay() != (<Date>this.Conversation.messages[messageIndex - 1].timeReceived).getDay();
  }

  public IsLastMessage(message: ChatMessage): boolean {

    if (this.Conversation.messages == null)
      return false;

    return this.Conversation.messages.findIndex((x) => x.id == message.id) == 0;
  }

  public IsMessageSelected(message: ChatMessage): boolean {
    return this.SelectedMessages.find(x => x.id == message.id) != null;
  }

  public IsFirstMessageInSequence(message: ChatMessage): boolean {
    if (this.Conversation.messages == null) {
      return false;
    }

    let messageIndex = this.Conversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == 0) {
      return true;
    }

    if (messageIndex == -1) {
      return false;
    }

    return this.Conversation.messages[messageIndex - 1].user.userName != message.user.userName;
  }

  public IsImage(message: ChatMessage): boolean {

    if (!message.isAttachment) {
      return false;
    }

    return message.attachmentInfo.attachmentKind == AttachmentKinds.Image;
  }
}

