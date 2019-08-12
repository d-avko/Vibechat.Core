import {AppUser} from "./AppUser";
import {Attachment} from "./Attachment";
import {MessageType} from "./MessageType";
import {ChatEvent} from "./ChatEvent";

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

  public type: MessageType;

  public event: ChatEvent;

  public encryptedPayload: string;
}
