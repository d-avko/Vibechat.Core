import {
  AfterContentChecked,
  AfterContentInit,
  AfterViewChecked,
  AfterViewInit,
  Component,
  EventEmitter,
  OnDestroy,
  Output,
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
import {MessageType} from "../../Data/MessageType";
import {Subscription} from "rxjs";

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
      state('not-selected', style({})),
      state('selected', style(
        {
          color: 'white',
          backgroundColor: 'cadetblue'
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
        animate('1500ms ease-out', keyframes(
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
export class MessagesComponent implements OnDestroy, AfterContentInit, AfterViewChecked, AfterContentChecked, AfterViewInit {

  @ViewChild(ConversationsFormatter, {static: true}) formatter : ConversationsFormatter;

  constructor(
    public dialog: MatDialog,
    public chatsService: ChatsService,
    public auth: AuthService,
    private themes: ThemesService,
    private vc: ViewContainerRef,
    private photos: ViewPhotoService,
    private options: MessageViewOptions) {
    this.ResetState();
  }

  public CurrentChat: Chat;

  @Output() public OnViewUserInfo = new EventEmitter<AppUser | string>();

  public HistoryLoading: boolean;

  public RecentMessagesLoading: boolean;

  public IsConversationHistoryEnd: boolean = false;

  public IsRecentMessagesEnd: boolean = false;

  public IsScrollingAssistNeeded: boolean = false;

  public static MessagesToScrollForGoBackButtonToShowUp: number = 20;

  public static MessagesBufferLength: number = 20;

  public MaxErrorInPixels: number = 75;

  @ViewChild(CdkVirtualScrollViewport, {static: false}) viewport: CdkVirtualScrollViewport;

  public SelectedMessages: Array<Message>;

  private ChatChangedSub : Subscription;

  ngAfterViewInit() {
    this.ScrollToLastMessage(true);
  }

  ngAfterContentInit(): void {
    this.ChatChangedSub = this.chatsService._currentChat.subscribe(async chat => {
      await this.onChatChanged(chat);
    });
  }

  ngOnDestroy(): void {
    if(this.ChatChangedSub){
      this.ChatChangedSub.unsubscribe();
    }
  }

  async onChatChanged(newChat: Chat){
    this.ResetState();

    this.CurrentChat = newChat;

    if (this.options.Option.getValue() != MessageViewOption.NoOption) {
      return;
    }

    if(!newChat){
      return;
    }

    await this.UpdateMessagesIfNotUpdated();
    await this.ReadMessagesInGroup();

    if (!this.CurrentChat.isGroup) {
      requestAnimationFrame(async () => {
        await this.ReadMessagesInViewport();
      });
    }

    this.ScrollToLastMessage(true);
  }

  ngAfterViewChecked(): void {
    if(!this.CurrentChat){
      return;
    }

    if(!this.CurrentChat.isGroup && this.chatsService.IsAnyUnreadMessagesInCurrentChat()){
      this.ReadMessagesInViewport();
    }
  }

  private _updatingOldValue = false;

  async ngAfterContentChecked() {
    if (!this.viewport) {
      return;
    }

    if(!this.CurrentChat){
      return;
    }

    if(!this.CurrentChat.messages){
      return;
    }

    let currentMessageIndex = this.CalculateCurrentMessageIndex();

    if (this.CurrentChat.messages.length - currentMessageIndex > MessagesComponent.MessagesToScrollForGoBackButtonToShowUp
      || !this.chatsService.IsUptoDate()
      && !(this.chatsService.IsCurrentChatDialog() && this.chatsService.IsAnyUnreadMessagesInCurrentChat())) {
      this.IsScrollingAssistNeeded = true;
    } else {
      this.IsScrollingAssistNeeded = false;
    }

    //chat finished loading, update messages

    if (this.chatsService.isUpdatingCurrentChat != this._updatingOldValue) {
      this._updatingOldValue = this.chatsService.isUpdatingCurrentChat;
      if (!this.chatsService.isUpdatingCurrentChat && this.options.Option.getValue() == MessageViewOption.NoOption) {
        await this.UpdateMessagesIfNotUpdated();
        this.ScrollToLastMessage(true);
      }
    }
  }

  private ResetState() {
    this.SelectedMessages = new Array<Message>();
    this.IsScrollingAssistNeeded = false;
    this.IsConversationHistoryEnd = false;
    this.IsRecentMessagesEnd = false;
    this.updatingMessagesAllowed = true;
    this.busy = false;
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
  public async ResolveProvidedOptions() {

    if (this.busy) {
      return;
    }

    switch (this.options.Option.getValue()) {
      case MessageViewOption.ViewMessage: {
        try {
          this.busy = true;

          let messageIndex = this.CurrentChat.messages
            ?
            this.CurrentChat
              .messages
              .findIndex(msg => msg.id == this.options.MessageToViewId)
            : -1;

          //message exists, just scroll to it.

          if (messageIndex != -1) {
            return;
            //see finally
          }

          this.CurrentChat.messages = null;
          this.CurrentChat.clientLastMessageId = this.options.MessageToViewId;
          this.ResetState();
          await this.UpdateMessagesIfNotUpdated();

        } finally {
          this.updatingMessagesAllowed = false;
          this.LockedMessageIndex = this.options.MessageToViewId;
          this.ScrollToMessage(this.LockedMessageIndex - 1);
          this.OnMessageViewed();
          this.updatingMessagesAllowed = true;
          this.busy = false;
        }
        break;
      }
      case MessageViewOption.GotoMessage: {
        try {
          this.busy = true;
          this.CurrentChat.messages = null;
          this.CurrentChat.clientLastMessageId = this.options.MessageToViewId;
          this.ResetState();
          await this.UpdateMessagesIfNotUpdated();
        } finally {
          this.options.Option.next(MessageViewOption.NoOption);
          this.busy = false;
        }
        break;
      }
    }
  }

  public ScrollToLastMessage(onlyLocal: boolean = false) {
    if(!this.CurrentChat){
      return;
    }

    if (!this.CurrentChat.messages) {
      return;
    }

    if (!this.viewport) {
      return;
    }

    //if this is dialog, let user read all messages first.
    if (this.chatsService.IsCurrentChatDialog()) {

      if(this.chatsService.IsAnyUnreadMessagesInCurrentChat()){
        let index: number;

        if(!(index = this.chatsService.GetLastUnreadMessageIndexInCurrentChat())){
          return;
        }else{
          this.ScrollToMessage(index);
          return;
        }
      }
    }

    //all messages are already loaded, we can scroll already, if we are in group.
    if (this.chatsService.IsUptoDate()) {

      requestAnimationFrame(() => {

        if (!this.viewport) {
          return;
        }

        this.viewport.elementRef.nativeElement.scrollTop = this.viewport.elementRef.nativeElement.scrollHeight;
      });
    } else {

      if (onlyLocal) {
        return;
      }

      //reload messages if we can.
      if (this.CurrentChat.messagesUnread == 0) {
        this.options.MessageToViewId = this.CurrentChat.lastMessage.id;
        this.options.Option.next(MessageViewOption.GotoMessage);
        this.ResolveProvidedOptions();
      }
    }
  }

  public IsDarkTheme() {
    return this.themes.currentThemeName == 'dark';
  }

  public ReadMessagesInGroup() {
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

    this.chatsService.ResendMessages(this.SelectedMessages, this.CurrentChat);
  }

  /**
   * Remembers first clientLastMessageId index.
   */
  private _lockedMsgIndex: number = -1;

  public _lockedMsgId: number = -1;

  private get LockedMessageIndex() {
    if (!this.CurrentChat.messages[this._lockedMsgIndex]) {
      this._lockedMsgIndex = this.CurrentChat.messages.length - 1;
      this._lockedMsgId = this.CurrentChat.messages[this.CurrentChat.messages.length - 1].id;
    }

    return this._lockedMsgIndex;
  }

  /**
   * Updates _lockedMsgId too.
   * @param lastMessageId
   * @constructor
   */
  private set LockedMessageIndex(lastMessageId: number) {
    this._lockedMsgIndex = this.CurrentChat.messages
      .findIndex(msg => msg.id == lastMessageId);

    if (this._lockedMsgIndex != -1) {
      this._lockedMsgId = this.CurrentChat.messages[this._lockedMsgIndex].id;
    }
  }

  private ResetLockedMessage() {
    this._lockedMsgId = -1;
    this._lockedMsgIndex = -1;
  }

  public async UpdateHistory(scrollEvent: boolean = false) {

    if(scrollEvent && this.options.Option.getValue() != MessageViewOption.NoOption){
      return;
    }

    let currentChat = this.CurrentChat;

    if (currentChat.messages == null) {
      return;
    }

    if (!this.HistoryLoading
      && !this.IsConversationHistoryEnd
      && this.updatingMessagesAllowed
      && !this.chatsService.isUpdatingCurrentChat) {
      this.HistoryLoading = true;

      let result = await this.chatsService.UpdateMessagesHistory(MessagesComponent.MessagesBufferLength, currentChat);

      this.HistoryLoading = false;

      if (result == null || result.length == 0) {
        this.IsConversationHistoryEnd = true;
        return;
      }

      if (!this.CurrentChat) {
        return;
      }

      if (currentChat.id == this.CurrentChat.id) {
          if(!scrollEvent){
            //on messages update, scrollTop won't be changed , so visually
            // messages will be higher than the message we want to view,
            //so scroll back.
            let index = this.CurrentChat
              .messages
              .findIndex(x => x.id == this.options.MessageToViewId);

            if(index == -1){
              return;
            }

            this.ScrollToMessage(index);
          }else{
            this.ScrollToMessage(result.length);
          }
      }
    }

  }

  public async UpdateRecentMessages(scrollEvent: boolean = false) {

    let currentChat = this.CurrentChat;

    if (!currentChat.messages) {
      return;
    }

    if(scrollEvent && this.options.Option.getValue() != MessageViewOption.NoOption){
      return;
    }

    if (!this.RecentMessagesLoading
      && !this.IsRecentMessagesEnd
      && this.updatingMessagesAllowed
      && !this.chatsService.isUpdatingCurrentChat) {
      this.RecentMessagesLoading = true;

      let result = await this.chatsService.UpdateRecentMessages(MessagesComponent.MessagesBufferLength, currentChat);

      this.RecentMessagesLoading = false;

      if (result == null || result.length == 0) {
        this.IsRecentMessagesEnd = true;
        return;
      }

      this.IsRecentMessagesEnd = false;
    }

  }

  private OnMessageViewed() {
    this.options.Option.next(MessageViewOption.NoOption);
    setTimeout(() => this.ResetLockedMessage(), 200);
  }

  public async DeleteMessages() {
    await this.chatsService.DeleteMessages(this.SelectedMessages, this.CurrentChat);

    this.SelectedMessages.splice(0, this.SelectedMessages.length);
  }

  public ForwardMessages() {
    let forwardMessagesDialog = this.dialog.open(
      ForwardMessagesDialogComponent,
      {
        panelClass: "profile-dialog",
        width: '350px',
        data: {
          conversations: this.chatsService.Chats.filter(chat => !chat.isSecure)
        }
      }
    );

    forwardMessagesDialog
      .beforeClosed()
      .subscribe(async (result: Array<Chat>) => {

        await this.chatsService.ForwardMessagesTo(result, this.SelectedMessages);
        this.SelectedMessages.splice(0, this.SelectedMessages.length);
      });
  }

  public async UpdateMessagesIfNotUpdated() {
    if (!this.CurrentChat.messages) {
      this.CurrentChat.messages = new Array<Message>();
    }
    await this.UpdateRecentMessages();
    await this.UpdateHistory();
  }

  public ViewUserInfo(event: any, user: AppUser) {
    // do not highlight the message, just show user profile.
    event.stopPropagation();
    this.OnViewUserInfo.emit(user);
  }

  public ViewUserInfoWithoutEvent(user: string) {
    this.OnViewUserInfo.emit(user);
  }

  private readingMessages: boolean = false;

  public async ReadMessagesInViewport() {
    if (!this.viewport || this.readingMessages) {
      return;
    }

    try {
      this.readingMessages = true;
      let boundaries = this.CalculateMessagesViewportBoundaries();
      let left = boundaries[0] - Math.floor(MessagesComponent.MessagesBufferLength / 5);
      let right = boundaries[1] + Math.ceil(MessagesComponent.MessagesBufferLength / 5);
      for (let i = left; i < right; ++i) {
        if (this.CurrentChat.messages[i] && this.CurrentChat.messages[i].state == MessageState.Delivered) {
          //do sequential read, to keep lastMessageId on last read message.
          await this.chatsService.ReadMessage(this.CurrentChat.messages[i], this.CurrentChat);
        }
      }
    }
    finally {
      this.readingMessages = false;
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

  public CalculateCurrentMessageIndex(): number {
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
    if (!this.viewport) {
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
    if(!this.CurrentChat){
      return;
    }

    if (this.CurrentChat.messages == null) {
      return;
    }

    if (index == 0) {
      await this.UpdateHistory(true);
    }

    let viewSize = this.viewport.getViewportSize();

    if (this.viewport.elementRef.nativeElement.scrollTop + viewSize
      >= this.viewport.elementRef.nativeElement.scrollHeight - this.MaxErrorInPixels) {
      await this.UpdateRecentMessages(true);
    }

    //reading is supported for dialogs only.

    if (!this.CurrentChat.isGroup && this.chatsService.IsAnyUnreadMessagesInCurrentChat()) {
      await this.ReadMessagesInViewport();
    }
  }

  public SelectMessage(message: Message): void {
    if(message.type == MessageType.Event){
      return;
    }

    let messageIndex = this.SelectedMessages.findIndex(x => x.id == message.id);

    if (messageIndex == -1) {

      this.SelectedMessages.push(message);

    } else {

      this.SelectedMessages.splice(messageIndex, 1);
    }
  }

  public IsPreviousMessageOnAnotherDay(message: Message, index: number): boolean {
    if(!this.CurrentChat){
      return false;
    }

    if (this.CurrentChat.messages == null) {
      return false;
    }

    if (index == 0) {
      return true;
    }

    if (typeof message.timeReceived !== 'object') {
      message.timeReceived = new Date(<string>message.timeReceived);
    }

    let previousMessage = this.CurrentChat.messages[index - 1];

    if(!previousMessage){
      return true;
    }

    if (typeof previousMessage.timeReceived !== 'object') {
      previousMessage.timeReceived = new Date(<string>previousMessage.timeReceived);
    }

    return (<Date>message.timeReceived).getDay() != (<Date>previousMessage.timeReceived).getDay();
  }

  public IsMessageSelected(message: Message): boolean {
    return this.SelectedMessages.find(x => x.id == message.id) != null;
  }

  public IsFirstMessageInSequence(message: Message, index: number): boolean {
    if(!this.CurrentChat){
      return false;
    }

    if (this.CurrentChat.messages == null) {
      return false;
    }

    if(message.type == MessageType.Event){
      return false;
    }

    if (index == 0) {
      return true;
    }

    if(this.CurrentChat.messages[index - 1].type == MessageType.Event){
      return true;
    }

    return this.CurrentChat.messages[index - 1].user.userName != message.user.userName;
  }


  public IsText(msg : Message){
    return msg.type == MessageType.Text;
  }

  public IsImage(message: Message): boolean {

    if (message.type != MessageType.Attachment) {
      return false;
    }

    return message.attachmentInfo.attachmentKind == AttachmentKind.Image;
  }

  public IsFile(message: Message): boolean {

    if (message.type != MessageType.Attachment) {
      return false;
    }

    return message.attachmentInfo.attachmentKind == AttachmentKind.File;
  }

  public IsEvent(msg: Message){
    return msg.type == MessageType.Event;
  }

  public IsForwarded(msg: Message){
    return msg.type == MessageType.Forwarded;
  }

  public IsDialog() {
    return this.CurrentChat && !this.CurrentChat.isGroup;
  }

  public DownloadFile(event: any) {
    event.stopPropagation();
  }

  ViewForwardedMessage(event: Event, message: Message) {
    event.stopPropagation();

    if (this.CurrentChat.id != message.conversationID) {
      return;
    }

    this.options.MessageToViewId = message.id;
    this.options.Option.next(MessageViewOption.ViewMessage);
    this.ResolveProvidedOptions();
  }
}

