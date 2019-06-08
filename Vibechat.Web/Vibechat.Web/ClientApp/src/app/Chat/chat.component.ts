import { Component, ViewChild, OnInit, Type } from "@angular/core";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { AuthService } from "../Auth/AuthService";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { MatSnackBar, MatDialog, MatDrawer } from "@angular/material";
import { ChatMessage } from "../Data/ChatMessage";
import { trigger, state, style, transition, animate } from "@angular/animations";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { MessagesComponent } from "../Conversation/Messages/messages.component";
import { UserInfo } from "../Data/UserInfo";
import { FindUsersDialogComponent } from "../Dialogs/FindUsersDialog";
import { AddGroupDialogComponent } from "../Dialogs/AddGroupDialog";
import { GroupInfoDialogComponent } from "../Dialogs/GroupInfoDialog";
import { SearchListComponent } from "../Search/searchlist.component";
import { UserInfoDialogComponent } from "../Dialogs/UserInfoDialog";
import { ViewAttachmentsDialogComponent } from "../Dialogs/ViewAttachmentsDialog";
import { ForwardMessagesDialogComponent } from "../Dialogs/ForwardMessagesDialog";
import { UsersService } from "../Services/UsersService";
import { ConversationsService } from "../Services/ConversationsService";

@Component({
  selector: 'chat-root',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css'],
  animations: [
    trigger('slideIn', [
      state('*', style({ 'overflow-y': 'hidden' })),
      state('void', style({ 'overflow-y': 'hidden' })),
      transition('* => void', [
        style({ height: '*' }),
        animate(250, style({ height: 0 }))
      ]),
      transition('void => *', [
        style({ height: '0' }),
        animate(250, style({ height: '*' }))
      ])
    ])
  ]
})
export class ChatComponent implements OnInit {

  public SearchString: string;

  @ViewChild(MessagesComponent) messages: MessagesComponent;
  @ViewChild(MatDrawer) sideDrawer: MatDrawer;
  @ViewChild(SearchListComponent) searchList: SearchListComponent;

  constructor(
    public dialog: MatDialog,
    public formatter: ConversationsFormatter,
    private auth: AuthService,
    private usersService: UsersService,
    private conversationsService: ConversationsService) { }

  async ngOnInit(): Promise<void> {
    await this.auth.TryAuthenticate();

    this.UpdateConversations();

    await this.usersService.UpdateUserInfo(this.auth.User.id);
  }

  public OnForwardMessages(values: Array<ChatMessage>) {
    let forwardMessagesDialog = this.dialog.open(
      ForwardMessagesDialogComponent,
      {
        data: {
          conversations: this.conversationsService.Conversations
        }
      }
    );

    forwardMessagesDialog
      .beforeClosed()
      .subscribe((result: Array<ConversationTemplate>) => {

        this.conversationsService.ForwardMessagesTo(result, values);
      })
  }

  public async OnViewGroupInfo(group: ConversationTemplate) : Promise<void> {

    if (!group.isGroup) {
      this.OnViewUserInfo(group.dialogueUser);
      return;
    }

    await this.conversationsService.UpdateExisting(group);

    const groupInfoRef = this.dialog.open(GroupInfoDialogComponent, {
      width: '450px',
      data: {
        Conversation: group,
        user: this.auth.User,
        ExistsInThisGroup: this.conversationsService.ExistsIn(group.conversationID)
      }
    });

    groupInfoRef.componentInstance
      .OnInviteUsers
      .subscribe((group: ConversationTemplate) => this.OnInviteUsersToGroup(group));

    groupInfoRef.componentInstance
      .OnChangeName
      .subscribe(async (name: string) => await this.OnChangeConversationName(name));

    groupInfoRef.componentInstance
      .OnLeaveGroup
      .subscribe((group: ConversationTemplate) => {
        this.OnLeaveGroup(group);
        groupInfoRef.close();
      });

    groupInfoRef.componentInstance
      .OnChangeThumbnail
      .subscribe(async (file: File) => await this.OnChangeGroupThumbnail(file));

    groupInfoRef.componentInstance
      .OnClearMessages
      .subscribe((async (group: ConversationTemplate) => {
        await this.OnRemoveAllMessages(group);
        groupInfoRef.close();
      }));

    groupInfoRef.componentInstance
      .OnViewUserInfo
      .subscribe((user: UserInfo) => this.OnViewUserInfo(user));

    groupInfoRef.componentInstance
      .OnJoinGroup
      .subscribe((group: ConversationTemplate) => {
        groupInfoRef.close();
        this.OnJoinGroup(group);
      });

    groupInfoRef.componentInstance.OnKickUser
      .subscribe((user: UserInfo) => this.OnKickUser(user));

    groupInfoRef.componentInstance.OnBanUser
      .subscribe(async (user: UserInfo) => await this.BanFromConversation(user));

    groupInfoRef.componentInstance.OnUnBanUser
      .subscribe(async (user: UserInfo) => await this.UnbanFromConversation(user));

    groupInfoRef.componentInstance.OnRemoveGroup
      .subscribe((group: ConversationTemplate) => {
       this.OnRemoveGroup(group)
      });

    groupInfoRef.componentInstance.OnViewAttachments
      .subscribe((group: ConversationTemplate) => {
        this.ViewAttachments(group);
      });

  
  }

  public OnRemoveGroup(group: ConversationTemplate) {
    this.conversationsService.RemoveGroup(group);
  }

  public OnKickUser(user: UserInfo) {
    this.conversationsService.KickUser(user);
  }

