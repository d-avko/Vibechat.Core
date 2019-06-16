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
import { ThemesService } from "../Theming/ThemesService";
import { ChooseContactDialogComponent } from "../Dialogs/ChooseContactDialog";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";

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
  over: any;

  constructor(
    public dialog: MatDialog,
    public formatter: ConversationsFormatter,
    public auth: AuthService,
    private usersService: UsersService,
    private conversationsService: ConversationsService,
    private themesService: ThemesService,
    private snackBar: SnackBarHelper) { }

  async ngOnInit(): Promise<void> {
    await this.auth.TryAuthenticate();

    await this.UpdateConversations();

    await this.usersService.UpdateUserInfo(this.auth.User.id);

    await this.usersService.UpdateContacts();
  }

  public IsDarkTheme() {
    return this.themesService.currentThemeName == 'dark';
  }

  public SwitchTheme(name: string) {
    this.themesService.changeTheme(name);
  }

  public async OnViewGroupInfo(group: ConversationTemplate) : Promise<void> {

    if (!group.isGroup) {
      this.OnViewUserInfo(group.dialogueUser, group);
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
      .OnViewUserInfo
      .subscribe((user: UserInfo) => this.OnViewUserInfo(user, null));

    groupInfoRef.componentInstance
      .OnJoinGroup
      .subscribe((group: ConversationTemplate) => {
        groupInfoRef.close();
        this.OnJoinGroup(group);
      });
  }

  public OnLogOut(): void {
    this.auth.LogOut();
  }

  public CreateSecureChat() {
    const chooseContactRef = this.dialog.open(ChooseContactDialogComponent, {
      width: '450px'
    });

    chooseContactRef.beforeClosed().subscribe(
      (user) => {
        if (!user) {
          return;
        }

        if (!this.conversationsService.CreateSecureChat(user)) {
          this.snackBar.openSnackBar('Unable to create the chat with this user. Probably there already exists one.', 3);
        }
      }
    )
  }

  public OnJoinGroup(conversation: ConversationTemplate) {
    this.SearchString = '';
    this.conversationsService.JoinGroup(conversation);
  }

  public async OnViewUserInfo(user: UserInfo, chat: ConversationTemplate) {
    await this.usersService.UpdateUserInfo(user.id);

    const userInfoRef = this.dialog.open(UserInfoDialogComponent, {
      width: '450px',
      data: {
        user: user,
        currentUser: this.auth.User,
        conversation: chat
      }
    });
  }

  public CreateDialogWith(user: UserInfo) {
    this.conversationsService.CreateDialogWith(user, false);
  }

  public async Search() {
    if (this.SearchString == null || this.SearchString == '') {
      return;
    }

    await this.searchList.Search();
  }

  public async OnChangeGroupThumbnail(file: File): Promise<void>{
    await this.conversationsService.ChangeThumbnail(file, this.conversationsService.CurrentConversation);
  }

  public async OnChangeConversationName(name: string) : Promise<void> {
    await this.conversationsService.ChangeConversationName(name, this.conversationsService.CurrentConversation);
  }

  public CreateGroup() {

    this.sideDrawer.close();

    const dialogRef = this.dialog.open(AddGroupDialogComponent, {
      width: '250px'
    });


    dialogRef.beforeClosed().subscribe(async result => {

      if (!result || !result.name) {
        return;
      }

      await this.conversationsService.CreateGroup(result.name, result.isPublic);    
    });
  }

  public IsConversationSelected(): boolean {
    return this.conversationsService.IsConversationSelected();
  }

  public async UpdateConversations() {
    await this.conversationsService.UpdateConversations();
  }

  public async ChangeConversationTo(conversation: ConversationTemplate) {
    this.SearchString = '';
    await this.conversationsService.ChangeConversation(null);
    await this.conversationsService.ChangeConversation(conversation);
  }

  public async ChangeConversation(conversation: ConversationTemplate) {
    //if we were on search screen, we should hide it

    this.SearchString = '';

    await this.conversationsService.ChangeConversation(conversation);
  }

  //input events

  public async OnSendMessage(message: string) {
    await this.conversationsService.SendMessage(message, this.conversationsService.CurrentConversation);
  }

  public async OnUploadImages(files: FileList) {
    await this.conversationsService.UploadImages(files, this.conversationsService.CurrentConversation);
  }
}
