import { Component, Inject, EventEmitter, Input, Output, ViewContainerRef } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { UserInfo } from "../Data/UserInfo";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { ChangeNameDialogComponent } from "./ChangeNameDialog";
import { ChatsService } from "../Services/ChatsService";
import { UsersService } from "../Services/UsersService";
import { ViewAttachmentsDialogComponent } from "./ViewAttachmentsDialog";
import { AuthService } from "../Auth/AuthService";
import { ViewPhotoService } from "./ViewPhotoService";

export interface UserInfoData {
  user: UserInfo;
  currentUser: UserInfo;
  conversation: ConversationTemplate;
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
    public conversationsService: ChatsService,
    public usersService: UsersService,
    public auth: AuthService,
    public viewContainerRef: ViewContainerRef,
    public photos: ViewPhotoService
  ){
    //this check is needed for changes in name/lastname to be displayed correctly.
    if (this.data.user.id == auth.User.id) {
      this.data.user = auth.User;
    }

    this.photos.viewContainerRef = this.viewContainerRef;
  }

  public HasConversationWith(): boolean {

    if (!this.data.conversation) {
      //we are viewing user info outside the chat.
      return this.conversationsService.FindDialogWithSecurityCheck(this.data.user, false) != null;
    }
    //we are viewing user info inside the chat.
    return this.conversationsService.FindDialogWithSecurityCheck(this.data.user, this.data.conversation.isSecure) != null;
  }

  public DeleteConversation(): void {
    this.conversationsService.RemoveGroup(this.data.conversation);
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

  public ViewPicture() {
    this.photos.ViewProfilePicture(this.data.user.fullImageUrl);
  }

  public ViewAttachments() {
    if (this.data.conversation) {
      const attachmentsDialogRef = this.dialog.open(ViewAttachmentsDialogComponent, {
        width: '450px',
        data: {
          conversation: this.data.conversation
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

  public async UpdateThumbnail(event: Event) {
    await this.usersService.UpdateProfilePicture((<HTMLInputElement>event.target).files[0]);
    this.ResetInput(<HTMLInputElement>event.target);
  }

  public ResetInput(input: HTMLInputElement) {
    input.value = '';

    if (!/safari/i.test(navigator.userAgent)) {
      input.type = '';
      input.type = 'file';
    }
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

  public ChangeUsername(): void {
    const groupInfoRef = this.dialog.open(ChangeNameDialogComponent, {
      width: '450px'
    });

    groupInfoRef.afterClosed().subscribe(
      async (name) => {
        if (!name) {
          return;
        }

        await this.usersService.ChangeUsername(name);
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
