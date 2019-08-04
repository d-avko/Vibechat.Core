import {AttachmentKind} from "./AttachmentKinds";

export class Attachment{
  public contentUrl: string;

  public attachmentName: string;

  public attachmentKind: AttachmentKind;

  public imageWidth: number;

  public imageHeight: number;

  public fileSize: number;
}
