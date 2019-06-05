import { Component, Output, EventEmitter, Input, ViewChild, OnInit, AfterViewInit } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { trigger, state, style, transition, animate } from "@angular/animations";
import { ConversationTemplate } from '../../Data/ConversationTemplate';
import { ChatMessage } from '../../Data/ChatMessage';
import { ConversationsFormatter } from '../../Formatters/ConversationsFormatter';
import { AttachmentKinds } from '../../Data/AttachmentKinds';
import { UserInfo } from '../../Data/UserInfo';
import { retry } from 'rxjs/operators';


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
export class MessagesComponent implements AfterViewInit {

  public formatter: ConversationsFormatter

  constructor(formatter: ConversationsFormatter) {
    this.formatter = formatter;
    this.SelectedMessages = new Array<ChatMessage>();
  }

  @Output() public OnUpdateMessages = new EventEmitter<void>();

  @Output() public OnDeleteMessages = new EventEmitter<Array<ChatMessage>>();

  @Output() public OnViewUserInfo = new EventEmitter<UserInfo>();

  @Input() public Conversation: ConversationTemplate;

  @Input() public MessagesLoading: boolean;

  public IsScrollingAssistNeeded: boolean = false;

  public static MessagesToScrollForGoBackButtonToShowUp: number = 20;

  @ViewChild(CdkVirtualScrollViewport) viewport: CdkVirtualScrollViewport;


  public SelectedMessages: Array<ChatMessage>;

  ngAfterViewInit() {
    this.UpdateMessagesIfNotUpdated();
    this.ScrollToMessage(this.Conversation.messages.length - 1);
  }

  public UpdateMessagesIfNotUpdated() {

    if (this.Conversation.messages == null) {
      return;
    }
    

    if (this.Conversation.messages.length > 1) {
      this.OnUpdateMessages.emit();
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

    let messages = this.viewport.elementRef.nativeElement.children.item(0).children;
    let offset = 0;
    let index = 0;
    for (index = 0; offset < currentOffset; ++index) {
      offset += messages.item(index).clientHeight;
    }

    return index - 1;
  }

  public OnMessagesScrolled(messageIndex: number): void {
    // user scrolled to last loaded message, load more messages

    if (this.Conversation.messages == null) {
      return;
    }

    if (messageIndex == 0) {
      this.OnUpdateMessages.emit();
      return;
    }

    if (this.Conversation.messages.length - this.CalculateCurrentMessageIndex() > MessagesComponent.MessagesToScrollForGoBackButtonToShowUp) {
      this.IsScrollingAssistNeeded = true;
    } else {
      this.IsScrollingAssistNeeded = false;
    }
  }

  public DeleteMessages() {
    this.OnDeleteMessages.emit(this.SelectedMessages);
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

