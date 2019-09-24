import {AppUser} from "../Data/AppUser";
import {Component, Inject} from "@angular/core";
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material";
import {ChatComponent} from "../UiComponents/Chat/chat.component";
import {Chat} from "../Data/Chat";
import {ChatRole} from "../Roles/ChatRole";
import {AuthService} from "../Services/AuthService";

export interface AdminPanelDialogData {
  user: AppUser;
  chat: Chat;
  kickFunc: (user: AppUser) => void;
  banFunc: (user: AppUser) => Promise<boolean>;
  unBanFunc: (user: AppUser) => Promise<boolean>;
  makeModerFunc: (userId: string, chatId: number) => Promise<boolean>;
  removeModerFunc: (userId: string, chatId: number) => Promise<boolean>;
}

@Component({
  selector: 'admin-panel-dialog',
  templateUrl: 'admin-panel-dialog.html',
})
export class AdminPanelDialog {
  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    public auth: AuthService,
  @Inject(MAT_DIALOG_DATA) public data: AdminPanelDialogData) { }

  public async Ban() {
    let result = await this.data.banFunc(this.data.user);

    if (result) {
      this.data.user.isBlockedInConversation = false;
    }
  }

  public async UnBan() {
    let result = await this.data.unBanFunc(this.data.user);

    if (result) {
      this.data.user.isBlockedInConversation = false;
    }
  }

  public IsBanAvailable() {
    return this.data.chat.chatRole.role == ChatRole.Creator
      || this.data.user.chatRole.role == ChatRole.NoRole;
  }

  public Kick() {
    this.data.kickFunc(this.data.user);
    this.dialogRef.close();
  }

  public IsModerator() {
    return this.data.user.chatRole.role == ChatRole.Moderator;
  }

  public IsCurrentUserCreator() {
    return this.data.chat.chatRole.role == ChatRole.Creator;
  }

  public async MakeModerator() {
    let result = await this.data.makeModerFunc(this.data.user.id, this.data.chat.id);

    if (result) {
      this.data.user.chatRole.role = ChatRole.Moderator;
    }
  }

  public async RemoveModerator() {
    const result = await this.data.removeModerFunc(this.data.user.id, this.data.chat.id);

    if (result) {
      this.data.user.chatRole.role = ChatRole.NoRole;
    }
  }
}
