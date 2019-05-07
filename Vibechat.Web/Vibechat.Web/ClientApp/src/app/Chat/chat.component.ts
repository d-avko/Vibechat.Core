import { Component } from "@angular/core";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ConversationResponse } from "../ApiModels/ConversationResponse";
import { Cache } from "../Auth/Cache";
import { ServerResponse } from "../ApiModels/ServerResponse";
import { MatSnackBar } from "@angular/material";
import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { Router } from "@angular/router";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { ConversationsFormatter } from "../Data/ConversationsFormatter";

@Component({
  selector: 'chat-root',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent {

  public Conversations: Array<ConversationTemplate>;

  protected snackbar: SnackBarHelper;

  protected router: Router;

  protected requestsBuilder: ApiRequestsBuilder;

  public formatter: ConversationsFormatter;


  constructor(requestsBuilder: ApiRequestsBuilder, snackbar: MatSnackBar, router: Router, formatter: ConversationsFormatter) {

    this.snackbar = new SnackBarHelper(snackbar);
    this.router = router;
    this.requestsBuilder = requestsBuilder;
    this.Conversations = new Array<ConversationTemplate>();
    this.formatter = formatter;
    
    if (!Cache.IsAuthenticated) {

      this.router.navigateByUrl('/login');

    } else {
      this.UpdateConversations();
    }

  }

  public UpdateConversations() {
    this.requestsBuilder.UpdateConversationsRequest(Cache.JwtToken, Cache.UserCache.id)
      .subscribe((result) => this.OnConversationsUpdated(result))
  }

  public OnConversationsUpdated(response: ServerResponse<ConversationResponse>): void {

    if (!response.isSuccessfull) {
      this.snackbar.openSnackBar(response.errorMessage);
      return;
    }

    this.Conversations = response.response.conversations;
  }

  public IsCurrentConversation(conversation: ConversationTemplate): boolean {
    if (Cache.CurrentConversation == null)
      return false;

    return Cache.CurrentConversation.conversationID == conversation.conversationID;
  }

  public GetCurrentDialogueUserImageUrl() : string {
    return Cache.CurrentConversation.dialogueUser.imageUrl;
  }

  public GetCurrentConversationImageUrl(): string {
    return Cache.CurrentConversation.imageUrl;
  }

  public IsConversationSelected(): boolean {
    return Cache.CurrentConversation != null;
  }

  public IsCurrentConversationGroup(): boolean {
    return Cache.CurrentConversation.isGroup;
  }

  public GetCurrentConversationName(): string {

    if (Cache.CurrentConversation.name == '' || Cache.CurrentConversation.name == null) {
      return Cache.CurrentConversation.dialogueUser.userName;
    }

    return Cache.CurrentConversation.name;
  }

  public GetThisUserImageUrl(): string {
    return Cache.UserCache.imageUrl;
  }

  public SendMessage() {
    //TODO
  }

  public GetCurrentConversationMembersFormatted(): string {
    return this.formatter.GetConversationMembersFormatted(Cache.CurrentConversation);
  }

  public ChangeConversation(conversation: ConversationTemplate): void {
    if (conversation == Cache.CurrentConversation) {
      Cache.CurrentConversation = null;
      return;
    }

    Cache.CurrentConversation = conversation;
  }
}
