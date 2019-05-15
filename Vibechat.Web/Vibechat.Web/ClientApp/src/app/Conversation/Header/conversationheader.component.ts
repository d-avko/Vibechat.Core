import { Component, Input, Output, EventEmitter } from "@angular/core";
import { ConversationTemplate } from "../../Data/ConversationTemplate";
import { ConversationsFormatter } from "../../Formatters/ConversationsFormatter";
import { MatDialog, MatSnackBar } from "@angular/material";
import { FindUsersDialogComponent } from "../../Dialogs/FindUsersDialog";
import { ApiRequestsBuilder } from "../../Requests/ApiRequestsBuilder";
import { SnackBarHelper } from "../../Snackbar/SnackbarHelper";
import { Cache } from "../../Auth/Cache";
import { UserInfo } from "../../Data/UserInfo";

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

  @Output() public OnAddUsersToConversation = new EventEmitter<Array<UserInfo>>();

  public test() {
    const dialogRef = this.dialog.open(FindUsersDialogComponent, {
      width: '350px',
      data: {
        conversationId: this.Conversation.conversationID,
        requestsBuilder: this.requestsBuilder,
        snackbar: this.snackbar,
        token: Cache.JwtToken
      }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (result === '' || result == null) {
        return;
      }

      this.OnAddUsersToConversation.emit(result);
    });
  }

}
