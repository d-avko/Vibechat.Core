import { Component, ViewChild } from "@angular/core";
import { MatDialogRef, MatCheckbox } from "@angular/material";
import { ChatComponent } from "../UiComponents/Chat/chat.component";

@Component({
  selector: 'add-group-dialog',
  templateUrl: 'add-group-dialog.html',
})
export class AddGroupDialogComponent {

  public GroupName: string;

  @ViewChild(MatCheckbox, { static: true }) public IsPublic: MatCheckbox;

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

  public CreateGroup() {
    this.dialogRef.close({ name: this.GroupName, isPublic: this.IsPublic.checked });
  }

}
