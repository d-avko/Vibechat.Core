import { Component, Input, Output, EventEmitter } from "@angular/core";
import { ConversationTemplate } from "../../Data/ConversationTemplate";
import { ConversationsFormatter } from "../../Formatters/ConversationsFormatter";
import { MatDialog, MatSnackBar } from "@angular/material";
import { ApiRequestsBuilder } from "../../Requests/ApiRequestsBuilder";
import { SnackBarHelper } from "../../Snackbar/SnackbarHelper";

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

  @Input() public Conversation: ConversationTemplate;

  @Output() public OnViewGroupInfo = new EventEmitter<void>();

  public ViewGroupInfo() : void{
    this.OnViewGroupInfo.emit(null);
  }
}
