import {Component, EventEmitter, Inject, ViewContainerRef} from "@angular/core";
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material";
import {ChatComponent} from "../Chat/chat.component";
import {Chat} from "../Data/Chat";
import {UserInfo} from "../Data/UserInfo";
import {ConversationsFormatter} from "../Formatters/ConversationsFormatter";
import {ChangeNameDialogComponent} from "./ChangeNameDialog";
import {FindUsersDialogComponent} from "./FindUsersDialog";
import {ChatsService} from "../Services/ChatsService";
import {ViewAttachmentsDialogComponent} from "./ViewAttachmentsDialog";
import {ViewPhotoService} from "./ViewPhotoService";
import {ChatRole} from "../Roles/ChatRole";
import {ChatUsersDialogComponent} from "./ChatUsersDialog";
import {AdminPanelDialog} from "./AdminPanelDialog";
import {UsersService} from "../Services/UsersService";
import {AuthService} from "../Auth/AuthService";

export interface  GroupInfoData {
  Conversation: Chat;
  user: UserInfo;
  ExistsInThisGroup: boolean;
}

@Component({
  selector: 'group-info-dialog',
  templateUrl: 'group-info-dialog.html',
})
export class GroupInfoDialogComponent {

  public OnViewUserInfo = new EventEmitter<UserInfo>();

  public OnJoinGroup = new EventEmitter<Chat>();

  constructor(
    public dialogRef: MatDialogRef<ChatComponent>,
    public dialog: MatDialog,
    public anotherDialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: GroupInfoData,
    public formatter: ConversationsFormatter,
    public chats: ChatsService,
    public users: UsersService,
    public ChangeNameDialog: MatDialog,
    public photos: ViewPhotoService,
    public auth: AuthService,
    public viewContainerRef: ViewContainerRef) { this.photos.viewContainerRef = this.viewContainerRef }

  public uploadProgress: number = 0;

  public uploading: boolean = false;

  public ChooseUser() {
    const chatUsersDialogRef = this.dialog.open(ChatUsersDialogComponent, {
      width: '450px',
      data: {
        conversationId: this.data.Conversation.id
      }
    }).beforeClosed().subscribe((user) => {

      if (!user) {
        return;
      }

      //show admin panel, pass user
      const adminPanelRef = this.anotherDialog.open(AdminPanelDialog, {
        width: '450px',
        data: {
          user: user,
          chat: this.data.Conversation,
          banFunc: this.BanUser.bind(this),
          kickFunc: this.KickUser.bind(this),
          unBanFunc: this.UnbanUser.bind(this),
          makeModerFunc: this.MakeModerator.bind(this),
          removeModerFunc: this.RemoveModerator.bind(this)
        }
      });
    });
  }

  public async MakeModerator(userId: string, chatId: number) {
    return await this.chats.MakeUserModerator(chatId, userId);
  }

  public async RemoveModerator(userId: string, chatId: number) {
    return await this.chats.RemoveModerator(chatId, userId);
  }

  public ViewUserInfo(user: UserInfo) {
    this.OnViewUserInfo.emit(user);
  }

  public IsJoined() {
    return this.data.ExistsInThisGroup;
  }

  public async ClearMessages() {
    await this.chats.RemoveAllMessages(this.data.Conversation);
    this.dialogRef.close();
  }

  public RemoveGroup() {
    this.chats.RemoveGroup(this.data.Conversation);
    this.dialogRef.close();
  }

  public JoinGroup() {
    this.OnJoinGroup.emit(this.data.Conversation);
  }

  public LeaveGroup() {
    this.chats.Leave(this.data.Conversation);
    this.dialogRef.close();
  }

  public IsModeratorOrCreator() {
    return this.data.Conversation.chatRole.role == ChatRole.Moderator
      || this.data.Conversation.chatRole.role == ChatRole.Creator;
  }

  public KickUser(user: UserInfo) {
    this.chats.KickUser(user, this.data.Conversation);

    if (this.auth.User.id == user.id) {
      this.dialog.closeAll();
      this.anotherDialog.closeAll();
    }
  }

  public async BanUser(user: UserInfo) {
    let result = await this.chats.BanFromConversation(user, this.data.Conversation);

    if (!result) {
      return;
    }

    user.isBlockedInConversation = true;
  }

  public async UnbanUser(user: UserInfo) {
    let result = await this.chats.UnbanFromConversation(user, this.data.Conversation);

    if (!result) {
      return;
    }

    user.isBlockedInConversation = false;
  }

  public ResetInput(input: HTMLInputElement) {
    input.value = '';

    if (!/safari/i.test(navigator.userAgent)) {
      input.type = '';
      input.type = 'file';
    }
  }

  public ViewAttachments() {
    const attachmentsDialogRef = this.dialog.open(ViewAttachmentsDialogComponent, {
      width: '450px',
      data: {
        conversation: this.data.Conversation
      }
    });
  }

  public ViewPicture(image: Event) {
    this.photos.viewContainerRef = this.viewContainerRef;
    this.photos.ViewProfilePicture(this.data.Conversation.fullImageUrl);
  }

  public IsCurrentUserCreatorOfConversation() {
    return this.data.Conversation.chatRole.role == ChatRole.Creator;
  }

  public ProgressCallback(value: number) {
    this.uploadProgress = value;
  }

  public async UpdateThumbnail(event: Event) {
    try {
      this.uploading = true;
      await this.chats.ChangeThumbnail((<HTMLInputElement>event.target).files[0], this.data.Conversation, this.ProgressCallback.bind(this));
    } finally {
      this.uploading = false;
      this.ResetInput(<HTMLInputElement>event.target);
    }
  }

  public ChangeName() {

    const groupInfoRef = this.ChangeNameDialog.open(ChangeNameDialogComponent, {
      width: '450px'
    });

    groupInfoRef.afterClosed().subscribe(
      async (name) => {
        if (name == null || name == '') {
          return;
        }

        await this.chats.ChangeConversationName(name, this.data.Conversation);
      }
    )
  }

  public InviteUsers() {
    const dialogRef = this.dialog.open(FindUsersDialogComponent, {
      width: '350px',
      data: {
        conversationId: this.data.Conversation.id,
        isMultiSelect: true
      }
    });

    dialogRef.beforeClosed().subscribe(users => {

      this.chats.InviteUsersToGroup(users, this.data.Conversation);
    });
  }

}
