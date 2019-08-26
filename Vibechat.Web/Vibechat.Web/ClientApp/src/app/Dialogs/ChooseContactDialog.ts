import {Component} from "@angular/core";
import {MatDialogRef} from "@angular/material";
import {ChatComponent} from "../UiComponents/Chat/chat.component";
import {AppUser} from "../Data/AppUser";
import {AuthService} from "../Services/AuthService";

@Component({
  selector: 'choose-contact-dialog',
  templateUrl: 'choose-contact-dialog.html',
})
export class ChooseContactDialogComponent {

  public SelectedUser = null;

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    public auth: AuthService) { }

    public onCancelClick() {
      this.dialogRef.close(null);
    }

  public SelectUser(user: AppUser) {
    if (!this.SelectedUser) {
      this.SelectedUser = user;
      return;
    }

    if (user.id == this.SelectedUser.id) {
      this.SelectedUser = null;
      return;
    }

    this.SelectedUser = user;
  }

}
