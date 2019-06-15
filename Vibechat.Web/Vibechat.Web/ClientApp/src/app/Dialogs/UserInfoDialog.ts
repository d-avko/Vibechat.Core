import { Component, Inject, EventEmitter, Input, Output } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { UserInfo } from "../Data/UserInfo";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { ChangeNameDialogComponent } from "./ChangeNameDialog";
import { ConversationsService } from "../Services/ConversationsService";
import { UsersService } from "../Services/UsersService";
import { ViewAttachmentsDialogComponent } from "./ViewAttachmentsDialog";

export interface UserInfoData {
  user: UserInfo;
  currentUser: UserInfo;
  Conversations: Array<ConversationTemplate>;
}

@Component({
  selector: 'user-info-dialog',
  templateUrl: 'user-info-dialog.html',
})
export class UserInfoDialogComponent {

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: UserInfoData,
    public dialog: MatDialog,
    public formatter: ConversationsFormatter,
    public conversationsService: ConversationsService,
    public usersService: UsersService
    ) { }

  public HasConversationWith() : boolean {
    if (this.data.Conversations == null) {
      return false;
    }

    return this.conversationsService.FindDialogWith(this.data.user) != null;
  }

  public DeleteConversation(): void {
    this.conversationsService.RemoveDialogWith(this.data.user);
    this.dialogRef.close();
  }

  public CreateDialog(): void {
    this.conversationsService.CreateDialogWith(this.data.user, false);
  }

  public IsInContactList(): boolean {
    return this.usersService.HasContactWith(this.data.user)
  }

  public async RemoveFromContacts() {
    await this.usersService.RemoveFromContacts(this.data.user);
  }

  public async AddToContacts() {
    await this.usersService.AddToContacts(this.data.user);
  }

  public ViewAttachments() {
    let conversation = this.conversationsService.FindDialogWith(this.data.user);

    if (conversation) {
      const attachmentsDialogRef = this.dialog.open(ViewAttachmentsDialogComponent, {
        width: '450px',
        data: {
          conversation: conversation
        }
      });
    }
  }

  public async Block() {
    await this.usersService.BlockUser(this.data.user);
  }

  public async UnBlock() {
    await this.usersService.UnblockUser(this.data.user);
  }

  public async UpdateThumbnail(event: any) {
    await this.usersService.UpdateProfilePicture(event.target.files[0]);
  }

  public ChangeName(): void {
    const groupInfoRef = this.dialog.open(ChangeNameDialogComponent, {
      width: '450px'
    });

    groupInfoRef.afterClosed().subscribe(
      async (name) => {
        if (!name) {
          return;
        }

        await this.usersService.ChangeName(name);
      }
    )
  }

  public ChangeLastname(): void {
    const groupInfoRef = this.dialog.open(ChangeNameDialogComponent, {
      width: '450px'
    });

    groupInfoRef.afterClosed().subscribe(
      async (name) => {
        if (!name) {
          return;
        }

        await this.usersService.ChangeLastname(name);
      }
    )
  }
}
