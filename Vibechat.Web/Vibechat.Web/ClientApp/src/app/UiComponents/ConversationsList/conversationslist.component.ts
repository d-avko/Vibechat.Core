import {Component, EventEmitter, Input, Output, ViewChild} from "@angular/core";
import {Chat} from "../../Data/Chat";
import {ConversationsFormatter} from "../../Formatters/ConversationsFormatter";

@Component({
  selector: 'conversationslist-view',
  templateUrl: './conversationslist.component.html',
  styleUrls: ['./conversationslist.component.css']
})
export class ConversationsListComponent {

  @ViewChild(ConversationsFormatter, {static: true}) formatter;

  @Input() public Conversations: Array<Chat>;

  @Output() public OnSelectedConversationChanged = new EventEmitter<Chat>();

  @Input() public CurrentConversation: Chat;

  constructor() {

  }

  public ChangeConversation(conversation: Chat) {
    this.OnSelectedConversationChanged.emit(conversation);
  }

  public IsCurrentConversation(conversation: Chat): boolean {
    if (this.CurrentConversation == null)
      return false;

    return this.CurrentConversation.id == conversation.id;
  }
}
