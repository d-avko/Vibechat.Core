import { Component, ViewChild, OnInit } from "@angular/core";
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
import { resetCompiledComponents } from "@angular/core/src/render3/jit/module";

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

  @ViewChild(MessagesComponent) messages: MessagesComponent;
  @ViewChild(MatDrawer) sideDrawer: MatDrawer;

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
      (error) => this.OnSignalrError(error),
      (data) => this.OnRemovedFromGroup(data)
    );

    this.IsAuthenticated = Cache.IsAuthenticated;
    this.CurrentUser = Cache.UserCache;

  }
  ngOnInit(): void {
    if (this.IsAuthenticated) {
      this.UpdateConversations();
    }
  }

  public OnLogOut() : void {
    Cache.LogOut();
    this.router.navigateByUrl('/login');
  }

  public OnSignalrError(error: string) : void{
    this.snackbar.openSnackBar(error, 2);
  }

  public OnViewGroupInfo(group: ConversationTemplate) {
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
    .subscribe(() => this.OnInviteUsersToGroup());

    groupInfoRef.componentInstance
    .OnChangeName
    .subscribe((name: string) => this.OnChangeConversationName(name));

    groupInfoRef.componentInstance
    .OnLeaveGroup
    .subscribe(() => {
      this.OnLeaveGroup();
      groupInfoRef.close();
    });

    groupInfoRef.componentInstance
    .OnChangeThumbnail
    .subscribe((file: File) => this.OnChangeGroupThumbnail(file));

    groupInfoRef.componentInstance
    .OnClearMessages
    .subscribe(() => {
      this.OnRemoveAllMessages();
      groupInfoRef.close();
    });

    groupInfoRef.componentInstance
    .OnViewUserInfo
    .subscribe((user: UserInfo) => this.OnViewUserInfo(user));

    groupInfoRef.componentInstance
    .OnJoinGroup
    .subscribe((group: ConversationTemplate) => {
      this.OnJoinGroup(group);
      groupInfoRef.close();
    });

    groupInfoRef.componentInstance.OnKickUser
    .subscribe((user: UserInfo) => this.OnKickUser(user));
   
  }

  public OnKickUser(user: UserInfo){

  }

  public OnJoinGroup(conversation: ConversationTemplate){
    this.connectionManager.AddUserToConversation(this.CurrentUser.id, this.CurrentUser.id, conversation);
  }

  public OnViewUserInfo(user: UserInfo){

  }


  public OnRemoveAllMessages() {
    let currentConversationId = this.CurrentConversation.conversationID;

    this.requestsBuilder.DeleteMessages(this.CurrentConversation.messages, this.CurrentConversation.conversationID, Cache.JwtToken)
      .subscribe(
        (result) => {
          this.OnMessagesDeleted(result, this.CurrentConversation.messages, currentConversationId);
          this.CurrentConversation = null;
        } 
        ,
        (error) => {
          if (error.error instanceof ErrorEvent) {
            this.snackbar.openSnackBar('Network error occurred. Try refreshing the page.', 2);
          } else {
            //unauthorized
            if (error.status == 401) {
              this.OnTokenExpired();
            }
          }
        }
      )
  }

  public OnChangeGroupThumbnail(file: File): void{

    let currentConversationId = this.CurrentConversation.conversationID;

    this.requestsBuilder.UploadConversationThumbnail(file, this.CurrentConversation.conversationID, Cache.JwtToken)
      .subscribe((result) => {

        if (!result.isSuccessfull) {
          this.snackbar.openSnackBar(result.errorMessage);
          return;
        }

        let conversation = this.Conversations.find(x => x.conversationID == currentConversationId);

        conversation.thumbnailUrl = result.response.thumbnailUrl;
        conversation.fullImageUrl = result.response.fullImageUrl;
      });
  }

  public OnLeaveGroup(){
    this.connectionManager.RemoveUserFromConversation(this.CurrentUser.id, this.CurrentConversation.conversationID);
    this.CurrentConversation = null;
  }

  public OnChangeConversationName(name: string) {
    let currentConversationId = this.CurrentConversation.conversationID;

    this.requestsBuilder.ChangeConversationName(name, this.CurrentConversation.conversationID, Cache.JwtToken)
      .subscribe(
        (result) => {

          if (!result.isSuccessfull) {
            this.snackbar.openSnackBar(result.errorMessage);
            return;
          }

          this.Conversations.find(x => x.conversationID == currentConversationId).name = name;

        }
    )
  }

  public OnInviteUsersToGroup() {

    const dialogRef = this.dialog.open(FindUsersDialogComponent, {
      width: '350px',
      data: {
        conversationId: this.CurrentConversation.conversationID,
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
          this.connectionManager.AddUserToConversation(value.id, this.CurrentUser.id, this.CurrentConversation);
        }
      )

      //Now add users locally

      users.forEach(
        (user) => {

          //sort of sanitization of input

          if (user.id == this.CurrentUser.id) {
            return;
          }

          this.CurrentConversation.participants.push(user);
          this.CurrentConversation.participants = [...this.CurrentConversation.participants];

        }
      )
    });
  }

  public CreateGroup() {

    this.sideDrawer.close();

    const dialogRef = this.dialog.open(AddGroupDialogComponent, {
      width: '250px',
      data: { name: '' }
    });

    dialogRef.afterClosed().subscribe(result => {

      if (result === '' || result == null) {
        return;
      }

      this.requestsBuilder.CreateConversation(result, this.CurrentUser.id, null, null, true, Cache.JwtToken)
        .subscribe(
          (result) => {

            if (!result.isSuccessfull) {
              this.snackbar.openSnackBar(result.errorMessage);
              return;
            }

            this.connectionManager.InitiateConnections(new Array<number>(1).fill(result.response.conversationID));

            result.response.messages = new Array<ChatMessage>();

            this.Conversations = [...this.Conversations, result.response];
          },
          (error) => {
            if (error.error instanceof ErrorEvent) {
              this.snackbar.openSnackBar('Network error occurred. Try refreshing the page.', 2);
            } else {
              //unauthorized
              if (error.status == 401) {
                this.OnTokenExpired();
              }
            }
          }
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

  public OnConnecting(data: EmptyModel) {
    this.snackbar.openSnackBar("Connecting...");
  }

  public OnMessageReceived(data: MessageReceivedModel): void {
    data.message.timeReceived = new Date(<string>data.message.timeReceived);

    let conversation = this.Conversations
      .find(x => x.conversationID == data.conversationId);

    let newLength = conversation.messages.push(data.message);

    conversation.messages = [...conversation.messages];

    this.messages.ScrollToMessage(newLength);
  }

  public OnAddedToGroup(data: AddedToGroupModel): void {

    //someone invited myself.

    if (data.user.id == this.CurrentUser.id) {

      if (data.conversation.messages == null) {
        data.conversation.messages = new Array<ChatMessage>();
      }

      this.Conversations = [...this.Conversations, data.conversation];
    } else {

      let conversation = this.Conversations.find(x => x.conversationID == data.conversation.conversationID);
      conversation.participants.push(data.user);
      conversation.participants = [...conversation.participants];
    }
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

  public OnDisconnected(data: EmptyModel) {
    this.snackbar.openSnackBar("Disconnected. Retrying...");
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

          if (!result) {
            this.snackbar.openSnackBar("Failed to update messages for " + this.CurrentConversation.name);
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

          result.response.messages.forEach((x) => x.timeReceived = new Date(<string>x.timeReceived));

          //append old messages to new ones.
          this.CurrentConversation.messages = result.response.messages.concat(this.CurrentConversation.messages);
          this.IsMessagesLoading = false;

          this.messages.ScrollToMessage(result.response.messages.length);
        },
          (error) => {
            if (error.error instanceof ErrorEvent) {
              this.snackbar.openSnackBar('Network error occurred. Try refreshing the page.', 2);
            } else {
              //unauthorized
              if (error.status == 401) {
                this.OnTokenExpired();
              }
            }
          }
      )
    }

  }

  public OnDeleteMessages(SelectedMessages: Array<ChatMessage>) {

    let currentConversationId = this.CurrentConversation.conversationID;

    this.requestsBuilder.DeleteMessages(SelectedMessages, this.CurrentConversation.conversationID, Cache.JwtToken)
      .subscribe(
        (result) => this.OnMessagesDeleted(result, SelectedMessages, currentConversationId)
        ,
        (error) => {
          if (error.error instanceof ErrorEvent) {
            this.snackbar.openSnackBar('Network error occurred. Try refreshing the page.', 2);
          } else {
            //unauthorized
            if (error.status == 401) {
              this.OnTokenExpired();
            }
          }
        }
      )
  }

  public OnMessagesDeleted(response: ServerResponse<string>, SelectedMessages: Array<ChatMessage>, conversationId: number) {
    if (!response.isSuccessfull) {
      this.snackbar.openSnackBar("Failed to delete messages. Reason: " + response.errorMessage);
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
    this.requestsBuilder.UpdateConversationsRequest(Cache.JwtToken, this.CurrentUser.id)
      .subscribe(
        (response) => {

          if (!response.isSuccessfull) {
            this.snackbar.openSnackBar("Failed to update conversations. Reason: " + response.errorMessage);
            return;
          }

          if (response.response.conversations != null) {

            //parse string date to js Date

            response.response.conversations
              .forEach((conversation) => {

                if (conversation.messages != null) {
                  conversation.messages.forEach(msg => msg.timeReceived = new Date(<string>msg.timeReceived))
                } else {
                  conversation.messages = new Array<ChatMessage>();
                }

              })

            this.Conversations = response.response.conversations;

          } else {
            this.Conversations = new Array<ConversationTemplate>();
          }

          //Initiate signalR group connections

          this.connectionManager.Start();
        },
        (error) => {
          if (error.error instanceof ErrorEvent) {
            this.snackbar.openSnackBar('Network error occurred. Try refreshing the page.', 2);
          } else {
            //unauthorized
            if (error.status == 401) {
              this.OnTokenExpired();
            }
          }
        }
      )
  }

  public IsConversationSelected(): boolean {
    return this.CurrentConversation != null;
  }


  public ChangeConversation(conversation: ConversationTemplate): void {
    if (conversation == this.CurrentConversation) {
      this.CurrentConversation = null;
      return;
    }

    this.CurrentConversation = conversation;

    this.IsConversationHistoryEnd = false;
  }

  public OnUploadImages(files: FileList) {
    let conversationToSend = this.CurrentConversation.conversationID;

    for (let i = 0; i < files.length; ++i) {
      if (((files[i].size / 1024) / 1024) > ApiRequestsBuilder.maxUploadImageSizeMb) {
        this.snackbar.openSnackBar("Some of the files was larger than " + ApiRequestsBuilder.maxUploadImageSizeMb + "MB");
        return;
      }
    }

    this.requestsBuilder.UploadImages(files, Cache.JwtToken).subscribe(
      (result) => {

      if (!result.isSuccessfull) {
        this.snackbar.openSnackBar(result.errorMessage);
        return;
      }

      result.response.uploadedFiles.forEach(
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
      },
      (error) => {
        if (error.error instanceof ErrorEvent) {
          this.snackbar.openSnackBar('Network error occurred. Try refreshing the page.', 2);
        } else {
          //unauthorized
          if (error.status == 401) {
            this.OnTokenExpired();
          }
        }
      }
    )
  }

  private OnTokenExpired() {
    this.requestsBuilder.RefreshJwtToken(Cache.JwtToken, Cache.UserCache.id)
      .subscribe(
        (result) => {

          if (!result.isSuccessfull) {
            this.snackbar.openSnackBar(result.errorMessage, 2);
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
