import { Component, Output, EventEmitter, ViewChild, AfterViewInit, OnChanges,  AfterViewChecked, SimpleChanges, Input, ViewContainerRef } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { trigger, state, style, transition, animate } from "@angular/animations";
import { ChatMessage } from '../../Data/ChatMessage';
import { ConversationsFormatter } from '../../Formatters/ConversationsFormatter';
import { AttachmentKinds } from '../../Data/AttachmentKinds';
import { UserInfo } from '../../Data/UserInfo';
import { MatDialog } from '@angular/material';
import { MessageState } from '../../Shared/MessageState';
import { ChatsService } from '../../Services/ChatsService';
import { ForwardMessagesDialogComponent } from '../../Dialogs/ForwardMessagesDialog';
import { ConversationTemplate } from '../../Data/ConversationTemplate';
import { ThemesService } from '../../Theming/ThemesService';
import { ViewPhotoService } from '../../Dialogs/ViewPhotoService';

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
    public conversationsService: ChatsService,
    private themes: ThemesService,
    private vc: ViewContainerRef,
    private photos: ViewPhotoService) {

    this.SelectedMessages = new Array<ChatMessage>();
    this.photos.viewContainerRef = this.vc;
  }

  @Input() public CurrentConversation: ConversationTemplate;

  @Output() public OnViewUserInfo = new EventEmitter<UserInfo>();

  public MessagesLoading: boolean;

  public IsConversationHistoryEnd: boolean = false;

  public IsScrollingAssistNeeded: boolean = false;

  public static MessagesToScrollForGoBackButtonToShowUp: number = 20;

  public static MessageMinSize: number = 50;

  public static MessagesBufferLength: number = 50;

  @ViewChild(CdkVirtualScrollViewport) viewport: CdkVirtualScrollViewport;
  
  public SelectedMessages: Array<ChatMessage>;

  ngAfterViewInit() {
    this.ScrollToMessage(this.CurrentConversation.messages.length - 1);
  }

  ngOnChanges(changes: SimpleChanges) {

    if (changes.CurrentConversation != undefined) {
      this.IsScrollingAssistNeeded = false;
      this.IsConversationHistoryEnd = false;
      this.SelectedMessages = new Array<ChatMessage>();
      this.UpdateMessagesIfNotUpdated();
      this.ScrollToMessage(this.CurrentConversation.messages.length - 1);
    }
  } 

  ngAfterViewChecked() {

    if (!this.CurrentConversation.messages || this.CurrentConversation.messages.length == 0) {
      return;
    }

    this.ReadMessagesInViewport();
  }

  public IsDarkTheme() {
    return this.themes.currentThemeName == 'dark';
  }

  public ViewImage(event: Event, image: ChatMessage) {
    event.stopPropagation();
    this.photos.ViewPhoto(image, (<HTMLImageElement>event.target).naturalWidth, (<HTMLImageElement>event.target).naturalHeight);
  }

  public async UpdateMessages() {

    let currentChat = this.CurrentConversation;

    if (!this.MessagesLoading && !this.IsConversationHistoryEnd) {
      this.MessagesLoading = true;

      if (currentChat.messages == null) {
        return;
      }

      let result = await this.conversationsService.GetMessagesForConversation(MessagesComponent.MessagesBufferLength, currentChat);

      if (result == null || result.length == 0) {
        this.MessagesLoading = false;
        this.IsConversationHistoryEnd = true;
        return;
      }

      if (!this.CurrentConversation) {
        this.MessagesLoading = false;
        return;
      }

      if (currentChat.conversationID == this.CurrentConversation.conversationID) {
        this.ScrollToMessage(result.length);
      }

      this.MessagesLoading = false;
    }

  }

  public async DeleteMessages() {
    await this.conversationsService.DeleteMessages(this.SelectedMessages, this.CurrentConversation);
  }

  public ForwardMessages() {
    let forwardMessagesDialog = this.dialog.open(
      ForwardMessagesDialogComponent,
      {
        data: {
          conversations: this.conversationsService.Conversations
        }
      }
    );

    forwardMessagesDialog
      .beforeClosed()
      .subscribe((result: Array<ConversationTemplate>) => {

        this.conversationsService.ForwardMessagesTo(result, this.SelectedMessages);
      })
  }

  
  public UpdateMessagesIfNotUpdated() {

    if (this.CurrentConversation.messages == null) {
      return;
    }
    
    if (this.CurrentConversation.messages.length <= 1) {
      this.UpdateMessages();
    }
  }

  public ScrollToStart() {
    this.ScrollToMessage(this.CurrentConversation.messages.length - 1);
  }

  public ViewUserInfo(event: any, user: UserInfo) {
  // do not highlight the message, just show user profile.
    event.stopPropagation();
    this.OnViewUserInfo.emit(user);
  }

  public ReadMessagesInViewport() {
    let boundaries = this.CalculateMessagesViewportBoundaries();

    for (let i = boundaries[0]; i < boundaries[1] + 1; ++i) {
      if (this.CurrentConversation.messages[i].state == MessageState.Delivered) {
        this.conversationsService.ReadMessage(this.CurrentConversation.messages[i]);
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

        if (!this.viewport) {
          return;
        }

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

    if (this.CurrentConversation.messages == null) {
      return;
    }

    if (messageIndex == 0) {
      this.UpdateMessages();
      return;
    }

    let currentMessageIndex = this.CalculateCurrentMessageIndex();

    if (this.CurrentConversation.messages.length - currentMessageIndex > MessagesComponent.MessagesToScrollForGoBackButtonToShowUp) {
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
    if (this.CurrentConversation.messages == null) {
      return false;
    }

    let messageIndex = this.CurrentConversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == -1) {
      return false;
    }

    if (messageIndex == 0) {
      return true;
    }

    return (<Date>message.timeReceived).getDay() != (<Date>this.CurrentConversation.messages[messageIndex - 1].timeReceived).getDay();
  }

  public IsLastMessage(message: ChatMessage): boolean {

    if (this.CurrentConversation.messages == null)
      return false;

    return this.CurrentConversation.messages.findIndex((x) => x.id == message.id) == 0;
  }

  public IsMessageSelected(message: ChatMessage): boolean {
    return this.SelectedMessages.find(x => x.id == message.id) != null;
  }

  public IsFirstMessageInSequence(message: ChatMessage): boolean {
    if (this.CurrentConversation.messages == null) {
      return false;
    }

    let messageIndex = this.CurrentConversation.messages.findIndex((x) => x.id == message.id);

    if (messageIndex == 0) {
      return true;
    }

    if (messageIndex == -1) {
      return false;
    }

    return this.CurrentConversation.messages[messageIndex - 1].user.userName != message.user.userName;
  }

  public IsImage(message: ChatMessage): boolean {

    if (!message.isAttachment) {
      return false;
    }

    return message.attachmentInfo.attachmentKind == AttachmentKinds.Image;
  }

  public IsFile(message: ChatMessage): boolean {

    if (!message.isAttachment) {
      return false;
    }

    return message.attachmentInfo.attachmentKind == AttachmentKinds.File;
  }

  public DownloadFile(event: any) {
    event.stopPropagation();
  }
}

