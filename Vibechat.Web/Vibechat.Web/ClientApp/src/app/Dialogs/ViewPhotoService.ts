import {Overlay, OverlayConfig} from "@angular/cdk/overlay";
import {
  AfterContentInit,
  Component,
  ElementRef,
  Inject,
  Injectable,
  InjectFlags,
  InjectionToken,
  Injector,
  Type,
  ViewChild,
  ViewContainerRef
} from "@angular/core";
import {Message} from "../Data/Message";
import {ComponentPortal} from "@angular/cdk/portal";
import {ImageScalingService} from "../Services/ImageScalingService";
import {ChatsService} from "../Services/ChatsService";
import {ForwardMessagesDialogComponent} from "./ForwardMessagesDialog";
import {Chat} from "../Data/Chat";
import {MatDialog} from "@angular/material";
import {ImageWithLoadProgress} from "../Shared/ImageWithLoadProgress";

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

export class ViewPhotoData {
  photo: Message;
  fullsizedUrl: string;
  imageName: string;
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

  public ViewPhoto(photo: Message) {
    let overlayRef = this.overlay.create(this.config);

    let data = new ViewPhotoData();
    data.photo = photo;
    data.fullsizedUrl = photo.attachmentInfo.contentUrl;
    data.imageName = photo.attachmentInfo.attachmentName;
    data.isForwardSupported = true;

    let component = overlayRef.attach(new ComponentPortal(ViewPhotoComponent, this.viewContainerRef, this.createInjector(data)));

    overlayRef.backdropClick().subscribe(() => {
      overlayRef.detach();
      overlayRef.dispose();
      this.viewContainerRef.clear();
      component.destroy();
    });
  }

  public ViewProfilePicture(fullImageUrl: string) {
    let overlayRef = this.overlay.create(this.config);

    let data = new ViewPhotoData();
    data.photo = null;
    data.fullsizedUrl = fullImageUrl;
    data.imageName = '';
    data.isForwardSupported = false;

    let component = overlayRef.attach(new ComponentPortal(ViewPhotoComponent, this.viewContainerRef, this.createInjector(data)));

    overlayRef.backdropClick().subscribe(() => {
      overlayRef.detach();
      overlayRef.dispose();
      this.viewContainerRef.clear();
      component.destroy();
    });
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
export class ViewPhotoComponent implements AfterContentInit  {

  @ViewChild('mainImage', { static: true }) image: ElementRef;

  constructor(@Inject(ViewPhotoData) public data: ViewPhotoData,
    public loadingImage: ImageWithLoadProgress,
    public images: ImageScalingService,
    public chats: ChatsService,
    public dialog: MatDialog) { }

  ngAfterContentInit() {
    this.InitProfileOrGroupPicture()
  }

  private InitProfileOrGroupPicture() {
    this.loadingImage.load(this.data.fullsizedUrl);
    this.image.nativeElement.replaceWith(this.loadingImage.internalImg);
  }

  public Forward() {
    let forwardMessagesDialog = this.dialog.open(
      ForwardMessagesDialogComponent,
      {
        width: '350px',
        data: {
          conversations: this.chats.Chats
        }
      }
    );

    forwardMessagesDialog
      .beforeClosed()
      .subscribe((result: Array<Chat>) => {

        let forwardArray = new Array<Message>();
        forwardArray.push(this.data.photo);

        this.chats.ForwardMessagesTo(result, forwardArray);
      })
  }
}
