import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { UserInfo } from "../Data/UserInfo";

export interface InviteUsersData {
  conversationId: number;
  requestsBuilder: ApiRequestsBuilder;
  snackbar: SnackBarHelper;
  token: string;
}

@Component({
  selector: 'find-users-dialog',
  templateUrl: 'find-users-dialog.html',
})
export class FindUsersDialogComponent {

  public usernameToFind: string;

  public FoundUsers = new Array<UserInfo>();

  public SelectedUsers = new Array<UserInfo>();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: InviteUsersData) { }

  public OnFindUsers(): void {
    this.data.requestsBuilder.FindUsersByUsername(this.data.token, this.usernameToFind)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          this.data.snackbar.openSnackBar(result.errorMessage);
          return;
        }

        if (result.response.usersFound == null) {
          this.data.snackbar.openSnackBar('Noone was found.', 2);
        }

        this.FoundUsers = [...result.response.usersFound]; 
        this.SelectedUsers = new Array<UserInfo>();
      });

    this.usernameToFind = '';
  }

  public OnInvite() {
    this.dialogRef.close(this.SelectedUsers);
  }

  public IsUserSelected(user: UserInfo) : boolean {
    return this.SelectedUsers.findIndex((x) => x.id == user.id) != -1;
  }

  public SelectUser(user: UserInfo) {
    if (this.IsUserSelected(user)) {

      this.SelectedUsers = this.SelectedUsers.slice(this.SelectedUsers.findIndex((x) => x.id == user.id), 1);
      return;

    }

    this.SelectedUsers.push(user);
  }

}
