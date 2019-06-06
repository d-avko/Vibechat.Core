import { UserInfo } from "./UserInfo";
import { MessageAttachment } from "./MessageAttachment";

export class ChatMessage {
  constructor(init?: Partial<ChatMessage>) {
    (<any>Object).assign(this, init);
  }

  public id: number;

  public user: UserInfo;
    
  public messageContent: string;
    
  public conversationID: number;

  public state: number;
    
  public timeReceived: Date | string;

  public attachmentInfo: MessageAttachment;

  public forwardedMessage: ChatMessage;

  public isAttachment: boolean;
}
