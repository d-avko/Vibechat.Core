import { UserInfo } from "./UserInfo";
import { MessageAttachment } from "./MessageAttachment";

export class ChatMessage {
  public user: UserInfo;
    
  public messageContent: string;
    
  public conversationID: number;
    
  public timeReceived: string;

  public Attachments: Array<MessageAttachment>;
}
