import { Component, Inject, EventEmitter, Input, Output } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { UserInfo } from "../Data/UserInfo";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { ChangeNameDialogComponent } from "./ChangeNameDialog";

export interface UserInfoData {
  user: UserInfo;
  currentUser: UserInfo;
}

@Component({
  selector: 'user-info-dialog',
  templateUrl: 'user-info-dialog.html',
})
export class UserInfoDialogComponent {

  @Input() public Conversations: Array<ConversationTemplate>;

  public OnRemoveDialogWith = new EventEmitter<UserInfo>();

  public OnCreateDialogWith = new EventEmitter<UserInfo>();

  public OnBlockUser = new EventEmitter<UserInfo>();

  public OnUpdateProfilePicture = new EventEmitter<File>();

  public OnChangeName = new EventEmitter<string>();

  public OnChangeLastname = new EventEmitter<string>();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UserInfoData, public ChangeNameDialog: MatDialog, public formatter: ConversationsFormatter) { }

  public HasConversationWith() : boolean {
    if (this.Conversations == null) {
      return false;
    }

    return this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == this.data.user.id) != null;
  }

  public DeleteConversation() : void {
    this.OnRemoveDialogWith.emit(this.data.user);
  }

  public CreateDialog(): void {
    this.OnCreateDialogWith.emit(this.data.user);
  }

  public Block(): void {
    this.OnBlockUser.emit(this.data.user);
  }

  public UpdateThumbnail(event: any): void {
    this.OnUpdateProfilePicture.emit(event.target.files[0]);
  }

  public ChangeName(): void {
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

  public ChangeLastname(): void {
    const groupInfoRef = this.ChangeNameDialog.open(ChangeNameDialogComponent, {
      width: '450px'
    });

    groupInfoRef.afterClosed().subscribe(
      (name) => {
        if (name == null || name == '') {
          return;
        }

        this.OnChangeLastname.emit(name);
      }
    )
  }
}
