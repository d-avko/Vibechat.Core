import {
  AfterContentInit,
  AfterViewChecked,
  AfterViewInit,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  ViewChild,
  ViewContainerRef
} from '@angular/core';
import {CdkVirtualScrollViewport} from '@angular/cdk/scrolling';
import {animate, state, style, transition, trigger} from "@angular/animations";
import {Message} from '../../Data/Message';
import {ConversationsFormatter} from '../../Formatters/ConversationsFormatter';
import {AttachmentKind} from '../../Data/AttachmentKinds';
import {UserInfo} from '../../Data/UserInfo';
import {MatDialog} from '@angular/material';
import {MessageState} from '../../Shared/MessageState';
import {ChatsService} from '../../Services/ChatsService';
import {ForwardMessagesDialogComponent} from '../../Dialogs/ForwardMessagesDialog';
import {Chat} from '../../Data/Chat';
import {ThemesService} from '../../Theming/ThemesService';
import {ViewPhotoService} from '../../Dialogs/ViewPhotoService';

export class ForwardMessagesModel {
  public forwardTo: Array<number>;
  public Messages: Array<Message>;
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
export class MessagesComponent implements AfterViewChecked, AfterContentInit, AfterViewInit, OnChanges {

  constructor(
    public formatter: ConversationsFormatter,
    public dialog: MatDialog,
    public chatsService: ChatsService,
    private themes: ThemesService,
    private vc: ViewContainerRef,
    private photos: ViewPhotoService) {

    this.SelectedMessages = new Array<Message>();
  }

  @Input() public CurrentConversation: Chat;

  @Output() public OnViewUserInfo = new EventEmitter<UserInfo>();

  public HistoryLoading: boolean;

  public RecentMessagesLoading: boolean;

  public IsConversationHistoryEnd: boolean = false;

  public IsRecentMessagesEnd: boolean = false;

  public IsScrollingAssistNeeded: boolean = false;

  public static MessagesToScrollForGoBackButtonToShowUp: number = 20;

  public static MessagesBufferLength: number = 50;

  public MaxErrorInPixels: number = 25;

  @ViewChild(CdkVirtualScrollViewport, { static: false }) viewport: CdkVirtualScrollViewport;

  public SelectedMessages: Array<Message>;

  ngAfterViewInit() {
    this.ScrollToLastMessage();
  }

  ngAfterContentInit(): void {

  }

  async ngOnChanges(changes: SimpleChanges) {

    if (changes.CurrentConversation != undefined) {
      this.IsScrollingAssistNeeded = false;
      this.IsConversationHistoryEnd = false;
      this.SelectedMessages = new Array<Message>();
      await this.UpdateMessagesIfNotUpdated();
      await this.ReadMessagesInGroup();
      this.ScrollToLastMessage();
    }
  }

  ngAfterViewChecked() {

    if (!this.CurrentConversation.messages || this.CurrentConversation.messages.length == 0) {
      return;
    }

    this.ReadMessagesInViewport();
  }

  ScrollToLastMessage() {
    if (!this.CurrentConversation.messages) {
      return;
    }

    if(!this.viewport){
      return;
    }

    requestAnimationFrame(() => {
      this.viewport.elementRef.nativeElement.scrollTop = this.viewport.elementRef.nativeElement.scrollHeight;
    });
  }

  public IsDarkTheme() {
    return this.themes.currentThemeName == 'dark';
  }

  public ReadMessagesInGroup(){
    return this.chatsService.ReadExistingMessagesInGroup(MessagesComponent.MessagesBufferLength);
  }

  public ViewImage(event: Event, image: Message) {
    event.stopPropagation();
    this.photos.viewContainerRef = this.vc;
    this.photos.ViewPhoto(image);
  }

  public IsAllSelectedMessagesPending() {
    if (!this.SelectedMessages.length) {
      return false;
    }

    return this.SelectedMessages.every(x => x.state == MessageState.Pending);
  }

  public ResendMessages() {
    this.SelectedMessages.splice(0, this.SelectedMessages.length);

    this.chatsService.ResendMessages(this.SelectedMessages, this.CurrentConversation);
  }

  public async UpdateHistory() {

    let currentChat = this.CurrentConversation;

    if (!this.HistoryLoading && !this.IsConversationHistoryEnd) {
      this.HistoryLoading = true;

      if (currentChat.messages == null) {
        return;
      }

      let result = await this.chatsService.UpdateMessagesHistory(MessagesComponent.MessagesBufferLength,
        MessagesComponent.MessagesBufferLength, currentChat);

      if (result == null || result.length == 0) {
        this.HistoryLoading = false;
        this.IsConversationHistoryEnd = true;
        return;
      }

      if (!this.CurrentConversation) {
        this.HistoryLoading = false;
        return;
      }

      if (currentChat.id == this.CurrentConversation.id) {
        this.ScrollToMessage(result.length);
      }

      this.HistoryLoading = false;
    }

  }

