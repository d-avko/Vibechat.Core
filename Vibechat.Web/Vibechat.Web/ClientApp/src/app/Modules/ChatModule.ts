import {NgModule} from "@angular/core";
import {ChatComponent} from "../UiComponents/Chat/chat.component";
import {InputComponent} from "../UiComponents/Input/input.component";
import {ConversationHeaderComponent} from "../UiComponents/Header/conversationheader.component";
import {MessagesComponent} from "../UiComponents/Messages/messages.component";
import {ScrollingModule} from "@angular/cdk/scrolling";
import {ConversationsFormatter} from "../Formatters/ConversationsFormatter";
import {MaterialModule} from "../material.module";
import {CommonModule} from "@angular/common";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {BrowserModule} from "@angular/platform-browser";
import {AppRoutersModule} from "../app.routes";
import {HTTP_INTERCEPTORS, HttpClientModule} from "@angular/common/http";
import {BrowserAnimationsModule} from "@angular/platform-browser/animations";
import {ConversationsListComponent} from "../UiComponents/ConversationsList/conversationslist.component";
import {AddGroupDialogComponent} from "../Dialogs/AddGroupDialog";
import {FindUsersDialogComponent} from "../Dialogs/FindUsersDialog";
import {GroupInfoDialogComponent} from "../Dialogs/GroupInfoDialog";
import {ChangeNameDialogComponent} from "../Dialogs/ChangeNameDialog";
import {SearchListComponent} from "../Search/searchlist.component";
import {UserInfoDialogComponent} from "../Dialogs/UserInfoDialog";
import {UploaderService} from "../uploads/upload.service";
import {ViewAttachmentsDialogComponent} from "../Dialogs/ViewAttachmentsDialog";
import {HttpResponseInterceptor} from "../Interceptors/HttpResponseInterceptor";
import {SnackBarHelper} from "../Snackbar/SnackbarHelper";
import {ForwardMessagesDialogComponent} from "../Dialogs/ForwardMessagesDialog";
import {ChatsService} from "../Services/ChatsService";
import {UsersService} from "../Services/UsersService";
import {MessageReportingService} from "../Services/MessageReportingService";
import {AuthService} from "../Services/AuthService";
import {ThemesService} from "../Theming/ThemesService";
import {ChooseContactDialogComponent} from "../Dialogs/ChooseContactDialog";
import {ImageScalingService} from "../Services/ImageScalingService";
import {DownloadsService} from "../downloads/downloads.service";
import {ViewPhotoComponent, ViewPhotoService} from "../Dialogs/ViewPhotoService";
import {DeviceService} from "../Services/DeviceService";
import {TypingService} from "../Services/TypingService";
import {ChatUsersDialogComponent} from "../Dialogs/ChatUsersDialog";
import {AdminPanelDialog} from "../Dialogs/AdminPanelDialog";
import {ImageWithLoadProgress} from "../Shared/ImageWithLoadProgress";
import {MessageViewOptions} from "../Shared/MessageViewOptions";
import {UiScrollModule} from "ngx-ui-scroll";
import { ChatEventComponent } from '../chat-event-module/chat-event.component';

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
    ChooseContactDialogComponent,
    ChatUsersDialogComponent,
    AdminPanelDialog,
    ViewPhotoComponent,
    ChatEventComponent
  ],
  imports: [
    ScrollingModule,
    MaterialModule,
    CommonModule,
    ReactiveFormsModule,
    BrowserModule,
    AppRoutersModule,
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
    UiScrollModule
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
    ChooseContactDialogComponent,
    ChatUsersDialogComponent,
    AdminPanelDialog,
    ViewPhotoComponent],

  providers: [
    ConversationsFormatter,
    UploaderService,
    SnackBarHelper,
    ChatsService,
    UsersService,
    MessageReportingService,
    AuthService,
    ThemesService,
    { provide: HTTP_INTERCEPTORS, useClass: HttpResponseInterceptor, multi: true },
    ImageScalingService,
    DownloadsService,
    ViewPhotoService,
    DeviceService,
    TypingService,
    ImageWithLoadProgress,
    MessageViewOptions]
})
export class ChatModule { }
