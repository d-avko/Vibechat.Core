import {Component, EventEmitter, Inject, ViewChild, ViewContainerRef} from "@angular/core";
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef, MatTabChangeEvent} from "@angular/material";
import {ChatComponent} from "../Chat/chat.component";
import {Chat} from "../Data/Chat";
import {MessageAttachment} from "../Data/MessageAttachment";
import {AttachmentKind} from "../Data/AttachmentKinds";
import {ChatMessage} from "../Data/ChatMessage";
import {ConversationsFormatter} from "../Formatters/ConversationsFormatter";
import {CdkVirtualScrollViewport} from "@angular/cdk/scrolling";
import {ChatsService} from "../Services/ChatsService";
import {ViewPhotoService} from "./ViewPhotoService";

export interface AttachmentsData {
  conversation: Chat;
}

@Component({
  selector: 'view-attachments-dialog',
  templateUrl: 'view-attachments-dialog.html',
})
export class ViewAttachmentsDialogComponent {

  public OnDownloadMedia = new EventEmitter<MessageAttachment>();

  public PhotosWeeks: Array<Array<ChatMessage>>;

  public FilesWeeks: Array<Array<ChatMessage>>;

  @ViewChild(CdkVirtualScrollViewport, { static: true }) scroll: CdkVirtualScrollViewport;

  private static attachmentsToLoadAmount: number = 50;

  private static allowedErrorInOffset: number = 60;

  private PhotosLoading: boolean = false;

  private IsPhotosEnd: boolean = false;

  private IsFilesInitialized: boolean = false;

  private IsFilesEnd: boolean = false;

  private FilesLoading: boolean = false;

  constructor(public dialogRef: MatDialogRef<ChatComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AttachmentsData,
    public photoDialog: MatDialog,
    public formatter: ConversationsFormatter,
    public conversationsService: ChatsService,
    public photos: ViewPhotoService,
    public viewContainerRef: ViewContainerRef)
  {
    this.PhotosWeeks = new Array<Array<ChatMessage>>();
    this.FilesWeeks = new Array<Array<ChatMessage>>();
    this.Init();
    this.photos.viewContainerRef = viewContainerRef;
  }
  //photos should be sorted new to old.
  private AddPhotos(photos: Array<ChatMessage>) {
    if (photos.length == 0) {
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

  public AddFiles(files: Array<ChatMessage>) {
    if (files.length == 0) {
      return;
    }

    let currentWeek = 0;

    if (this.FilesWeeks.length != 0) {
      let lastWeek = this.FilesWeeks[this.FilesWeeks.length - 1];
      currentWeek = Math.floor(this.GetDaysSinceReceived(lastWeek[lastWeek.length - 1]) / 7);
    } else {
      currentWeek = Math.floor(this.GetDaysSinceReceived(files[0]) / 7);
    }


    files.forEach(
      (file) => {

        let weeksSinceReceived = Math.floor(this.GetDaysSinceReceived(file) / 7);

        if (weeksSinceReceived > currentWeek) {
          this.FilesWeeks.push(new Array<ChatMessage>());
          currentWeek = Math.floor(this.GetDaysSinceReceived(file) / 7);
        }

        if (this.FilesWeeks.length == 0) {
          this.FilesWeeks.push(new Array<ChatMessage>());
        }

        this.FilesWeeks[this.FilesWeeks.length - 1].push(file);
      }
    );

    this.FilesWeeks = [...this.FilesWeeks];
  }

  private Init() {
    //just load local messages
    if (this.data.conversation.messages) {
      let attachments =
        this.data.conversation.messages
        .filter(x => x.isAttachment && x.attachmentInfo.attachmentKind == AttachmentKind.Image)
          .reverse();

      this.AddPhotos(attachments);
    }

  }

  public TabChanged(event: MatTabChangeEvent) {

    //files tab
    if (event.index == 1) {
      this.InitFiles();
    }
    //do not init images as they are initialized during component construction.
  }

  public InitFiles() {
    if (this.IsFilesInitialized) {
      return;
    }

    if (this.data.conversation.messages) {
      let attachments =
        this.data.conversation.messages
          .filter(x => x.isAttachment && x.attachmentInfo.attachmentKind == AttachmentKind.File)
          .reverse();

      this.AddFiles(attachments);
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

  public ViewPhoto(event: Event, photo: ChatMessage) {
    this.photos.ViewPhoto(photo);
  }

  public async UpdateFiles() {

    if (this.IsFilesEnd || this.FilesLoading) {
      return;
    }

    let offset = 0;
    this.FilesWeeks.forEach(x => offset += x.length);
    this.PhotosLoading = true;

    let result = await this.conversationsService.GetAttachmentsFor(
      this.data.conversation.id,
      AttachmentKind.File,
      offset,
      ViewAttachmentsDialogComponent.attachmentsToLoadAmount);

    this.FilesLoading = false;

    if (result == null || result.length == 0) {
      this.IsFilesEnd = true;
      return;
    }

    this.AddFiles(result);
  }

  public async UpdatePhotos() {

    if (this.IsPhotosEnd || this.PhotosLoading) {
      return;
    }

    let offset = 0;
    this.PhotosWeeks.forEach(x => offset += x.length);
    this.PhotosLoading = true;

    let result = await this.conversationsService.GetAttachmentsFor(
      this.data.conversation.id,
      AttachmentKind.Image,
      offset,
      ViewAttachmentsDialogComponent.attachmentsToLoadAmount);

    this.PhotosLoading = false;

    if (result == null || result.length == 0) {
      this.IsPhotosEnd = true;
      return;
    }

    this.AddPhotos(result);
  }

  public OnPhotosScrolled(index: number) {

    let fullViewPortSize = this.scroll.measureRenderedContentSize();
    let currentOffset = this.scroll.measureScrollOffset() + this.scroll.getViewportSize() + ViewAttachmentsDialogComponent.allowedErrorInOffset;

    if (fullViewPortSize <= currentOffset) {
      // user scrolled to last attachment, load more.
      this.UpdatePhotos();
    }
  }

  public OnFilesScrolled(index: number) {
    let fullViewPortSize = this.scroll.measureRenderedContentSize();
    let currentOffset = this.scroll.measureScrollOffset() + this.scroll.getViewportSize() + ViewAttachmentsDialogComponent.allowedErrorInOffset;

    if (fullViewPortSize <= currentOffset) {
      // user scrolled to last attachment, load more.
      this.UpdateFiles();
    }
  }

}
