import { AttachmentKind } from "./AttachmentKinds";

export class MessageAttachment{
  public contentUrl: string;

  public attachmentName: string;

  public attachmentKind: AttachmentKind;

  public imageWidth: number;

  public imageHeight: number;

  public fileSize: number;
}
