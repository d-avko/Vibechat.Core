import { Overlay, OverlayConfig } from "@angular/cdk/overlay";
import { ViewContainerRef, Component, Injector, Type, InjectionToken, InjectFlags, Inject, ViewChild, ElementRef, Injectable } from "@angular/core";
import { ChatMessage } from "../Data/ChatMessage";
import { ComponentPortal } from "@angular/cdk/portal";
import { createInjector } from "@angular/core/src/view/refs";
import { ImageScalingService } from "../Services/ImageScalingService";
import { ChatsService } from "../Services/ChatsService";
import { ForwardMessagesModel } from "../Conversation/Messages/messages.component";
import { ForwardMessagesDialogComponent } from "./ForwardMessagesDialog";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { MatDialog } from "@angular/material";

export class ViewPhotoInjector extends Injector {

  constructor(private map: WeakMap<object, object>) { super() }

  get<T>(token: Type<T> | InjectionToken<T>, notFoundValue?: T, flags?: InjectFlags): T; get(token: any, notFoundValue?: any);
    get(token: any, notFoundValue?: any, flags?: any) {
      if (this.map.has(token)) {
        return this.map.get(token);
      } else {
        return notFoundValue;
      }
    }


}

class ViewPhotoData {
  photo: ChatMessage;
  url: string;
  imageName: string;
  width: number;
  heigth: number;
  isForwardSupported: boolean;
}

@Injectable()
export class ViewPhotoService {
  constructor(private overlay: Overlay) {
    this.config = new OverlayConfig();
    this.config.hasBackdrop = true;
  }

  public viewContainerRef: ViewContainerRef;

  private config: OverlayConfig;

  public ViewPhoto(photo: ChatMessage, imageW: number, imageH: number) {
    let overlayRef = this.overlay.create(this.config);

    overlayRef.backdropClick().subscribe(() => {
      overlayRef.dispose();
    });

    let data = new ViewPhotoData();
    data.heigth = imageH;
    data.width = imageW;
    data.photo = photo;
    data.url = photo.attachmentInfo.contentUrl;
    data.imageName = photo.attachmentInfo.attachmentName;
    data.isForwardSupported = true;

    overlayRef.attach(new ComponentPortal(ViewPhotoComponent, this.viewContainerRef, this.createInjector(data)));
  }

  public ViewProfilePicture(fullImageUrl: string) {
    let overlayRef = this.overlay.create(this.config);

    overlayRef.backdropClick().subscribe(() => {
      overlayRef.dispose();
    });

    let img = new Image();
    img.onload = () => {
      let data = new ViewPhotoData();
      data.url = fullImageUrl;
      data.imageName = '';
      data.isForwardSupported = false;
      data.width = img.width;
      data.heigth = img.height;
      overlayRef.attach(new ComponentPortal(ViewPhotoComponent, this.viewContainerRef, this.createInjector(data)));
    };

    img.src = fullImageUrl;
  }

  private createInjector(data: ViewPhotoData): Injector {
    const injectorTokens = new WeakMap();
    injectorTokens.set(ViewPhotoData, data);
    return new ViewPhotoInjector(injectorTokens);
  }
}

@Component({
  selector: 'view-photo',
  templateUrl: 'view-photo.component.html'
})
export class ViewPhotoComponent {
  constructor(@Inject(ViewPhotoData) private data: ViewPhotoData,
    private images: ImageScalingService,
    private chats: ChatsService,
    public dialog: MatDialog) {

    let dimensions = this.images.AdjustFullSizedImageDimensions(data.width, data.heigth);
    this.width = dimensions.width;
    this.height = dimensions.height;
  }

  public Forward() {
    let forwardMessagesDialog = this.dialog.open(
      ForwardMessagesDialogComponent,
      {
        data: {
          conversations: this.chats.Conversations
        }
      }
    );

    forwardMessagesDialog
      .beforeClosed()
      .subscribe((result: Array<ConversationTemplate>) => {

        let forwardArray = new Array<ChatMessage>();
        forwardArray.push(this.data.photo);

        this.chats.ForwardMessagesTo(result, forwardArray);
      })
  }

  public width: number;
  public height: number;
}
