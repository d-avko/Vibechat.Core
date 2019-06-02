import { Component, ViewChild, OnInit, Type } from "@angular/core";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { Cache } from "../Auth/Cache";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { MatSnackBar, MatDialog, MatDrawer } from "@angular/material";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { Router } from "@angular/router";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { ChatMessage } from "../Data/ChatMessage";
import { trigger, state, style, transition, animate } from "@angular/animations";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { ConnectionManager } from "../Connections/ConnectionManager";
import { MessageReceivedModel } from "../Shared/MessageReceivedModel";
import { AddedToGroupModel } from "../Shared/AddedToGroupModel";
import { RemovedFromGroupModel } from "../Shared/RemovedFromGroupModel";
import { EmptyModel } from "../Shared/EmptyModel";
import { MessagesComponent } from "../Conversation/Messages/messages.component";
import { UserInfo } from "../Data/UserInfo";
import { FindUsersDialogComponent } from "../Dialogs/FindUsersDialog";
import { AddGroupDialogComponent } from "../Dialogs/AddGroupDialog";
import { GroupInfoDialogComponent } from "../Dialogs/GroupInfoDialog";
import { SearchListComponent } from "../Search/searchlist.component";
import { UserInfoDialogComponent } from "../Dialogs/UserInfoDialog";
import { identity } from "rxjs";
import { resetCompiledComponents } from "@angular/core/src/render3/jit/module";
import { HttpResponse } from "@angular/common/http";
import { UploadFilesResponse } from "../Data/UploadFilesResponse";
import { retry } from "rxjs/operators";
import { AnimationGroupPlayer } from "@angular/animations/src/players/animation_group_player";
import { ViewAttachmentsDialogComponent } from "../Dialogs/ViewAttachmentsDialog";

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

  //This user conversations

  public Conversations: Array<ConversationTemplate>;

  public CurrentConversation: ConversationTemplate;

  public CurrentUser: UserInfo;

  //pop-up that will inform user of errors.

  protected snackbar: SnackBarHelper;

  protected router: Router;

  protected connectionManager: ConnectionManager;



  protected requestsBuilder: ApiRequestsBuilder

  public formatter: ConversationsFormatter;

  public static MessagesBufferLength: number = 50;



  public IsMessagesLoading: boolean = false;

  public IsAuthenticated: boolean;

  public IsConversationHistoryEnd: boolean = false;

  public SearchString: string;

  @ViewChild(MessagesComponent) messages: MessagesComponent;
  @ViewChild(MatDrawer) sideDrawer: MatDrawer;
  @ViewChild(SearchListComponent) searchList: SearchListComponent;

  constructor(
    public dialog: MatDialog,
    requestsBuilder: ApiRequestsBuilder,
    snackbar: MatSnackBar,
    router: Router,
    formatter: ConversationsFormatter) {

    this.snackbar = new SnackBarHelper(snackbar);

    this.router = router;
    
    this.requestsBuilder = requestsBuilder;

    this.Conversations = new Array<ConversationTemplate>();

    this.formatter = formatter;
  
    if (!Cache.IsAuthenticated) {
      if (!Cache.TryAuthenticate()) {
        router.navigateByUrl('/login');
        return;
      }
    }

    this.connectionManager = new ConnectionManager(
      () => this.Conversations.map((x) => x.conversationID),
      (data) => this.OnMessageReceived(data),
      (data) => this.OnAddedToGroup(data),
      (error) => this.OnError(error),
      (data) => this.OnRemovedFromGroup(data),
      () => this.OnDisconnected(),
      () => this.OnConnecting(),
      () => this.OnConnected()
    );

    this.IsAuthenticated = Cache.IsAuthenticated;
    this.CurrentUser = Cache.UserCache;
  }
  ngOnInit(): void {
    if (this.IsAuthenticated) {
      this.UpdateConversations();

      this.requestsBuilder.GetUserById(Cache.JwtToken,this.CurrentUser.id)
        .subscribe((result) => {

          if (!result.isSuccessfull) {
            return;
          }

          this.CurrentUser = result.response;
        })
    }
  }

  public OnConnected(): void {
    this.snackbar.openSnackBar("Connected.", 1);
  }

  public OnDisconnected(): void {
    this.snackbar.openSnackBar("Disconnected...", 1.5);
  }

  public OnConnecting() {
    this.snackbar.openSnackBar("Connecting...", 1.5);
  }

  public OnLogOut() : void {
    Cache.LogOut();
    this.router.navigateByUrl('/login');
  }

  public OnError(error: string) : void{
    this.snackbar.openSnackBar(error, 2);
  }

  public OnViewGroupInfo(group: ConversationTemplate) {

    if (!group.isGroup) {
      this.OnViewUserInfo(group.dialogueUser);
      return;
    }

    this.requestsBuilder.GetConversationById(Cache.JwtToken, group.conversationID)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        this.UpdateConversationFields(group, result.response);

        const groupInfoRef = this.dialog.open(GroupInfoDialogComponent, {
          width: '450px',
          data: {
            Conversation: group,
            user: this.CurrentUser,
            ExistsInThisGroup: this.Conversations.find(x => x.conversationID == group.conversationID) != null
          }
        });

        groupInfoRef.componentInstance
          .OnInviteUsers
          .subscribe((group: ConversationTemplate) => this.OnInviteUsersToGroup(group));

        groupInfoRef.componentInstance
          .OnChangeName
          .subscribe((name: string) => this.OnChangeConversationName(name));

        groupInfoRef.componentInstance
          .OnLeaveGroup
          .subscribe((group: ConversationTemplate) => {
            this.OnLeaveGroup(group);
            groupInfoRef.close();
          });

        groupInfoRef.componentInstance
          .OnChangeThumbnail
          .subscribe((file: File) => this.OnChangeGroupThumbnail(file));

        groupInfoRef.componentInstance
          .OnClearMessages
          .subscribe(((group: ConversationTemplate) => {
            this.OnRemoveAllMessages(group);
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
          .subscribe((user: UserInfo) => this.BanFromConversation(user));

        groupInfoRef.componentInstance.OnRemoveGroup
          .subscribe((group: ConversationTemplate) => {
            this.connectionManager.RemoveConversation(group);
          });

        groupInfoRef.componentInstance.OnViewAttachments
          .subscribe((group: ConversationTemplate) => {
            this.ViewAttachments(group);
          });
      })

  
  }

  public OnKickUser(user: UserInfo) {
    if (user.id == this.CurrentUser.id) {
      this.snackbar.openSnackBar("Couldn't kick yourself.");
      return;
    }

    this.connectionManager.RemoveUserFromConversation(user.id, this.CurrentConversation.conversationID, false);
  }

  public BanFromConversation(userToBan: UserInfo) {
    this.requestsBuilder.BanFromConversation(Cache.JwtToken, userToBan.id, this.CurrentConversation.conversationID)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        //should mark some flag there that user was banned successfully.
      });
  }

  public BanFromMessaging(userToBan: UserInfo) {

    let conversationToBan = this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == userToBan.id);

    this.requestsBuilder.BanUser(Cache.JwtToken, userToBan.id, conversationToBan == null ? 0 : conversationToBan.conversationID)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        userToBan.isBlocked = true;
      });
  }

  public OnJoinGroup(conversation: ConversationTemplate) {
    this.SearchString = '';
    this.connectionManager.AddUserToConversation(this.CurrentUser.id, conversation);
  }

  public OnViewUserInfo(user: UserInfo) {

    this.requestsBuilder.GetUserById(Cache.JwtToken, user.id)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        user = result.response;

        if (user.id == this.CurrentUser.id) {
          this.CurrentUser = result.response;
        }

        const userInfoRef = this.dialog.open(UserInfoDialogComponent, {
          width: '450px',
          data: {
            user: user,
            currentUser: this.CurrentUser,
            Conversations: this.Conversations
          }
        });

        userInfoRef.componentInstance.OnBlockUser
          .subscribe((user: UserInfo) => {
            this.BanFromMessaging(user);
          });

        userInfoRef.componentInstance.OnUnblockUser
          .subscribe((user: UserInfo) => {
            this.OnUnblockUser(user);
          });

        userInfoRef.componentInstance.OnChangeLastname
          .subscribe((lastName: string) => {

            this.requestsBuilder.ChangeCurrentUserLastName(Cache.JwtToken, lastName)
              .subscribe((result) => {

                if (!result.isSuccessfull) {
                  return;
                }

                this.CurrentUser.lastName = lastName;
              });

          });

        userInfoRef.componentInstance.OnChangeName
          .subscribe((name: string) => {

            this.requestsBuilder.ChangeCurrentUserName(Cache.JwtToken, name)
              .subscribe((result) => {

                if (!result.isSuccessfull) {
                  return;
                }

                this.CurrentUser.name = name;
              });
          });

        userInfoRef.componentInstance.OnCreateDialogWith
          .subscribe((user: UserInfo) => {
            this.connectionManager.CreateDialog(user);
          });

        userInfoRef.componentInstance.OnRemoveDialogWith
          .subscribe((user: UserInfo) => {
            this.connectionManager.RemoveConversation(this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == user.id));
            this.CurrentConversation = null;
            userInfoRef.close();
          });

        userInfoRef.componentInstance.OnUpdateProfilePicture
          .subscribe((file: File) => {

            this.requestsBuilder.UploadUserProfilePicture(file, Cache.JwtToken)
              .subscribe((result) => {

                if (!result.isSuccessfull) {
                  return;
                }

                this.CurrentUser.imageUrl = result.response.thumbnailUrl;
                this.CurrentUser.fullImageUrl = result.response.fullImageUrl;
              });

          });

        userInfoRef.componentInstance.OnViewAttachmentsOf
          .subscribe((user: UserInfo) => {
            this.ViewAttachmentsOf(user)
          });

      })
  }

  public ViewAttachmentsOf(user: UserInfo) {
    let conversation = this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == user.id);

    if (conversation) {
      const attachmentsDialogRef = this.dialog.open(ViewAttachmentsDialogComponent, {
        width: '450px',
        data: {
          conversation: conversation,
          token: Cache.JwtToken
        }
      });
    }

  }

  public ViewAttachments(conversation: ConversationTemplate) {
    if (conversation) {
      const attachmentsDialogRef = this.dialog.open(ViewAttachmentsDialogComponent, {
        width: '450px',
        data: {
          conversation: conversation,
          token: Cache.JwtToken
        }
      });
    }
  }

  public OnUnblockUser(user: UserInfo) {
    this.requestsBuilder.UnbanUser(Cache.JwtToken, user.id)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        user.isBlocked = false;
      });
  } 

  public OnRemoveAllMessages(group: ConversationTemplate) {
    this.requestsBuilder.DeleteMessages(group.messages, group.conversationID, Cache.JwtToken)
      .subscribe(
        (result) => {
          this.OnMessagesDeleted(result, this.CurrentConversation.messages, group.conversationID);
          this.CurrentConversation = null;
        },
        (error) => this.OnApiError(error)
      )
  }

  public OnApiError(error: any) {
    if (error.error instanceof ErrorEvent) {
      this.snackbar.openSnackBar('Network error occurred. Try refreshing the page.', 2);
    } else {
      //unauthorized
      if (error.status == 401) {
        this.OnTokenExpired();
      }
    }
  }

  public Search() {
    if (this.SearchString == null || this.SearchString == '') {
      return;
    }

    this.searchList.Search();
  }

  public OnChangeGroupThumbnail(file: File): void{

    let currentConversationId = this.CurrentConversation.conversationID;

    this.requestsBuilder.UploadConversationThumbnail(file, this.CurrentConversation.conversationID, Cache.JwtToken)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        let conversation = this.Conversations.find(x => x.conversationID == currentConversationId);

        conversation.thumbnailUrl = result.response.thumbnailUrl;
        conversation.fullImageUrl = result.response.fullImageUrl;
      });
  }

  public OnLeaveGroup(conversation: ConversationTemplate){
    this.connectionManager.RemoveUserFromConversation(this.CurrentUser.id, conversation.conversationID, true);
    this.CurrentConversation = null;
  }

  public OnChangeConversationName(name: string) {
    let currentConversationId = this.CurrentConversation.conversationID;

    this.requestsBuilder.ChangeConversationName(name, this.CurrentConversation.conversationID, Cache.JwtToken)
      .subscribe(
        (result) => {

          if (!result.isSuccessfull) {
            return;
          }

          this.Conversations.find(x => x.conversationID == currentConversationId).name = name;

        }
    )
  }

  public OnInviteUsersToGroup(group: ConversationTemplate) {

    const dialogRef = this.dialog.open(FindUsersDialogComponent, {
      width: '350px',
      data: {
        conversationId: group.conversationID,
        requestsBuilder: this.requestsBuilder,
        snackbar: this.snackbar,
        token: Cache.JwtToken
      }
    });

    dialogRef.afterClosed().subscribe(users => {

      if (users === '' || users == null) {
        return;
      }

      if (users == null) {
        return;
      }

      users.forEach(
        (value) => {
          this.connectionManager.AddUserToConversation(value.id, group);
        }
      )

      //Now add users locally

      users.forEach(
        (user) => {

          //sort of sanitization of input

          if (user.id == this.CurrentUser.id) {
            return;
          }

          group.participants.push(user);
          group.participants = [...group.participants];

        }
      )
    });
  }

  public CreateGroup() {

    this.sideDrawer.close();

    const dialogRef = this.dialog.open(AddGroupDialogComponent, {
      width: '250px'
    });

    
    dialogRef.afterClosed().subscribe(result => {

      if (result.name === '' || result.name == null) {
        return;
      }

      this.requestsBuilder.CreateConversation(result.name, this.CurrentUser.id, null, null, true, Cache.JwtToken, result.isPublic)
        .subscribe(
          (result) => {

            if (!result.isSuccessfull) {
              return;
            }

            this.connectionManager.InitiateConnections(new Array<number>(1).fill(result.response.conversationID));

            result.response.messages = new Array<ChatMessage>();

            this.Conversations = [...this.Conversations, result.response];
          },
          (error) => this.OnApiError(error)
        )
    });
  }

  public OnSendMessage(message: string) {

    this.connectionManager.SendMessage(
      new ChatMessage(
        {
          messageContent: message,
          isAttachment: false,
          user: this.CurrentUser,
          conversationID: this.CurrentConversation.conversationID
        }), this.CurrentConversation);
  }

  public OnMessageReceived(data: MessageReceivedModel): void {
    data.message.timeReceived = new Date(data.message.timeReceived);

    let conversation = this.Conversations
      .find(x => x.conversationID == data.conversationId);

    let newLength = conversation.messages.push(data.message);

    conversation.messages = [...conversation.messages];

    if (this.messages == undefined) {
      return;
    }

    this.messages.ScrollToMessage(newLength);
  }

  public OnAddedToGroup(data: AddedToGroupModel): void {
    //c# date -> ts date
    if (data.conversation.messages != null) {
      data.conversation.messages.forEach((x) => x.timeReceived = new Date(x.timeReceived));
    }

    if (data.conversation.isGroup) {

      //we created new group.

      if (data.user.id == this.CurrentUser.id) {

        if (data.conversation.messages == null) {
          data.conversation.messages = new Array<ChatMessage>();
        }

        this.Conversations = [...this.Conversations, data.conversation];
      } else {

        //someone added new user to existing group

        let conversation = this.Conversations.find(x => x.conversationID == data.conversation.conversationID);
        conversation.participants.push(data.user);
        conversation.participants = [...conversation.participants];
      }
    } else {

      //we created dialog with someone;

      if (data.user.id == this.CurrentUser.id) {

        if (data.conversation.messages == null) {
          data.conversation.messages = new Array<ChatMessage>();
        }

        this.Conversations = [...this.Conversations, data.conversation];

      } else {

        //someone created dialog with us.

        data.conversation.dialogueUser = data.user;
        this.Conversations = [...this.Conversations, data.conversation];
      }

    }

    //update data about this conversation.

    this.requestsBuilder.GetConversationById(Cache.JwtToken, data.conversation.conversationID)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        this.UpdateConversationFields(data.conversation, result.response);
      })
  }

  public UpdateConversationFields(old: ConversationTemplate, New: ConversationTemplate): void {
    old.isMessagingRestricted = New.isMessagingRestricted;
    old.name = New.name;
    old.fullImageUrl = New.fullImageUrl;
    old.thumbnailUrl = New.thumbnailUrl;
  }

  public OnRemovedFromGroup(data: RemovedFromGroupModel) {
    //either this client left or creator removed him.

    if (data.userId == this.CurrentUser.id) {

      this.Conversations.splice(this.Conversations.findIndex(x => x.conversationID == data.conversationId), 1);
    } else {

      let participants = this.Conversations.find(x => x.conversationID == data.conversationId).participants;

      participants.splice(participants.findIndex(x => x.id == data.userId), 1);
    }
  }

  public OnUpdateMessages() {

    if (!this.IsMessagesLoading && !this.IsConversationHistoryEnd) {
      this.IsMessagesLoading = true;

      if (this.CurrentConversation.messages == null) {
        return;
      }

      this.requestsBuilder.GetConversationMessages(
        this.CurrentConversation.messages.length,
        ChatComponent.MessagesBufferLength,
        this.CurrentConversation.conversationID, Cache.JwtToken)

        .subscribe((result) => {

          if (!result.isSuccessfull) {
            this.IsMessagesLoading = false;
            return;
          }

          //server sent zero messages, we reached end of our history.

          if (result.response.messages == null || result.response.messages.length == 0) {
            this.IsMessagesLoading = false;
            this.IsConversationHistoryEnd = true;

            return;
          }

          result.response.messages = result.response.messages.sort(this.MessagesSortFunc);

          result.response.messages.forEach((x) => x.timeReceived = new Date(x.timeReceived));

          //append old messages to new ones.
          this.CurrentConversation.messages = [...result.response.messages.concat(this.CurrentConversation.messages)];
          this.IsMessagesLoading = false;

          this.messages.ScrollToMessage(result.response.messages.length);
        },
          (error) => this.OnApiError(error)
      )
    }

  }

  public OnDeleteMessages(SelectedMessages: Array<ChatMessage>) {

    let currentConversationId = this.CurrentConversation.conversationID;

    this.requestsBuilder.DeleteMessages(SelectedMessages, this.CurrentConversation.conversationID, Cache.JwtToken)
      .subscribe(
        (result) => this.OnMessagesDeleted(result, SelectedMessages, currentConversationId)
        ,
        (error) => this.OnApiError(error)
      )
  }

  public OnMessagesDeleted(response: ServerResponse<string>, SelectedMessages: Array<ChatMessage>, conversationId: number) {
    if (!response.isSuccessfull) {
      return;
    }

    //delete messages locally

    let conversation = this.Conversations.find(x => x.conversationID == conversationId);

    conversation.messages = conversation
      .messages
      .filter(msg => SelectedMessages.findIndex(selected => selected.id == msg.id) == -1);

    this.messages.ScrollToStart();

    SelectedMessages.splice(0, SelectedMessages.length);
  }

  public UpdateConversations() {
    this.requestsBuilder.UpdateConversationsRequest(Cache.JwtToken)
      .subscribe(
        (response) => {

          if (!response.isSuccessfull) {
            return;
          }

          if (response.response != null) {

            //parse string date to js Date

            response.response
              .forEach((conversation) => {

                if (conversation.messages != null) {
                  conversation.messages.forEach(msg => msg.timeReceived = new Date(msg.timeReceived))
                } else {
                  conversation.messages = new Array<ChatMessage>();
                }

              })

              
            this.Conversations = response.response;

            this.Conversations.forEach((x) => {

              if (!x.isGroup) {
                x.isMessagingRestricted = x.dialogueUser.isMessagingRestricted;
              }
            });

          } else {
            this.Conversations = new Array<ConversationTemplate>();
          }

          //Initiate signalR group connections

          this.connectionManager.Start();
        },
        (error) => this.OnApiError(error)
      )
  }

  public IsConversationSelected(): boolean {
    return this.CurrentConversation != null;
  }

  public ChangeConversation(conversation: ConversationTemplate): void {
    //if we were on search screen, we should hide it

    this.SearchString = '';

    if (conversation == this.CurrentConversation) {
      this.CurrentConversation = null;
      return;
    }

    this.CurrentConversation = conversation;

    this.requestsBuilder.GetConversationById(Cache.JwtToken,conversation.conversationID)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }
        //update existing data. Couldn't assign fetched object,
        //because this will lead to lost pointer for messages, and thus they won't be updated on UI
        this.UpdateConversationFields(this.CurrentConversation, result.response);
      });

    this.IsConversationHistoryEnd = false;
  }

  public OnUploadImages(files: FileList) {
    let conversationToSend = this.CurrentConversation.conversationID;

    for (let i = 0; i < files.length; ++i) {
      if (((files[i].size / 1024) / 1024) > ApiRequestsBuilder.maxUploadImageSizeMb) {
        this.snackbar.openSnackBar("Some of the files were larger than " + ApiRequestsBuilder.maxUploadImageSizeMb + "MB");
        return;
      }
    }

    if (files.length == 0) {
      return;
    }

    this.requestsBuilder.UploadImages(files, Cache.JwtToken)
      .subscribe((result) => {

        let response = (<HttpResponse<ServerResponse<UploadFilesResponse>>>result).body;

        if (!response.isSuccessfull) {
          return;
        }

        response.response.uploadedFiles.forEach(
          (file) => this.connectionManager.SendMessage(
            new ChatMessage(
              {
                messageContent: null,
                isAttachment: true,
                user: this.CurrentUser,
                conversationID: conversationToSend,
                attachmentInfo: file
              }), this.CurrentConversation)
        )
      })
  }

  private OnTokenExpired() {
    this.requestsBuilder.RefreshJwtToken(Cache.JwtToken, Cache.UserCache.id)
      .subscribe(
        (result) => {

          if (!result.isSuccessfull) {
            return;
          }

          Cache.JwtToken = result.response;
          this.snackbar.openSnackBar('Your token expired and was updated. Please try that again.', 2);
        },
        (error) => {
          this.router.navigateByUrl('/login');
          return;
        }
      );
  }

  private MessagesSortFunc(left: ChatMessage, right: ChatMessage): number {
    if (left.timeReceived < right.timeReceived) return -1;
    if (left.timeReceived > right.timeReceived) return 1;
    return 0;
  }

}
