import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";

export interface CreateGroupData {
  name: string;
}

@Component({
  selector: 'change-name-dialog',
  templateUrl: 'change-name-dialog.html',
})
export class ChangeNameDialogComponent {

  public groupName: string;

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>) { }

  onCancelClick(): void {
    this.dialogRef.close();
  }

}
