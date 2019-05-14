import { Component, Input, EventEmitter, Output } from "@angular/core";
import { ConversationTemplate } from "../../Data/ConversationTemplate";
import { ConversationsFormatter } from "../../Formatters/ConversationsFormatter";

@Component({
  selector: 'conversationslist-view',
  templateUrl: './conversationslist.component.html',
  styleUrls: ['./conversationslist.component.css']
})
export class ConversationsListComponent {

  public formatter: ConversationsFormatter;

  @Input() public Conversations: Array<ConversationTemplate>;

  @Output() public OnSelectedConversationChanged = new EventEmitter<ConversationTemplate>();

  @Input() public CurrentConversation: ConversationTemplate;

  constructor(formatter: ConversationsFormatter) {
    this.formatter = formatter;
  }

  public ChangeConversation(conversation: ConversationTemplate) {
    this.OnSelectedConversationChanged.emit(conversation);
  }

  public IsCurrentConversation(conversation: ConversationTemplate): boolean {
    if (this.CurrentConversation == null)
      return false;

    return this.CurrentConversation.conversationID == conversation.conversationID;
  }
}
