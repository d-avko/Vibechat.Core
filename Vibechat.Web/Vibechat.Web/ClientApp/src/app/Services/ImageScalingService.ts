import { Injectable } from "@angular/core";
import { ChatMessage } from "../Data/ChatMessage";
import { AttachmentKind } from "../Data/AttachmentKinds";
import { ConversationsFormatter } from "../Formatters/ConversationsFormatter";

export class Dimensions {
  height: number;
  width: number;
}

@Injectable({
  providedIn: 'root'
})
export class ImageScalingService {

  constructor(private formatter: ConversationsFormatter) { }

  public chatImageMaxHeightRatio = 0.3;

  public fullScreenImageToScreenDesktopRatio = 0.70;

  public fullScreenImageToScreenMobileRatio = 0.45;

  public chatImageWidthRatioMobile = 0.5;

  public chatImageWidthRatioDesktop = 0.4;

  public ScaleImages(messages: Array<ChatMessage>) {

    for (let i = 0; i < messages.length; ++i) {

      if (this.IsImage(messages[i])) {

        let d = this.GetChatImageDimensions(messages[i].attachmentInfo.imageWidth, messages[i].attachmentInfo.imageHeight);
        messages[i].attachmentInfo.imageWidth = d.width;
        messages[i].attachmentInfo.imageHeight = d.height;
      } else if (this.IsImage(messages[i].forwardedMessage)) {

        let d = this.GetChatImageDimensions(messages[i].forwardedMessage.attachmentInfo.imageWidth, messages[i].forwardedMessage.attachmentInfo.imageHeight);
        messages[i].forwardedMessage.attachmentInfo.imageWidth = d.width;
        messages[i].forwardedMessage.attachmentInfo.imageHeight = d.height;
      }
    }
  }

  public IsImage(chat: ChatMessage) {
    if (!chat) {
      return false;
    }
      
    return chat.isAttachment && chat.attachmentInfo.attachmentKind == AttachmentKind.Image;
  }

  public ScaleImage(message: ChatMessage) {

    if (message.isAttachment && message.attachmentInfo.attachmentKind == AttachmentKind.Image) {
      let d = this.GetChatImageDimensions(message.attachmentInfo.imageWidth, message.attachmentInfo.imageHeight);
      message.attachmentInfo.imageWidth = d.width;
      message.attachmentInfo.imageHeight = d.height;
    }
  }

  /* Method is used to get dimensions of image in CHAT
   * */
  private GetChatImageDimensions(oldWidth: number, oldHeight: number) {
    let d = new Dimensions();

    let desiredWidth;

    if (this.formatter.IsMobileDevice()) {
      desiredWidth = document.documentElement.clientWidth * this.chatImageWidthRatioMobile;
    } else {
      desiredWidth = document.documentElement.clientWidth * this.chatImageWidthRatioDesktop;
    }

    let desiredHeight = document.documentElement.clientHeight * this.chatImageMaxHeightRatio;

    if (oldWidth > oldHeight) {
      d.width = desiredWidth;

      d.height = Math.floor(desiredWidth * (oldHeight / oldWidth));
    }
    else if (oldWidth < oldHeight) {
      d.height = desiredHeight;

      d.width = Math.floor(desiredHeight * (oldWidth / oldHeight));
    }
    else {
      d.width = d.height = desiredWidth;
    }


    return d;
  }

/* Method is used to get dimensions of image in full-screen
 * */
  public AdjustFullSizedImageDimensions(imageW: number, imageH: number) {
    let appropriateHeight = this.formatter.IsMobileDevice()
      ? Math.floor(document.documentElement.clientHeight * this.fullScreenImageToScreenMobileRatio)
      : Math.floor(document.documentElement.clientHeight * this.fullScreenImageToScreenDesktopRatio)

    let d = new Dimensions();
    d.height = appropriateHeight;
    d.width = (imageW / imageH) * appropriateHeight;

    return d;
  }
}
