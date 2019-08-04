import {AppUser} from "./AppUser";
import {Attachment} from "./Attachment";

export class Message {
  constructor(init?: Partial<Message>) {
    (<any>Object).assign(this, init);
  }

  public id: number;

  public user: AppUser;

  public messageContent: string;

  public conversationID: number;

  public state: number;

  public timeReceived: Date | string;

  public attachmentInfo: Attachment;

  public forwardedMessage: Message;

  public isAttachment: boolean;

  public encryptedPayload: string;
}
