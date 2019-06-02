import { Component, Inject, EventEmitter } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { MessageAttachment } from "../Data/MessageAttachment";
import { AttachmentKinds } from "../Data/AttachmentKinds";
import { ChatMessage } from "../Data/ChatMessage";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { retry } from "rxjs/operators";

export interface AttachmentsData {
  conversation: ConversationTemplate;
}

@Component({
  selector: 'view-attachments-dialog',
  templateUrl: 'view-attachments-dialog.html',
})
export class ViewAttachmentsDialogComponent {

  public OnDownloadMedia = new EventEmitter<MessageAttachment>();

  public PhotosWeeks: Array<Array<ChatMessage>>;

  constructor(public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AttachmentsData,
    public photoDialog: MatDialog,
    public formatter: ConversationsFormatter)
  {
    this.PhotosWeeks = new Array<Array<ChatMessage>>();
    this.Init();
  }

  private Init() {

    if (this.data.conversation.messages) {
      let buffer = new Array<ChatMessage>();

      let attachments =
        this.data.conversation.messages
        .filter(x => x.isAttachment && x.attachmentInfo.attachmentKind == AttachmentKinds.Image)
          .reverse();

      let currentWeek = this.GetDaysSinceReceived(attachments[0]);

      attachments.forEach(
        (attachment) => {

          buffer.push(attachment);

          if (this.GetDaysSinceReceived(attachment) / 7 > currentWeek) {
            this.PhotosWeeks.push(new Array<ChatMessage>(...buffer));
            buffer.splice(0, buffer.length);
            currentWeek = this.GetDaysSinceReceived(attachment) / 7;
          }
          
        }
      );

      if (buffer.length != 0) {
          this.PhotosWeeks.push(new Array<ChatMessage>(...buffer));
      }
    }

  }

  private GetDaysSinceReceived(message: ChatMessage): number {
    let messageDate = (<Date>message.timeReceived).getTime();
    let nowDate = new Date().getTime();
    let x = (nowDate - messageDate) / (1000 * 60 * 60 * 24);
    x = x / 60;
    x = x / 60;
    x = x / 24;
    return Math.floor(x);
  }

  public ViewPhoto(photo: MessageAttachment) {

  }

  public OnAttachmentsScrolled(index: number) {
    //manual handling.
  }

}