  public async UpdateRecentMessages() {

    let currentChat = this.CurrentConversation;

    if (!this.RecentMessagesLoading && !this.IsRecentMessagesEnd) {
      this.RecentMessagesLoading = true;

      if (!currentChat.messages) {
        return;
      }

      let result = await this.chatsService.UpdateRecentMessages(MessagesComponent.MessagesBufferLength, currentChat);

      if (result == null || result.length == 0) {
        this.RecentMessagesLoading = false;
        this.IsRecentMessagesEnd = true;
        return;
      }

      if (!this.CurrentConversation) {
        this.RecentMessagesLoading = false;
        return;
      }

      if (currentChat.id == this.CurrentConversation.id) {
        this.ScrollToLastMessage();
      }

      this.RecentMessagesLoading = false;
    }

  }

  public async DeleteMessages() {
    await this.chatsService.DeleteMessages(this.SelectedMessages, this.CurrentConversation);
  }

  public ForwardMessages() {
    let forwardMessagesDialog = this.dialog.open(
      ForwardMessagesDialogComponent,
      {
        data: {
          conversations: this.chatsService.Conversations
        }
      }
    );

    forwardMessagesDialog
      .beforeClosed()
      .subscribe((result: Array<Chat>) => {

        this.chatsService.ForwardMessagesTo(result, this.SelectedMessages);
      })
  }


  public async UpdateMessagesIfNotUpdated() {

    if (!this.CurrentConversation.messages) {
      this.CurrentConversation.messages = new Array<Message>();
    }

    if (this.CurrentConversation.messages.length <= 1) {
      await this.UpdateHistory();
      await this.UpdateRecentMessages();
    }
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
        this.chatsService.ReadMessage(this.CurrentConversation.messages[i], this.chatsService.CurrentConversation);
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


  /**
   * Main method for handling scrolling event.
   * @param messageIndex
   * @constructor
   */
  public async OnMessagesScrolled(messageIndex: number) {
    // user scrolled to last loaded message, load more messages

    if (this.CurrentConversation.messages == null) {
      return;
    }

    if (messageIndex == 0) {
      await this.UpdateHistory();
      return;
    }

    let currentMessageIndex = this.CalculateCurrentMessageIndex();

    if (this.CurrentConversation.messages.length - currentMessageIndex > MessagesComponent.MessagesToScrollForGoBackButtonToShowUp) {
      this.IsScrollingAssistNeeded = true;
    } else {
      this.IsScrollingAssistNeeded = false;
    }

    if(this.viewport.elementRef.nativeElement.scrollTop
      >= this.viewport.elementRef.nativeElement.scrollHeight - this.MaxErrorInPixels){
      await this.UpdateRecentMessages();
    }

    //reading is supported for dialogs only.

    if(!this.CurrentConversation.isGroup){
      this.ReadMessagesInViewport();
    }
  }

  public SelectMessage(message: Message): void {

    let messageIndex = this.SelectedMessages.findIndex(x => x.id == message.id);

    if (messageIndex == -1) {

      this.SelectedMessages.push(message);

    } else {

      this.SelectedMessages.splice(messageIndex, 1);
    }
  }

  public IsPreviousMessageOnAnotherDay(message: Message): boolean {
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

    if (typeof message.timeReceived !== 'object') {
      message.timeReceived = new Date(<string>message.timeReceived);
    }

    if (typeof this.CurrentConversation.messages[messageIndex - 1].timeReceived !== 'object') {
      this.CurrentConversation.messages[messageIndex - 1].timeReceived = new Date(<string>this.CurrentConversation.messages[messageIndex - 1].timeReceived);
    }

    return (<Date>message.timeReceived).getDay() != (<Date>this.CurrentConversation.messages[messageIndex - 1].timeReceived).getDay();
  }

  public IsLastMessage(message: Message): boolean {

    if (this.CurrentConversation.messages == null)
      return false;

    return this.CurrentConversation.messages.findIndex((x) => x.id == message.id) == 0;
  }

  public IsMessageSelected(message: Message): boolean {
    return this.SelectedMessages.find(x => x.id == message.id) != null;
  }

  public IsFirstMessageInSequence(message: Message): boolean {
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

  public IsImage(message: Message): boolean {

    if (!message.isAttachment) {
      return false;
    }

    return message.attachmentInfo.attachmentKind == AttachmentKind.Image;
  }

  public IsFile(message: Message): boolean {

    if (!message.isAttachment) {
      return false;
    }

    return message.attachmentInfo.attachmentKind == AttachmentKind.File;
  }

  public DownloadFile(event: any) {
    event.stopPropagation();
  }
}

