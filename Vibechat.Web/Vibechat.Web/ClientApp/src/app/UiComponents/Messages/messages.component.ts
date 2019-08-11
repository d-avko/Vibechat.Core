import {
  AfterContentChecked,
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
import {animate, keyframes, state, style, transition, trigger} from "@angular/animations";
import {Message} from '../../Data/Message';
import {ConversationsFormatter} from '../../Formatters/ConversationsFormatter';
import {AttachmentKind} from '../../Data/AttachmentKinds';
import {AppUser} from '../../Data/AppUser';
import {MatDialog} from '@angular/material';
import {MessageState} from '../../Shared/MessageState';
import {ChatsService} from '../../Services/ChatsService';
import {ForwardMessagesDialogComponent} from '../../Dialogs/ForwardMessagesDialog';
import {Chat} from '../../Data/Chat';
import {ThemesService} from '../../Theming/ThemesService';
import {ViewPhotoService} from '../../Dialogs/ViewPhotoService';
import {AuthService} from "../../Services/AuthService";
import {MessageViewOption, MessageViewOptions} from "../../Shared/MessageViewOptions";

export class ForwardMessagesModel {
  public forwardTo: Array<number>;
  public Messages: Array<Message>;
}

@Component({
  selector: 'messages-view',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css'],
  animations: [
    trigger('slideInOut', [
      transition(':enter', [
        style({transform: 'translateY(-100%)'}),
        animate('200ms ease-in', style({transform: 'translateY(0%)'}))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({transform: 'translateY(-100%)'}))
      ])
    ]),
    trigger('highlight-message', [
      state('not-selected', style({

      })),
      state('selected', style(
        {
          color: 'white',
          backgroundColor:'cadetblue'
        })),
      transition('not-selected => selected', [
        animate('1500ms ease-out', keyframes(
          [
            style({backgroundColor: 'cadetblue'}),
            style({backgroundColor: 'white'}),
            style({backgroundColor: 'cadetblue'}),
            style({backgroundColor: 'white'}),
            style({backgroundColor: 'inherit', color: 'inherit'})
          ]
        ))
      ]),
      transition('selected => not-selected', [
        animate('1500ms ease-out',keyframes(
          [
            style({backgroundColor: 'cadetblue'}),
            style({backgroundColor: 'white'}),
            style({backgroundColor: 'cadetblue'}),
            style({backgroundColor: 'white'}),
            style({backgroundColor: 'inherit', color: 'inherit'})
          ]))
      ]),
    ])
  ]
})
export class MessagesComponent implements AfterContentChecked, AfterViewInit, OnChanges {

  constructor(
    public formatter: ConversationsFormatter,
    public dialog: MatDialog,
    public chatsService: ChatsService,
    public auth: AuthService,
    private themes: ThemesService,
    private vc: ViewContainerRef,
    private photos: ViewPhotoService,
    private options: MessageViewOptions) {
    this.ResetState();
  }

  @Input() public CurrentConversation: Chat;

  @Output() public OnViewUserInfo = new EventEmitter<AppUser>();

  public HistoryLoading: boolean;

  public RecentMessagesLoading: boolean;

  public IsConversationHistoryEnd: boolean = false;

  public IsRecentMessagesEnd: boolean = false;

  public IsScrollingAssistNeeded: boolean = false;

  public static MessagesToScrollForGoBackButtonToShowUp: number = 20;

  public static MessagesBufferLength: number = 50;

  public MaxErrorInPixels: number = 75;

  @ViewChild(CdkVirtualScrollViewport, { static: false }) viewport: CdkVirtualScrollViewport;

  public SelectedMessages: Array<Message>;

  ngAfterViewInit() {
    this.ScrollToLastMessage(true);
  }

  ngAfterContentChecked(): void {
    if(!this.viewport){
      return;
    }

    let currentMessageIndex = this.CalculateCurrentMessageIndex();

    if (this.CurrentConversation.messages.length - currentMessageIndex
      > MessagesComponent.MessagesToScrollForGoBackButtonToShowUp
      || !this.chatsService.IsUptoDate()
      && !(this.chatsService.IsCurrentChatDialog() && this.chatsService.IsAnyUnreadMessagesInCurrentChat())) {
      this.IsScrollingAssistNeeded = true;
    } else {
      this.IsScrollingAssistNeeded = false;
    }
  }

  async ngOnChanges(changes: SimpleChanges) {
    if(changes.CurrentConversation){
      if(this.options.Option.getValue() != MessageViewOption.NoOption){
        return;
      }

      await this.UpdateMessagesIfNotUpdated();
      await this.ReadMessagesInGroup();

      if(!this.CurrentConversation.isGroup){
        await this.ReadMessagesInViewport();
      }
      this.ScrollToLastMessage();
    }
  }

