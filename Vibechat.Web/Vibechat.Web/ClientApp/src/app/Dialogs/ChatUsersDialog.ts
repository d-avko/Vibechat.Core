import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { UserInfo } from "../Data/UserInfo";
import { UsersService } from "../Services/UsersService";
import { ChatsService } from "../Services/ChatsService";
import { ChatRole } from "../Roles/ChatRole";

export interface ChatUsersDialogData {
  conversationId: number;
}

@Component({
  selector: 'chat-users-dialog',
  templateUrl: 'chat-users-dialog.html',
})
export class ChatUsersDialogComponent {

  public usernameToFind: string;

  public FoundUsers = new Array<UserInfo>();

  public SelectedUser: UserInfo;

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ChatUsersDialogData, public chats: ChatsService, public snackBar: SnackBarHelper) { }

  public async OnFindUsers(): Promise<void> {

    if (this.usernameToFind == '' || this.usernameToFind == null) {
      this.snackBar.openSnackBar('Please enter a username in search bar.', 2);
      return;
    }

    let users = await this.chats.FindUsersInChat(this.usernameToFind, this.data.conversationId);

    if (!users) {
      this.snackBar.openSnackBar('Noone was found.', 2);

      this.FoundUsers = new Array<UserInfo>();

    } else {
      this.FoundUsers = [...users];
    }

    if (this.SelectedUser) {
      this.SelectedUser = null;
    }

    this.usernameToFind = '';
  }

  public onCancelClick() {
    this.dialogRef.close();
  }

  public IsUserSelected(user: UserInfo): boolean {
    if (!this.SelectedUser) {
      return false;
    }

    return this.SelectedUser.id == user.id;
  }

  public SelectUser(user: UserInfo) {
    if (this.IsUserSelected(user)) {
      this.SelectedUser = null;
      return;
    }

    this.SelectedUser = user;
  }

}
