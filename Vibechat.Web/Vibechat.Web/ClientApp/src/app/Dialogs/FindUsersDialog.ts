import {Component, Inject} from "@angular/core";
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material";
import {ChatComponent} from "../UiComponents/Chat/chat.component";
import {SnackBarHelper} from "../Snackbar/SnackbarHelper";
import {AppUser} from "../Data/AppUser";
import {UsersService} from "../Services/UsersService";
import {MessageReportingService} from "../Services/MessageReportingService";

export interface InviteUsersData {
  conversationId: number;
  isMultiSelect: boolean;
}

@Component({
  selector: 'find-users-dialog',
  templateUrl: 'find-users-dialog.html',
})
export class FindUsersDialogComponent {

  public usernameToFind: string;

  public FoundUsers = new Array<AppUser>();

  public SelectedUsers = new Array<AppUser>();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: InviteUsersData,
    public usersService: UsersService,
    public snackBar: SnackBarHelper,
    public messages: MessageReportingService) {
    this.SelectedUsers = new Array<AppUser>();
  }

  public async OnFindUsers(): Promise<void> {

    if (this.usernameToFind == '' || this.usernameToFind == null) {
      return;
    }

    let users = await this.usersService.FindUsersByUsername(this.usernameToFind);

    if (users == null) {
      this.messages.SearchUsersNoResults();

      this.FoundUsers = new Array<AppUser>();

    } else {
      this.FoundUsers = [...users];
    }

    if (this.SelectedUsers.length != 0) {
      this.SelectedUsers.splice(0, this.SelectedUsers.length);
    }

    this.usernameToFind = '';
  }

  public onCancelClick() {
    this.dialogRef.close();
  }

  public IsUserSelected(user: AppUser) : boolean {
    return this.SelectedUsers.findIndex((x) => x.id == user.id) != -1;
  }

  public SelectUser(user: AppUser) {
    if (this.IsUserSelected(user)) {

      this.SelectedUsers.splice(this.SelectedUsers.findIndex((x) => x.id == user.id), 1);
      return;

    }

    if (!this.data.isMultiSelect) {
      this.SelectedUsers.splice(0, 1);
    }

    this.SelectedUsers.push(user);
  }

}
