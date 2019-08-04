import {Component, Inject} from "@angular/core";
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material";
import {ChatComponent} from "../UiComponents/Chat/chat.component";
import {Chat} from "../Data/Chat";

export interface ForwardMessagesData {
  conversations: Array<Chat>;
}

@Component({
  selector: 'forward-messages-dialog',
  templateUrl: 'forward-messages-dialog.html',
})
export class ForwardMessagesDialogComponent {

  public SelectedConversations = new Array<Chat>();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ForwardMessagesData) { }

  public onCancelClick() {
    this.dialogRef.close();
  }

  public IsConversationSelected(conversation: Chat): boolean {
    return this.SelectedConversations.findIndex((x) => x.id == conversation.id) != -1;
  }

  public SelectConversation(conversation: Chat) {
    if (this.IsConversationSelected(conversation)) {

      this.SelectedConversations.splice(this.SelectedConversations.findIndex((x) => x.id == conversation.id), 1);
      return;

    }

    this.SelectedConversations.push(conversation);
  }

}
