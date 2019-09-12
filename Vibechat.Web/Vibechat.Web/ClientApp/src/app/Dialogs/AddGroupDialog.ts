import {Component, ViewChild} from '@angular/core';
import {MatCheckbox, MatDialogRef} from '@angular/material';
import {ChatComponent} from '../UiComponents/Chat/chat.component';
import {FormControl, Validators} from '@angular/forms';
import {ChatsService} from '../Services/ChatsService';

@Component({
  selector: 'add-group-dialog',
  templateUrl: 'add-group-dialog.html',
})
export class AddGroupDialogComponent {

  public GroupName: FormControl;

  @ViewChild(MatCheckbox, { static: true }) public IsPublic: MatCheckbox;

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>) {
    this.GroupName =
      new FormControl('', Validators.compose(
        [Validators.required, Validators.minLength(ChatsService.MinGroupNameLength)]));
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  public CreateGroup() {
    if (!this.GroupName.valid) {
      return;
    }
    this.dialogRef.close({ name: this.GroupName.value, isPublic: this.IsPublic.checked });
  }

}
