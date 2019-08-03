import { UserInfo } from "../Data/UserInfo";
import { Inject, Component } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ChatRole } from "../Roles/ChatRole";
import { AuthService } from "../Auth/AuthService";

export interface AdminPanelDialogData {
  user: UserInfo;
  chat: ConversationTemplate;
  kickFunc: (user: UserInfo) => void;
  banFunc: (user: UserInfo) => Promise<boolean>;
  unBanFunc: (user: UserInfo) => Promise<boolean>;
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
    let result = await this.data.makeModerFunc(this.data.user.id, this.data.chat.conversationID);

    if (result) {
      this.data.user.chatRole.role = ChatRole.Moderator;
    }
  }

  public async RemoveModerator() {
    let result = await this.data.removeModerFunc(this.data.user.id, this.data.chat.conversationID);

    if (result) {
      this.data.user.chatRole.role = ChatRole.NoRole;
    }
  }
}
