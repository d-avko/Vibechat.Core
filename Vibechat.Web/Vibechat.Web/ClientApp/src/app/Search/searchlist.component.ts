import { ConversationTemplate } from "../Data/ConversationTemplate";
import { UserInfo } from "../Data/UserInfo";
import { Output, EventEmitter, Component, Input } from "@angular/core";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { Cache } from "../Auth/Cache";

@Component({
  selector: 'searchlist-view',
  templateUrl: './searchlist.component.html'
})
export class SearchListComponent {
  public GlobalConversations: Array<ConversationTemplate>;

  public FoundLocalConversations: Array<ConversationTemplate>;

  public Users: Array<UserInfo>;

  private requestsBuilder: ApiRequestsBuilder;

  @Output() public OnViewUser = new EventEmitter<UserInfo>();
  @Output() public OnViewConversation = new EventEmitter<ConversationTemplate>();
  @Output() public OnViewLocalConversation = new EventEmitter<ConversationTemplate>();
  @Output() public OnApiError = new EventEmitter<any>();
  @Output() public OnError = new EventEmitter<string>();
  @Input() public SearchString: string;
  @Input() public LocalConversations: Array<ConversationTemplate>;

  private IsSearchingForGroups: boolean;
  private IsSearchingForUsers: boolean;

  constructor(requestsBuilder: ApiRequestsBuilder) {
    this.requestsBuilder = requestsBuilder;
    this.FoundLocalConversations = new Array<ConversationTemplate>();
  }

  public Search() {

    while (this.FoundLocalConversations.length != 0) {
      this.FoundLocalConversations.pop();
    }

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

    if (this.IsSearchingForUsers) {
      return;
    }

    this.IsSearchingForUsers = true;

    this.requestsBuilder.FindUsersByUsername(this.SearchString)
      .subscribe(
        (result) => {
          this.IsSearchingForUsers = false;

          if (!result.isSuccessfull) {
            this.OnError.emit(result.errorMessage);
            return;
          }

          if (result.response.usersFound == null) {
            return;
          }

          this.Users = [...result.response.usersFound];
        },
        (error) => {
          this.OnApiError.emit(error);
          this.IsSearchingForUsers = false;
        }
    );

    if (this.IsSearchingForGroups) {
      return;
    }

    this.IsSearchingForGroups = true;

    this.requestsBuilder.SearchForGroups(this.SearchString)
      .subscribe(
        (result) => {
          this.IsSearchingForGroups = false;

          if (!result.isSuccessfull) {
            this.OnError.emit(result.errorMessage);
            return;
          }

          this.GlobalConversations = [...result.response];
        },
        (error) => {
          this.IsSearchingForGroups = false;
          this.OnApiError.emit(error);
        }
      );
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
