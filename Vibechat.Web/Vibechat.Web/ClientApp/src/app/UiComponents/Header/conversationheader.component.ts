import {Component, EventEmitter, Input, Output, ViewChild} from "@angular/core";
import {Chat} from "../../Data/Chat";
import {ConversationsFormatter} from "../../Formatters/ConversationsFormatter";
import {MatDialog} from "@angular/material";
import {Api} from "../../Services/Api/api.service";

@Component({
  selector: 'conversationHeader-view',
  templateUrl: './conversationheader.component.html',
  styleUrls: ['./conversationheader.component.css']
})
export class ConversationHeaderComponent {

  @ViewChild(ConversationsFormatter, {static: true}) formatter;

  constructor(
    public dialog: MatDialog,
    protected requestsBuilder: Api) {
  }

  @Input() public Conversation: Chat;

  @Output() public OnViewGroupInfo = new EventEmitter<void>();

  @Output() public OnGoback = new EventEmitter<void>();

  public ViewGroupInfo() : void{
    this.OnViewGroupInfo.emit(null);
  }

  public GoBack() {
    this.OnGoback.emit();
  }
}
