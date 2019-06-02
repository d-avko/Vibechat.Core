import { Component, Inject, EventEmitter, ViewChild } from "@angular/core";
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from "@angular/material";
import { ChatComponent } from "../Chat/chat.component";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { MessageAttachment } from "../Data/MessageAttachment";
import { AttachmentKinds } from "../Data/AttachmentKinds";
import { ChatMessage } from "../Data/ChatMessage";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";
import { retry } from "rxjs/operators";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { CdkVirtualScrollViewport } from "@angular/cdk/scrolling";

export interface AttachmentsData {
  conversation: ConversationTemplate;
  token: string;
}

@Component({
  selector: 'view-attachments-dialog',
  templateUrl: 'view-attachments-dialog.html',
})
export class ViewAttachmentsDialogComponent {

  public OnDownloadMedia = new EventEmitter<MessageAttachment>();

  public PhotosWeeks: Array<Array<ChatMessage>>;

  @ViewChild(CdkVirtualScrollViewport) viewport: CdkVirtualScrollViewport;

  private static attachmentsToLoadAmount: number = 50;

  private static allowedErrorInOffset: number = 60;

  private PhotosLoading: boolean = false;

  private IsPhotosEnd: boolean = false;

  constructor(public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AttachmentsData,
    public photoDialog: MatDialog,
    public formatter: ConversationsFormatter,
    public requestsBuilder: ApiRequestsBuilder)
  {
    this.PhotosWeeks = new Array<Array<ChatMessage>>();
    this.Init();
  }
  //photos should be sorted new to old.
  private AddPhotos(photos: Array<ChatMessage>) {
    if (!photos) {
      return;
    }

    let currentWeek = 0;

    if (this.PhotosWeeks.length != 0) {
      let lastWeek = this.PhotosWeeks[this.PhotosWeeks.length - 1];
      currentWeek = Math.floor(this.GetDaysSinceReceived(lastWeek[lastWeek.length - 1]) / 7);
    } else {
      currentWeek = Math.floor(this.GetDaysSinceReceived(photos[0]) / 7);
    }


    photos.forEach(
      (photo) => {

        let weeksSinceReceived = Math.floor(this.GetDaysSinceReceived(photo) / 7);

        if (weeksSinceReceived > currentWeek) {
          this.PhotosWeeks.push(new Array<ChatMessage>());
          currentWeek = Math.floor(this.GetDaysSinceReceived(photo) / 7);
        }

        if (this.PhotosWeeks.length == 0) {
          this.PhotosWeeks.push(new Array<ChatMessage>());
        }

        this.PhotosWeeks[this.PhotosWeeks.length - 1].push(photo);
      }
    );

    this.PhotosWeeks = [...this.PhotosWeeks];
  }

  private Init() {
    //just load local messages
    if (this.data.conversation.messages) {
      let attachments =
        this.data.conversation.messages
        .filter(x => x.isAttachment && x.attachmentInfo.attachmentKind == AttachmentKinds.Image)
          .reverse();

      this.AddPhotos(attachments);
    }

  }

  private GetDaysSinceReceived(message: ChatMessage): number {
    let messageDate = (<Date>message.timeReceived).getTime();
    let nowDate = new Date().getTime();
    let x = (nowDate - messageDate) / 1000;
    x = x / 60;
    x = x / 60;
    x = x / 24;
    return Math.floor(x);
  }

  public ViewPhoto(photo: MessageAttachment) {

  }

  public UpdatePhotos() {
    let offset = 0;
    this.PhotosWeeks.forEach(x => offset += x.length);

    this.requestsBuilder.GetAttachmentsForConversation(
      this.data.conversation.conversationID,
      AttachmentKinds.Image,
      this.data.token,
      offset,
      ViewAttachmentsDialogComponent.attachmentsToLoadAmount)
        .subscribe((result) => {

          if (!result.isSuccessfull) {
            this.PhotosLoading = false;
            return;
          }

          if (this.IsPhotosEnd) {
            this.PhotosLoading = false;
            return;
          }

          if (result.response == null || result.response.length == 0) {
            this.IsPhotosEnd = true;
            this.PhotosLoading = false;
            return;
          }

          this.PhotosLoading = false;
          result.response.forEach(x => x.timeReceived = new Date(x.timeReceived));
          this.AddPhotos(result.response);
        });
  }

  public OnAttachmentsScrolled(index: number) {

    if (this.PhotosLoading || this.IsPhotosEnd) {
      return;
    }

    let fullViewPortSize = this.viewport.measureRenderedContentSize();
    let currentOffset = this.viewport.measureScrollOffset() + this.viewport.getViewportSize() + ViewAttachmentsDialogComponent.allowedErrorInOffset;
    console.log(`in event section current offset ${currentOffset}, size: ${fullViewPortSize}`);
    if (fullViewPortSize <= currentOffset) {
      // user scrolled to last attachment, load more.
      this.PhotosLoading = true;
      console.log("in update section");
      this.UpdatePhotos();
    }
  }

}
