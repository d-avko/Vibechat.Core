import { Component, Inject } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../UiComponents/Chat/chat.component";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { AppUser } from "../Data/AppUser";
import { ChatsService } from "../Services/ChatsService";
import { AuthService } from "../Auth/AuthService";

export interface ChatUsersDialogData {
  conversationId: number;
}

@Component({
  selector: 'chat-users-dialog',
  templateUrl: 'chat-users-dialog.html',
})
export class ChatUsersDialogComponent {

  public usernameToFind: string;

  public FoundUsers = new Array<AppUser>();

  public SelectedUser: AppUser;

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ChatUsersDialogData, public chats: ChatsService, public snackBar: SnackBarHelper, public auth: AuthService) { }

  public async OnFindUsers(): Promise<void> {

    if (this.usernameToFind == '' || this.usernameToFind == null) {
      this.snackBar.openSnackBar('Please enter a username in search bar.', 2);
      return;
    }

    let users = await this.chats.FindUsersInChat(this.usernameToFind, this.data.conversationId);

    if (!users) {
      this.snackBar.openSnackBar('Noone was found.', 2);

      this.FoundUsers = new Array<AppUser>();

    } else {
      users = users.filter(x => x.id != this.auth.User.id);
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

  public IsUserSelected(user: AppUser): boolean {
    if (!this.SelectedUser) {
      return false;
    }

    return this.SelectedUser.id == user.id;
  }

  public SelectUser(user: AppUser) {
    if (this.IsUserSelected(user)) {
      this.SelectedUser = null;
      return;
    }

    this.SelectedUser = user;
  }

}
