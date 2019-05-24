import { Component, Output, EventEmitter, Input, ViewChild, OnInit, AfterViewInit } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { trigger, state, style, transition, animate } from "@angular/animations";
import { ConversationTemplate } from '../../Data/ConversationTemplate';
import { ChatMessage } from '../../Data/ChatMessage';
import { ConversationsFormatter } from '../../Formatters/ConversationsFormatter';
import { AttachmentKinds } from '../../Data/AttachmentKinds';


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

  @Input() public Conversation: ConversationTemplate;

  @Input() public MessagesLoading: boolean;



  public IsScrollingAssistNeeded: boolean = false;

  public static MessagesToScrollForGoBackButtonToShowUp: number = 60;

  @ViewChild(CdkVirtualScrollViewport) viewport: CdkVirtualScrollViewport;


  public SelectedMessages: Array<ChatMessage>;

  ngAfterViewInit() {
    this.UpdateMessagesIfNotUpdated();
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

  public ScrollToMessage(scrollToMessageIndex: number) {
    requestAnimationFrame(() => {
      
      let messages = this.viewport.elementRef.nativeElement.children.item(0).children;
      let offset = 0;
      for (let i = 0; i < scrollToMessageIndex; ++i) {
        offset += messages.item(i).clientHeight;
      }

      this.viewport.scrollToOffset(offset, 'smooth');
    });
  }

  public OnMessagesScrolled(messageIndex: number): void {
    // user scrolled to last loaded message, load more messages

    if (this.Conversation.messages == null) {
      return;
    }

    if (messageIndex == 0) {
      this.OnUpdateMessages.emit();
    }

    if (this.Conversation.messages.length - messageIndex > MessagesComponent.MessagesToScrollForGoBackButtonToShowUp) {
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

