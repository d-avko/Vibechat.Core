import {Component, EventEmitter, Input, Output} from "@angular/core";
import {Chat} from "../../Data/Chat";
import {ConversationsFormatter} from "../../Formatters/ConversationsFormatter";
import {MatDialog, MatSnackBar} from "@angular/material";
import {ApiRequestsBuilder} from "../../Requests/ApiRequestsBuilder";
import {SnackBarHelper} from "../../Snackbar/SnackbarHelper";

@Component({
  selector: 'conversationHeader-view',
  templateUrl: './conversationheader.component.html',
  styleUrls: ['./conversationheader.component.css']
})
export class ConversationHeaderComponent {

  protected snackbar: SnackBarHelper;

  constructor(
    public formatter: ConversationsFormatter,
    public dialog: MatDialog,
    protected requestsBuilder: ApiRequestsBuilder,
    snackbar: MatSnackBar) {
    this.snackbar = new SnackBarHelper(snackbar);
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