  private ResetState(){
    this.SelectedMessages = new Array<Message>();
    this.IsScrollingAssistNeeded = false;
    this.IsConversationHistoryEnd = false;
    this.IsRecentMessagesEnd = false;
    this.SelectedMessages = new Array<Message>();
    this._lockedMsgIndex = -1;
    this._lockedMsgId = -1;
  }

  private busy: boolean;
  private updatingMessagesAllowed = true;

  /**
   * Does actions considering this.options.
   * @constructor
   */
  public async ResolveProvidedOptions(){

    if(this.busy){
      return;
    }

    switch (this.options.Option.getValue()) {
      case MessageViewOption.ViewMessage:{
        try {
          this.busy = true;

          let messageIndex = this.chatsService.CurrentConversation.messages
            ?
            this.chatsService.CurrentConversation
              .messages
              .findIndex(msg => msg.id == this.options.MessageToViewId)
            : -1;

          //message exists, just scroll to it.

          if(messageIndex != -1){
            this.updatingMessagesAllowed = false;
            this.LockedMessageIndex = this.options.MessageToViewId;
            this.ScrollToMessage(messageIndex);
            this.OnMessageViewed();
            this.updatingMessagesAllowed = true;
            return;
          }

          this.chatsService.CurrentConversation.messages = null;
          this.chatsService.CurrentConversation.clientLastMessageId = this.options.MessageToViewId;
          this.ResetState();
          await this.UpdateMessagesIfNotUpdated();
        }
        finally {
          this.busy = false;
        }
        break;
      }
      case MessageViewOption.GotoMessage:{
        try {
          this.busy = true;
          this.chatsService.CurrentConversation.messages = null;
          this.chatsService.CurrentConversation.clientLastMessageId = this.options.MessageToViewId;
          this.ResetState();
          await this.UpdateMessagesIfNotUpdated();
        }
        finally {
          this.busy = false;
        }
        break;
      }
    }
  }

  ScrollToLastMessage(onlyLocal: boolean = false) {
    if (!this.CurrentConversation.messages) {
      return;
    }

    if(!this.viewport){
      return;
    }
    //all messages are already loaded, we can scroll already, if we are in group.
    //if this is dialog, let user read all messages first.
    if(this.chatsService.IsUptoDate()){

      if(this.chatsService.IsCurrentChatDialog() && this.chatsService.IsAnyUnreadMessagesInCurrentChat()){
        return;
      }

      requestAnimationFrame(() => {

        if(!this.viewport){
          return;
        }

        this.viewport.elementRef.nativeElement.scrollTop = this.viewport.elementRef.nativeElement.scrollHeight;
      });
    }else{

      if(onlyLocal){
        return;
      }

      //reload messages if we can.
      if(this.chatsService.CurrentConversation.messagesUnread == 0){
        this.options.MessageToViewId = this.chatsService.CurrentConversation.lastMessage.id;
        this.options.Option.next(MessageViewOption.GotoMessage);
        this.ResolveProvidedOptions();
      }
    }
  }

  public IsDarkTheme() {
    return this.themes.currentThemeName == 'dark';
  }

