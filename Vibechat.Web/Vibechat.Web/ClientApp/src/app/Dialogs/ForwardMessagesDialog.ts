import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { UserInfo } from "../Data/UserInfo";
import { ConversationTemplate } from "../Data/ConversationTemplate";

export interface ForwardMessagesData {
  conversations: Array<ConversationTemplate>;
}

@Component({
  selector: 'forward-messages-dialog',
  templateUrl: 'forward-messages-dialog.html',
})
export class ForwardMessagesDialogComponent {

  public SelectedConversations = new Array<ConversationTemplate>();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public conversationsService: ForwardMessagesData) { }

  public onCancelClick() {
    this.dialogRef.close();
  }

  public IsConversationSelected(conversation: ConversationTemplate): boolean {
    return this.SelectedConversations.findIndex((x) => x.conversationID == conversation.conversationID) != -1;
  }

  public SelectConversation(conversation: ConversationTemplate) {
    if (this.IsConversationSelected(conversation)) {

      this.SelectedConversations.splice(this.SelectedConversations.findIndex((x) => x.conversationID == conversation.conversationID), 1);
      return;

    }

    this.SelectedConversations.push(conversation);
  }

}
