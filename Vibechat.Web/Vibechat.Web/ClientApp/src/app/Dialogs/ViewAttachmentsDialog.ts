import { Component, Inject, EventEmitter } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { MessageAttachment } from "../Data/MessageAttachment";
import { AttachmentKinds } from "../Data/AttachmentKinds";
import { ChatMessage } from "../Data/ChatMessage";

export interface AttachmentsData {
  conversation: ConversationTemplate;
}

@Component({
  selector: 'view-attachments-dialog',
  templateUrl: 'view-attachments-dialog.html',
})
export class ViewAttachmentsDialogComponent {

  public OnDownloadMedia = new EventEmitter<MessageAttachment>();

  public Photos: Array<ChatMessage>;

  constructor(public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AttachmentsData,
    public photoDialog: MatDialog)
  {
    this.Init();
  }

  private Init() {

    if (this.data.conversation.messages) {
      this.Photos = this.data.conversation.messages.filter(x => x.isAttachment && x.attachmentInfo.attachmentKind == AttachmentKinds.Image);
    }

  }

  public ViewPhoto(photo: MessageAttachment) {

  }

  public OnAttachmentsScrolled(index: number) {

  }

}
