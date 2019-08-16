import {Chat} from "../Data/Chat";
import {AppUser} from "../Data/AppUser";
import {Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild} from "@angular/core";
import {ChatsService} from "../Services/ChatsService";
import {UsersService} from "../Services/UsersService";
import {MessageReportingService} from "../Services/MessageReportingService";
import {Message} from "../Data/Message";
import {ConversationsFormatter} from "../Formatters/ConversationsFormatter";

export class FoundMessage{
  public constructor(init?: Partial<FoundMessage>) {
    (<any>Object).assign(this, init);
  }

  chat: Chat;
  message: Message;
}

@Component({
  selector: 'searchlist-view',
  templateUrl: './searchlist.component.html'
})
export class SearchListComponent implements OnChanges{
  public GlobalConversations: Array<Chat>;

  public FoundLocalConversations: Array<Chat>;

  public FoundMessages: Array<FoundMessage>;

  public Users: Array<AppUser>;

  @Output() public OnViewUser = new EventEmitter<AppUser>();
  @Output() public OnViewConversation = new EventEmitter<Chat>();
  @Output() public OnViewLocalConversation = new EventEmitter<Chat>();
  @Output() public OnApiError = new EventEmitter<any>();
  @Output() public OnError = new EventEmitter<string>();
  @Output() public OnViewMessage = new EventEmitter<FoundMessage>();
  @Input() public SearchString: string;
  @Input() public LocalConversations: Array<Chat>;

  public IsSearchingForGroups: boolean;
  public IsSearchingForUsers: boolean;
  public IsSearchingForMessages: boolean;
  public IsMessagesEnd: boolean;
  public MinSearchChars = 5;
  public static MessagesBufferLength = 50;

  @ViewChild(ConversationsFormatter, {static: true}) formatter;

  constructor(private chats: ChatsService,
              private usersService: UsersService,
              private messages: MessageReportingService) {
    this.FoundLocalConversations = new Array<Chat>();
    this.FoundMessages = new Array<FoundMessage>();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes['SearchString'] != undefined){
      this.IsMessagesEnd = false;
      this.IsSearchingForGroups = false;
      this.IsSearchingForMessages = false;
      this.IsSearchingForUsers = false;
      this.IsMessagesEnd = false;
      this.FoundMessages = new Array<FoundMessage>();
    }
  }

  public async Search() {

    if(this.SearchString.length < this.MinSearchChars){
      this.messages.EnterAtLeast(this.MinSearchChars);
      return;
    }
    this.SearchLocalGroups();
    await this.SearchUsers();
    await this.SearchGlobalGroups();
    await this.SearchMessages();
  }
  private async SearchUsers(){
    //Searching users globally

    if (this.IsSearchingForUsers) {
      return;
    }

    this.IsSearchingForUsers = true;

    let result = await this.usersService.FindUsersByUsername(this.SearchString);

    this.IsSearchingForUsers = false;

    if (result == null) {
      return;
    }

    this.Users = [...result];
  }

  private SearchLocalGroups(){
    while (this.FoundLocalConversations.length != 0) {
      this.FoundLocalConversations.pop();
    }

    //searching groups and dialogs locally

    this.LocalConversations.forEach(
      (conversation) => {

        if (conversation.isGroup) {
          if (conversation.name.toUpperCase().startsWith(this.SearchString.toUpperCase())) {
            this.FoundLocalConversations.push(conversation);
          }
        } else {
          if (conversation.dialogueUser.userName.toUpperCase().startsWith(this.SearchString.toUpperCase())) {
            this.FoundLocalConversations.push(conversation);
          }
        }

      });
  }

  private async SearchGlobalGroups(){
    if (this.IsSearchingForGroups) {
      return;
    }

    //searching groups globally

    this.IsSearchingForGroups = true;

    let response = await this.chats.FindGroupsByName(this.SearchString);

    this.IsSearchingForGroups = false;

    if (response == null) {
      return;
    }

    this.GlobalConversations = [...response];
  }


  private async SearchMessages(){
    if (this.IsSearchingForMessages || this.IsMessagesEnd) {
      return;
    }

    this.IsSearchingForMessages = true;

    let foundMessages = await this.chats
      .SearchMessages(this.SearchString, this.FoundMessages.length, SearchListComponent.MessagesBufferLength);

    this.IsSearchingForMessages = false;

    if(foundMessages && foundMessages.length){
      let messagesToAppend = new Array<FoundMessage>();

      foundMessages.forEach(msg => {
        let chat = this.chats.GetById(msg.conversationID);

        if(!chat){
          return;
        }

        messagesToAppend.push(new FoundMessage({chat: chat, message: msg}));
      });

      this.FoundMessages.push(...messagesToAppend);
      this.FoundMessages = [...this.FoundMessages];
    }else{
      this.IsMessagesEnd = true;
    }
  }

  OnMessagesScrolled(index: number){
    if(index >= this.FoundMessages.length - 1){
      this.SearchMessages();
    }
  }

  public ViewMessage(msg: FoundMessage){
    this.OnViewMessage.emit(msg);
  }

  public ViewLocalConversation(conversation: Chat): void {
    this.OnViewLocalConversation.emit(conversation);
  }

  public ViewConversation(conversation: Chat): void {
    this.OnViewConversation.emit(conversation);
  }

  public ViewUser(user: AppUser) {
    this.OnViewUser.emit(user);
  }
}
