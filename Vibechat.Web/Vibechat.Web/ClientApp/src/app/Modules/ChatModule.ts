import { NgModule } from "@angular/core";
import { ChatComponent } from "../Chat/chat.component";
import { InputComponent } from "../Conversation/Input/input.component";
import { ConversationHeaderComponent } from "../Conversation/Header/conversationheader.component";
import { MessagesComponent } from "../Conversation/Messages/messages.component";
import { ScrollDispatchModule } from "@angular/cdk/scrolling";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { MaterialModule } from "../material.module";
import { CommonModule } from "@angular/common";
import { ReactiveFormsModule, FormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { AppRoutersModule } from "../app.routes";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { BrowserAnimationsModule } from "@angular/platform-browser/animations";
import { ConversationsListComponent } from "../Conversation/ConversationsList/conversationslist.component";
import { AddGroupDialogComponent } from "../Dialogs/AddGroupDialog";
import { FindUsersDialogComponent } from "../Dialogs/FindUsersDialog";
import { GroupInfoDialogComponent } from "../Dialogs/GroupInfoDialog";
import { ChangeNameDialogComponent } from "../Dialogs/ChangeNameDialog";
import { SearchListComponent } from "../Search/searchlist.component";
import { UserInfoDialogComponent } from "../Dialogs/UserInfoDialog";
import { UploaderService } from "../uploads/upload.service";
import { ViewAttachmentsDialogComponent } from "../Dialogs/ViewAttachmentsDialog";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { HttpResponseInterceptor } from "../Interceptors/HttpResponseInterceptor";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { ForwardMessagesDialogComponent } from "../Dialogs/ForwardMessagesDialog";
import { MessagesDateParserService } from "../Services/MessagesDateParserService";
import { ChatsService } from "../Services/ChatsService";
import { UsersService } from "../Services/UsersService";
import { MessageReportingService } from "../Services/MessageReportingService";
import { AuthService } from "../Auth/AuthService";
import { ThemesService } from "../Theming/ThemesService";
import { ChooseContactDialogComponent } from "../Dialogs/ChooseContactDialog";

@NgModule({
  declarations: [
    ChatComponent,
    InputComponent,
    ConversationHeaderComponent,
    MessagesComponent,
    ConversationsListComponent,
    AddGroupDialogComponent,
    FindUsersDialogComponent,
    GroupInfoDialogComponent,
    ChangeNameDialogComponent,
    UserInfoDialogComponent,
    SearchListComponent,
    ViewAttachmentsDialogComponent,
    ForwardMessagesDialogComponent,
    ChooseContactDialogComponent
  ],
  imports: [
    ScrollDispatchModule,
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
    BrowserModule,
    AppRoutersModule,
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule
  ],
  exports: [AddGroupDialogComponent],
  entryComponents: [
    AddGroupDialogComponent,
    FindUsersDialogComponent,
    GroupInfoDialogComponent,
    ChangeNameDialogComponent,
    UserInfoDialogComponent,
    ViewAttachmentsDialogComponent,
    ForwardMessagesDialogComponent,
    ChooseContactDialogComponent],

  providers: [
    ConversationsFormatter,
    MessagesDateParserService,
    UploaderService,
    SnackBarHelper,
    ChatsService,
    UsersService,
    MessageReportingService,
    AuthService,
    ThemesService,
    { provide: HTTP_INTERCEPTORS, useClass: HttpResponseInterceptor, multi: true }]
})
export class ChatModule { }
