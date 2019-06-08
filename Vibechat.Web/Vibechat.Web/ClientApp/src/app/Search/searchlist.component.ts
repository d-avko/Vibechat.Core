import { ConversationTemplate } from "../Data/ConversationTemplate";
import { UserInfo } from "../Data/UserInfo";
import { Output, EventEmitter, Component, Input } from "@angular/core";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { ConversationsService } from "../Services/ConversationsService";
import { UsersService } from "../Services/UsersService";

@Component({
  selector: 'searchlist-view',
  templateUrl: './searchlist.component.html'
})
export class SearchListComponent {
  public GlobalConversations: Array<ConversationTemplate>;

  public FoundLocalConversations: Array<ConversationTemplate>;

  public Users: Array<UserInfo>;

  @Output() public OnViewUser = new EventEmitter<UserInfo>();
  @Output() public OnViewConversation = new EventEmitter<ConversationTemplate>();
  @Output() public OnViewLocalConversation = new EventEmitter<ConversationTemplate>();
  @Output() public OnApiError = new EventEmitter<any>();
  @Output() public OnError = new EventEmitter<string>();
  @Input() public SearchString: string;
  @Input() public LocalConversations: Array<ConversationTemplate>;

  private IsSearchingForGroups: boolean;
  private IsSearchingForUsers: boolean;

  constructor(private conversationsService: ConversationsService, private usersService: UsersService) {
    this.FoundLocalConversations = new Array<ConversationTemplate>();
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

  public ViewLocalConversation(conversation: ConversationTemplate): void {
    this.OnViewLocalConversation.emit(conversation);
  }

  public ViewConversation(conversation: ConversationTemplate): void {
    this.OnViewConversation.emit(conversation);
  }

  public ViewUser(user: UserInfo) {
    this.OnViewUser.emit(user);
  }
}
