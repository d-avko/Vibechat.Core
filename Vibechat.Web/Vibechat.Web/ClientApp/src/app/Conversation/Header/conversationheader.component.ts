import { Component, Input } from "@angular/core";
import { ConversationTemplate } from "../../Data/ConversationTemplate";
import { ConversationsFormatter } from "../../Formatters/ConversationsFormatter";

@Component({
  selector: 'conversationHeader-view',
  templateUrl: './conversationheader.component.html',
  styleUrls: ['./conversationheader.component.css']
})
export class ConversationHeaderComponent {

  public formatter: ConversationsFormatter

  constructor(formatter: ConversationsFormatter) {
    this.formatter = formatter;
  }

  @Input() public Conversation: ConversationTemplate;
}
