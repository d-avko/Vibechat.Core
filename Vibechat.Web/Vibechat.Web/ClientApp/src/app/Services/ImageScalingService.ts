import { Injectable } from "@angular/core";
import { ChatMessage } from "../Data/ChatMessage";
import { AttachmentKinds } from "../Data/AttachmentKinds";

export class Dimensions {
  height: number;
  width: number;
}

@Injectable({
  providedIn: 'root'
})
export class ImageScalingService {

  public ScaleImages(messages: Array<ChatMessage>) {

    for (let i = 0; i < messages.length; ++i) {

      if (messages[i].isAttachment && messages[i].attachmentInfo.attachmentKind == AttachmentKinds.Image) {
        let d = this.GetAppropriateDimensions(messages[i].attachmentInfo.imageWidth, messages[i].attachmentInfo.imageHeight);
        messages[i].attachmentInfo.imageWidth = d.width;
        messages[i].attachmentInfo.imageHeight = d.height;
      }

    }
  }

  public ScaleImage(message: ChatMessage) {

    if (message.isAttachment && message.attachmentInfo.attachmentKind == AttachmentKinds.Image) {
      let d = this.GetAppropriateDimensions(message.attachmentInfo.imageWidth, message.attachmentInfo.imageHeight);
      message.attachmentInfo.imageWidth = d.width;
      message.attachmentInfo.imageHeight = d.height;
    }
  }

  private GetAppropriateDimensions(oldWidth: number, oldHeight: number) : Dimensions {
    let appropriateWidth = Math.floor(window.innerWidth / 2);
    let appropriateHeight = appropriateWidth;

    let resultingHeight: number, resultingWidth: number;

    if (oldWidth > oldHeight) {
      resultingWidth = appropriateWidth;

      resultingHeight = Math.floor(appropriateWidth * (oldHeight / oldWidth));
    }
    else if (oldWidth < oldHeight) {
      resultingHeight = appropriateHeight;

      resultingWidth = Math.floor(appropriateHeight * (oldWidth / oldHeight));
    }
    else {
      resultingWidth = resultingHeight = appropriateWidth;
    }

    let d = new Dimensions();
    d.height = resultingHeight;
    d.width = resultingWidth;

    return d;
  }
}
