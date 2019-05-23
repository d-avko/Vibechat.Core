import { Component, Inject, EventEmitter } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { UserInfo } from "../Data/UserInfo";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { ChangeNameDialogComponent } from "./ChangeNameDialog";

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

  public OnClearMessages = new EventEmitter<ConversationTemplate>();

  public OnChangeName = new EventEmitter<string>();

  public OnChangeThumbnail = new EventEmitter<File>();

  public OnLeaveGroup = new EventEmitter<ConversationTemplate>();

  public OnInviteUsers = new EventEmitter<ConversationTemplate>();

  public OnViewUserInfo = new EventEmitter<UserInfo>();

  public OnJoinGroup = new EventEmitter<ConversationTemplate>();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: GroupInfoData, public formatter: ConversationsFormatter, public ChangeNameDialog: MatDialog) { }

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
    this.OnClearMessages.emit(this.data.Conversation);
  }

  public JoinGroup() {
    this.OnJoinGroup.emit(this.data.Conversation);
  }

  public LeaveGroup() {
    this.OnLeaveGroup.emit(this.data.Conversation);
  }

  public KickUser(user: UserInfo) {
    this.OnKickUser.emit(user);
  }

  public IsCurrentUserCreatorOfConversation() {
    return this.data.user.id == this.data.Conversation.creator.id;
  }

  public UpdateThumbnail(event: any) {
    this.OnChangeThumbnail.emit(event.target.files[0]);
  }

  public ChangeName() {

    const groupInfoRef = this.ChangeNameDialog.open(ChangeNameDialogComponent, {
      width: '450px'
    });

    groupInfoRef.afterClosed().subscribe(
      (name) => {
        if (name == null || name == '') {
          return;
        }

        this.OnChangeName.emit(name);
      }
    )
  }

  public InviteUsers() {
    this.OnInviteUsers.emit(this.data.Conversation);
  }

}
