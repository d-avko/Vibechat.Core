import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";

export interface CreateGroupData {
  name: string;
}

@Component({
  selector: 'add-group-dialog',
  templateUrl: 'add-group-dialog.html',
})
export class AddGroupDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CreateGroupData) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

}
