import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { UserInfo } from "../Data/UserInfo";
import { UsersService } from "../Services/UsersService";
import { AuthService } from "../Auth/AuthService";

export interface InviteUsersData {
  conversationId: number;
}

@Component({
  selector: 'choose-contact-dialog',
  templateUrl: 'choose-contact-dialog.html',
})
export class ChooseContactDialogComponent {

  public SelectedUser = new UserInfo();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    public auth: AuthService) { }

    public onCancelClick() {
      this.dialogRef.close();
    }

  public SelectUser(user: UserInfo) {
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
