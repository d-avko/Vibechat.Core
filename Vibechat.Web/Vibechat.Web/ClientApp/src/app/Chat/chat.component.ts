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
import { MessagesComponent, ForwardMessagesModel } from "../Conversation/Messages/messages.component";
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
import { TokensService } from "../tokens/TokensService";
import { HttpResponseInterceptor } from "../Interceptors/HttpResponseInterceptor";
import { ForwardMessagesDialogComponent } from "../Dialogs/ForwardMessagesDialog";
import { MessagesDateParserService } from "../Services/MessagesDateParserService";
import { MessageState } from "../Shared/MessageState";
import { MessageAttachment } from "../Data/MessageAttachment";

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

  protected connectionManager: ConnectionManager;

  public IsAuthenticated: boolean;

  public SearchString: string;

  @ViewChild(MessagesComponent) messages: MessagesComponent;
  @ViewChild(MatDrawer) sideDrawer: MatDrawer;
  @ViewChild(SearchListComponent) searchList: SearchListComponent;

  constructor(
    public dialog: MatDialog,
    protected requestsBuilder: ApiRequestsBuilder,
    protected snackbar: SnackBarHelper,
    protected router: Router,
    public formatter: ConversationsFormatter,
    public tokensService: TokensService,
    public dateParser: MessagesDateParserService) {

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
      () => this.OnConnected(),
      (msgId, clientMessageId, conversationId) => this.OnMessageDelivered(msgId, clientMessageId, conversationId),
      (msgId, conversationId) => this.OnMessageRead(msgId, conversationId),
    );

    this.IsAuthenticated = Cache.IsAuthenticated;
    this.CurrentUser = Cache.UserCache;
  }
  async ngOnInit(): Promise<void> {
    if (this.IsAuthenticated) {

      HttpResponseInterceptor.IsRefreshingToken = true;

      let result = await this.tokensService.RefreshToken()

      if (!result.isSuccessfull) {
        this.router.navigateByUrl('/login');
        HttpResponseInterceptor.IsRefreshingToken = false;
        return;
      }

      Cache.token = result.response;
      localStorage.setItem('token', result.response);
      HttpResponseInterceptor.IsRefreshingToken = false;

      this.UpdateConversations();

      let response = await this.requestsBuilder.GetUserById(this.CurrentUser.id);

      if (!response.isSuccessfull) {
        return;
      }

      this.CurrentUser = response.response;
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

  public OnForwardMessages(values: Array<ChatMessage>) {
    let forwardMessagesDialog = this.dialog.open(
      ForwardMessagesDialogComponent,
      {
        data: {
          conversations: this.Conversations
        }
      }
    );

    forwardMessagesDialog
      .beforeClosed()
      .subscribe((result: Array<ConversationTemplate>) => {

        if (result == null || result.length == 0) {
          return;
        }

        result.forEach(
          (conversation) => {

            values.forEach(msg => {
              let messageToSend = this.BuildForwardedMessage(conversation.conversationID, msg.forwardedMessage ? msg.forwardedMessage : msg);

              this.connectionManager.SendMessage(messageToSend, conversation);

              conversation.messages.push(messageToSend);

            })
          }
        )

        values.splice(0, values.length);
      })
  }

  public OnViewGroupInfo(group: ConversationTemplate) {

    if (!group.isGroup) {
      this.OnViewUserInfo(group.dialogueUser);
      return;
    }

    this.requestsBuilder.GetConversationById(group.conversationID)
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

        groupInfoRef.componentInstance.OnUnBanUser
          .subscribe((user: UserInfo) => this.UnbanFromConversation(user));

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

  public UnbanFromConversation(user: UserInfo) {
    this.requestsBuilder.UnBanFromConversation(user.id, this.CurrentConversation.conversationID)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        user.isBlockedInConversation = false;
      });
  }

  public BanFromConversation(userToBan: UserInfo) {
    this.requestsBuilder.BanFromConversation(userToBan.id, this.CurrentConversation.conversationID)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        userToBan.isBlockedInConversation = true;
      });
  }

  public BanFromMessaging(userToBan: UserInfo) {

    let conversationToBan = this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == userToBan.id);

    this.requestsBuilder.BanUser(userToBan.id, conversationToBan == null ? 0 : conversationToBan.conversationID)
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

  public async OnViewUserInfo(user: UserInfo) {

    let result = await this.requestsBuilder.GetUserById(user.id)

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

        this.requestsBuilder.ChangeCurrentUserLastName(lastName)
          .subscribe((result) => {

            if (!result.isSuccessfull) {
              return;
            }

            this.CurrentUser.lastName = lastName;
          });

      });

    userInfoRef.componentInstance.OnChangeName
      .subscribe((name: string) => {

        this.requestsBuilder.ChangeCurrentUserName(name)
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

        this.requestsBuilder.UploadUserProfilePicture(file)
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
  }

  public ViewAttachmentsOf(user: UserInfo) {
    let conversation = this.Conversations.find(x => !x.isGroup && x.dialogueUser.id == user.id);

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

  public OnUnblockUser(user: UserInfo) {
    this.requestsBuilder.UnbanUser(user.id)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }

        user.isBlocked = false;
      });
  } 

  public OnRemoveAllMessages(group: ConversationTemplate) {
    this.requestsBuilder.DeleteMessages(group.messages, group.conversationID)
      .subscribe(
        (result) => {
          this.OnMessagesDeleted(result, this.CurrentConversation.messages, group.conversationID);
          this.CurrentConversation = null;
        }
      )
  }

  public Search() {
    if (this.SearchString == null || this.SearchString == '') {
      return;
    }

    this.searchList.Search();
  }

  public OnChangeGroupThumbnail(file: File): void{

    let currentConversationId = this.CurrentConversation.conversationID;

    this.requestsBuilder.UploadConversationThumbnail(file, this.CurrentConversation.conversationID)
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

    this.requestsBuilder.ChangeConversationName(name, this.CurrentConversation.conversationID)
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
        snackbar: this.snackbar
      }
    });

    dialogRef.beforeClosed().subscribe(users => {

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


    dialogRef.beforeClosed().subscribe(result => {

      if (result.name === '' || result.name == null) {
        return;
      }

      this.requestsBuilder.CreateConversation(result.name, this.CurrentUser.id, null, null, true, result.isPublic)
        .subscribe(
          (result) => {

            if (!result.isSuccessfull) {
              return;
            }

            this.connectionManager.InitiateConnections(new Array<number>(1).fill(result.response.conversationID));

            result.response.messages = new Array<ChatMessage>();

            this.Conversations = [...this.Conversations, result.response];
          }
        )
    });
  }

  public OnSendMessage(message: string) {

    let messageToSend = this.BuildMessage(message, this.CurrentConversation.conversationID);

    this.CurrentConversation.messages.push(
      messageToSend
    );

    this.connectionManager.SendMessage(messageToSend, this.CurrentConversation);
  }

  public BuildMessage(message: string, whereTo: number, isAttachment = false, AttachmentInfo: MessageAttachment = null): ChatMessage {
    var min = 1;
    var max = 100000;
    var clientMessageId = Math.floor(Math.random() * (+max - +min) + +min);

    return new ChatMessage(
      {
        id: clientMessageId,
        messageContent: message,
        isAttachment: isAttachment,
        attachmentInfo: AttachmentInfo,
        user: this.CurrentUser,
        conversationID: whereTo,
        state: MessageState.Pending,
        timeReceived: new Date()
      });
  }

  public BuildForwardedMessage(whereTo: number, forwarded: ChatMessage): ChatMessage {
    var min = 1;
    var max = 100000;
    var clientMessageId = Math.floor(Math.random() * (+max - +min) + +min);

    return new ChatMessage(
      {
        id: clientMessageId,
        isAttachment: false,
        user: this.CurrentUser,
        conversationID: whereTo,
        state: MessageState.Pending,
        timeReceived: new Date(),
        forwardedMessage: forwarded
      });
  }

  public OnMessageReceived(data: MessageReceivedModel): void {
    this.dateParser.ParseStringDateInMessage(data.message);

    let conversation = this.Conversations
      .find(x => x.conversationID == data.conversationId);

    let newLength = conversation.messages.push(data.message);

    conversation.messages = [...conversation.messages];

    if (this.messages == undefined) {
      return;
    }

    if (data.conversationId == this.CurrentConversation.conversationID) {
      this.messages.ScrollToMessage(newLength);
    }
  }

  public ReadMessage(message: ChatMessage) {

    if (message.user.id == this.CurrentUser.id) {
      return;
    }

    this.connectionManager.ReadMessage(message.id, message.conversationID);
  }

  public OnMessageRead(msgId: number, conversationId: number) {
    let conversation = this.Conversations.find(x => x.conversationID == conversationId);

    if (conversation == null) {
      return;
    }

    let message = conversation.messages.find(x => x.id == msgId);

    if (message == null) {
      return;
    }

    message.state = MessageState.Read;
    conversation.messages = [...conversation.messages];
  }

  public OnMessageDelivered(msgId: number, clientMessageId: number, conversationId: number) {
    let conversation = this.Conversations.find(x => x.conversationID == conversationId);

    if (conversation == null) {
      return;
    }

    let message = conversation.messages.find(x => x.id == clientMessageId);

    if (message == null) {
      return;
    }

    message.id = msgId;
    message.state = MessageState.Delivered;
    conversation.messages = [...conversation.messages];
  }

  public OnAddedToGroup(data: AddedToGroupModel): void {

    if (data.conversation.messages != null) {
      this.dateParser.ParseStringDatesInMessages(data.conversation.messages);
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

    this.requestsBuilder.GetConversationById(data.conversation.conversationID)
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

  public OnDeleteMessages(SelectedMessages: Array<ChatMessage>) {

    let currentConversationId = this.CurrentConversation.conversationID;

    let notLocalMessages = SelectedMessages.filter(x => x.state != MessageState.Pending)

    //delete local unsent messages
    this.CurrentConversation.messages = this.CurrentConversation.messages
      .filter(msg => notLocalMessages.findIndex(selected => selected.id == msg.id) == -1);

    if (notLocalMessages.length == 0) {
      return;
    }

    this.requestsBuilder.DeleteMessages(notLocalMessages, this.CurrentConversation.conversationID)
      .subscribe(
        (result) => this.OnMessagesDeleted(result, SelectedMessages, currentConversationId)
      );   
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
    this.requestsBuilder.UpdateConversationsRequest()
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
                  this.dateParser.ParseStringDatesInMessages(conversation.messages);
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
        }
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

    this.requestsBuilder.GetConversationById(conversation.conversationID)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          return;
        }
        //update existing data. Couldn't assign fetched object,
        //because this will lead to lost pointer for messages, and thus they won't be updated on UI
        this.UpdateConversationFields(conversation, result.response);
      });
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

    this.requestsBuilder.UploadImages(files)
      .subscribe((result) => {

        let response = (<HttpResponse<ServerResponse<UploadFilesResponse>>>result).body;

        if (!response.isSuccessfull) {
          return;
        }

        response.response.uploadedFiles.forEach(
          (file) => {
            let message = this.BuildMessage(null, conversationToSend, true, file);

            this.connectionManager.SendMessage(message, this.CurrentConversation);

            this.CurrentConversation.messages.push(message);
          } 
        )
      })
  }
}