  public ReadMessagesInGroup(){
    return this.chatsService.ReadExistingMessagesInGroup();
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

  /**
   * Remembers first clientLastMessageId index.
   */
  private _lockedMsgIndex : number = -1;

  public _lockedMsgId: number = -1;

  private get LockedMessageIndex(){
    if(!this.CurrentConversation.messages[this._lockedMsgIndex]){
      this._lockedMsgIndex = this.CurrentConversation.messages.length - 1;
      this._lockedMsgId = this.CurrentConversation.messages[this.CurrentConversation.messages.length - 1].id;
    }

    return this._lockedMsgIndex;
  }

  /**
   * Updates _lockedMsgId too.
   * @param lastMessageId
   * @constructor
   */
  private set LockedMessageIndex(lastMessageId: number){
    this._lockedMsgIndex = this.chatsService.CurrentConversation.messages
      .findIndex(msg => msg.id == lastMessageId);

    if(this._lockedMsgIndex != -1){
      this._lockedMsgId = this.CurrentConversation.messages[this._lockedMsgIndex].id;
    }
  }

  private ResetLockedMessage(){
    this._lockedMsgId = -1;
    this._lockedMsgIndex = -1;
  }

  public async UpdateHistory() {

    let currentChat = this.CurrentConversation;

    if (!this.HistoryLoading && !this.IsConversationHistoryEnd && this.updatingMessagesAllowed) {
      this.HistoryLoading = true;

      if (currentChat.messages == null) {
        return;
      }

      let result = await this.chatsService.UpdateMessagesHistory(MessagesComponent.MessagesBufferLength,
        MessagesComponent.MessagesBufferLength, currentChat);

      this.HistoryLoading = false;

      if (result == null || result.length == 0) {
        this.IsConversationHistoryEnd = true;
        return;
      }

      if (!this.CurrentConversation) {
        return;
      }

      if (currentChat.id == this.CurrentConversation.id) {
        this.ScrollToMessage(result.length);
      }
    }

  }

  public async UpdateRecentMessages() {

    let currentChat = this.chatsService.CurrentConversation;

    if (!this.RecentMessagesLoading && !this.IsRecentMessagesEnd && this.updatingMessagesAllowed) {
      this.RecentMessagesLoading = true;

      if (!currentChat.messages) {
        return;
      }

      let result = await this.chatsService.UpdateRecentMessages(MessagesComponent.MessagesBufferLength, currentChat);

      this.RecentMessagesLoading = false;

      if (result == null || result.length == 0) {
        this.IsRecentMessagesEnd = true;
        return;
      }

      switch (this.options.Option.getValue()) {
        case MessageViewOption.ViewMessage:{
          this.LockedMessageIndex = this.options.MessageToViewId;
          this.ScrollToMessage(this.LockedMessageIndex - 1);
          this.OnMessageViewed();
          break;
        }
        case MessageViewOption.GotoMessage:{
          //do not highlight the message
          this.options.Option.next(MessageViewOption.NoOption);
          break;
        }
      }

      this.IsRecentMessagesEnd = false;
    }

  }

  private OnMessageViewed(){
    this.options.Option.next(MessageViewOption.NoOption);
    setTimeout(() => this.ResetLockedMessage(), 200);
  }

  public async DeleteMessages() {
    await this.chatsService.DeleteMessages(this.SelectedMessages, this.CurrentConversation);

    this.SelectedMessages.splice(0, this.SelectedMessages.length);
  }

  public ForwardMessages() {
    let forwardMessagesDialog = this.dialog.open(
      ForwardMessagesDialogComponent,
      {
        width: '350px',
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
    if (!this.chatsService.CurrentConversation.messages) {
      this.CurrentConversation.messages = new Array<Message>();
    }

    await this.UpdateRecentMessages();

    if (this.chatsService.CurrentConversation.messages.length <= 1) {
      await this.UpdateHistory();
      //prevent updating history on scroll event,
      // as on conversation change first message should be shown.
      this.HistoryLoading = true;
      setTimeout(() => this.HistoryLoading = false, 500);
    }
  }

  public ViewUserInfo(event: any, user: AppUser) {
  // do not highlight the message, just show user profile.
    event.stopPropagation();
    this.OnViewUserInfo.emit(user);
  }

  public async ReadMessagesInViewport() {
    if(!this.viewport){
      return;
    }

    let boundaries = this.CalculateMessagesViewportBoundaries();

    for (let i = boundaries[0]; i < boundaries[1] + 1; ++i) {
      if (this.CurrentConversation.messages[i] && this.CurrentConversation.messages[i].state == MessageState.Delivered) {
        //do sequential read, to keep lastMessageId on last read message.
        await this.chatsService.ReadMessage(this.CurrentConversation.messages[i], this.chatsService.CurrentConversation);
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
    if(!this.viewport){
      return;
    }

    let currentOffset = this.viewport.measureScrollOffset();
    let viewPortSize = this.viewport.getViewportSize();
    let messages = this.viewport.elementRef.nativeElement.children.item(0).children;
    let startBoundary: number;
    let endBoundary: number;
    let offset = 0;
    let index: number;

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
   * @constructor
   */
  public async OnMessagesScrolled(index: number) {
    // user scrolled to last loaded message, load more messages


    if (this.CurrentConversation.messages == null) {
      return;
    }

    if (index == 0) {
      await this.UpdateHistory();
    }

    let viewSize = this.viewport.getViewportSize();

    if(this.viewport.elementRef.nativeElement.scrollTop + viewSize
      >= this.viewport.elementRef.nativeElement.scrollHeight - this.MaxErrorInPixels){
      await this.UpdateRecentMessages();
    }

    //reading is supported for dialogs only.

    if(!this.CurrentConversation.isGroup){
      await this.ReadMessagesInViewport();
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

    let previousMessage = this.CurrentConversation.messages[messageIndex - 1];

    if (typeof previousMessage.timeReceived !== 'object') {
      previousMessage.timeReceived = new Date(<string>previousMessage.timeReceived);
    }

    return (<Date>message.timeReceived).getDay() != (<Date>previousMessage.timeReceived).getDay();
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

  public IsDialog(){
    return !this.CurrentConversation.isGroup;
  }

  public DownloadFile(event: any) {
    event.stopPropagation();
  }

  ViewForwardedMessage(event:Event, message: Message) {
    event.stopPropagation();

    if(this.chatsService.CurrentConversation.id != message.conversationID){
      return;
    }

    this.options.MessageToViewId = message.id;
    this.options.Option.next(MessageViewOption.ViewMessage);
    this.ResolveProvidedOptions();
  }
}

