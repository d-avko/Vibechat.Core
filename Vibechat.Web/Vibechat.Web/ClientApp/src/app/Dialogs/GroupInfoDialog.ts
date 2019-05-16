import { Component, Inject, EventEmitter } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { UserInfo } from "../Data/UserInfo";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";

export interface  GroupInfoData {
  Conversation: ConversationTemplate;
  user: UserInfo;
  ExistsInThisGroup: boolean;
}

@Component({
  selector: 'group-info-dialog',
  templateUrl: 'group-info-dialog.html',
})
export class GroupInfoDialogComponent {

  public OnKickUser = new EventEmitter<UserInfo>();

  public OnClearMessages = new EventEmitter<void>();

  public OnChangeName = new EventEmitter<string>();

  public OnChangeThumbnail = new EventEmitter<File>();

  public OnLeaveGroup = new EventEmitter<void>();

  public OnInviteUsers = new EventEmitter<void>();

  public OnViewUserInfo = new EventEmitter<UserInfo>();

  public OnJoinGroup = new EventEmitter<ConversationTemplate>();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GroupInfoData, public formatter: ConversationsFormatter) { }

  onNoClick(): void {
    this.dialogRef.close();
  }

  public ViewUserInfo(user: UserInfo) {
    this.OnViewUserInfo.emit(user);
  }

  public IsJoined() {
    return this.data.ExistsInThisGroup;
  }

  public ClearMessages(){
    this.OnClearMessages.emit(null);
  }

  public JoinGroup() {
    this.OnJoinGroup.emit(null);
  }

  public LeaveGroup() {
    this.OnLeaveGroup.emit(null);
  }

  public UpdateThumbnail(event: any) {
    return event.target.Files[0];
  }

  public ChangeName() {
    this.OnChangeName.emit(null);
  }

  public InviteUsers() {
    this.OnInviteUsers.emit(null);
  }

}