  public async UnbanFromConversation(user: UserInfo) {
    await this.conversationsService.UnbanFromConversation(user);
  }

  public async BanFromConversation(userToBan: UserInfo) {
    await this.conversationsService.BanFromConversation(userToBan);
  }

  public OnJoinGroup(conversation: ConversationTemplate) {
    this.SearchString = '';
    this.conversationsService.JoinGroup(conversation);
  }

  public async OnViewUserInfo(user: UserInfo) {
    await this.usersService.UpdateUserInfo(user.id);

    const userInfoRef = this.dialog.open(UserInfoDialogComponent, {
      width: '450px',
      data: {
        user: user,
        currentUser: this.auth.User,
        Conversations: this.conversationsService.Conversations
      }
    });

    userInfoRef.componentInstance.OnBlockUser
      .subscribe(async (user: UserInfo) => {
        await this.BlockUser(user);
      });

    userInfoRef.componentInstance.OnUnblockUser
      .subscribe(async (user: UserInfo) => {
        await this.OnUnblockUser(user);
      });

    userInfoRef.componentInstance.OnChangeLastname
      .subscribe(async (lastName: string) => {

        await this.ChangeThisUserLastname(lastName);
      });

    userInfoRef.componentInstance.OnChangeName
      .subscribe(async (name: string) => {
        await this.ChangeThisUserFirstname(name);
      });

    userInfoRef.componentInstance.OnCreateDialogWith
      .subscribe((user: UserInfo) => {
        this.CreateDialogWith(user);
      });

    userInfoRef.componentInstance.OnRemoveDialogWith
      .subscribe((user: UserInfo) => {
        this.RemoveDialogWith(user);
        userInfoRef.close();
      });

    userInfoRef.componentInstance.OnUpdateProfilePicture
      .subscribe(async (file: File) => {
        await this.UpdateProfilePicture(file);
      });

    userInfoRef.componentInstance.OnViewAttachmentsOf
      .subscribe((user: UserInfo) => {
        this.ViewAttachmentsOf(user)
      });
  }

  public async UpdateProfilePicture(file: File) {
    await this.usersService.UpdateProfilePicture(file);
  }

  public RemoveDialogWith(user: UserInfo) {
    this.conversationsService.RemoveDialogWith(user);
  }

  public CreateDialogWith(user: UserInfo) {
    this.conversationsService.CreateDialogWith(user);
  }

  public async ChangeThisUserLastname(name: string) {
    await this.usersService.ChangeLastname(name);
  }

  public async ChangeThisUserFirstname(name: string) {
    await this.usersService.ChangeName(name);
  }

  public async BlockUser(userToBan: UserInfo) {

    await this.usersService.BlockUser(userToBan);
  }

  public ViewAttachmentsOf(user: UserInfo) {
    let conversation = this.conversationsService.FindDialogWith(user);

    if (conversation) {
      const attachmentsDialogRef = this.dialog.open(ViewAttachmentsDialogComponent, {
        width: '450px',
        data: {
          conversation: conversation
        }
      });
    }

  }

  public ViewAttachments(conversation: ConversationTemplate) {
    if (conversation) {
      const attachmentsDialogRef = this.dialog.open(ViewAttachmentsDialogComponent, {
        width: '450px',
        data: {
          conversation: conversation
        }
      });
    }
  }

  public async OnUnblockUser(user: UserInfo) {
    await this.usersService.UnblockUser(user);
  } 

  public async OnRemoveAllMessages(group: ConversationTemplate) : Promise<void> {
    await this.conversationsService.RemoveAllMessages(group);
  }

  public async Search() {
    if (this.SearchString == null || this.SearchString == '') {
      return;
    }

    await this.searchList.Search();
  }

  public async OnChangeGroupThumbnail(file: File): Promise<void>{
    await this.conversationsService.ChangeThumbnail(file);
  }

  public OnLeaveGroup(conversation: ConversationTemplate){
    this.conversationsService.Leave(conversation);
  }

  public async OnChangeConversationName(name: string) : Promise<void> {
    await this.conversationsService.ChangeConversationName(name);
  }

  public OnInviteUsersToGroup(group: ConversationTemplate) {

    const dialogRef = this.dialog.open(FindUsersDialogComponent, {
      width: '350px',
      data: {
        conversationId: group.conversationID
      }
    });

    dialogRef.beforeClosed().subscribe(users => {

      this.conversationsService.InviteUsersToGroup(users, group);
    });
  }

  public CreateGroup() {

    this.sideDrawer.close();

    const dialogRef = this.dialog.open(AddGroupDialogComponent, {
      width: '250px'
    });


    dialogRef.beforeClosed().subscribe(async result => {

      if (result.name === '' || result.name == null) {
        return;
      }

      await this.conversationsService.CreateGroup(result.name, result.isPublic);    
    });
  }

  public async OnSendMessage(message: string) {
    await this.conversationsService.SendMessage(message);
  }
 
  public ReadMessage(message: ChatMessage) {
    this.conversationsService.ReadMessage(message);
  }

  public IsConversationSelected(): boolean {
    return this.conversationsService.IsConversationSelected();
  }


  public async UpdateConversations() {
    await this.conversationsService.UpdateConversations();
  }

  public async ChangeConversation(conversation: ConversationTemplate) {
    //if we were on search screen, we should hide it

    this.SearchString = '';

    await this.conversationsService.ChangeConversation(conversation);
  }

  public async OnUploadImages(files: FileList) {
    await this.conversationsService.UploadImages(files);
  }
}
