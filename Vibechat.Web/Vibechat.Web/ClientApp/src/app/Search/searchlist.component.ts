import {Chat} from "../Data/Chat";
import {UserInfo} from "../Data/UserInfo";
import {Component, EventEmitter, Input, Output} from "@angular/core";
import {ChatsService} from "../Services/ChatsService";
import {UsersService} from "../Services/UsersService";

@Component({
  selector: 'searchlist-view',
  templateUrl: './searchlist.component.html'
})
export class SearchListComponent {
  public GlobalConversations: Array<Chat>;

  public FoundLocalConversations: Array<Chat>;

  public Users: Array<UserInfo>;

  @Output() public OnViewUser = new EventEmitter<UserInfo>();
  @Output() public OnViewConversation = new EventEmitter<Chat>();
  @Output() public OnViewLocalConversation = new EventEmitter<Chat>();
  @Output() public OnApiError = new EventEmitter<any>();
  @Output() public OnError = new EventEmitter<string>();
  @Input() public SearchString: string;
  @Input() public LocalConversations: Array<Chat>;

  public IsSearchingForGroups: boolean;
  public IsSearchingForUsers: boolean;

  constructor(private conversationsService: ChatsService, private usersService: UsersService) {
    this.FoundLocalConversations = new Array<Chat>();
  }

  public async Search() {

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

    if (this.IsSearchingForGroups) {
      return;
    }

    //searching groups globally

    this.IsSearchingForGroups = true;

    let response = await this.conversationsService.FindGroupsByName(this.SearchString);

    this.IsSearchingForGroups = false;

    if (response == null) {
      return;
    }

    this.GlobalConversations = [...response];
  }

  public ViewLocalConversation(conversation: Chat): void {
    this.OnViewLocalConversation.emit(conversation);
  }

  public ViewConversation(conversation: Chat): void {
    this.OnViewConversation.emit(conversation);
  }

  public ViewUser(user: UserInfo) {
    this.OnViewUser.emit(user);
  }
}
