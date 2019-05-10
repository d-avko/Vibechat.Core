import { UserInfo } from "./UserInfo";
import { MessageAttachment } from "./MessageAttachment";

export class ChatMessage {
  public id: number;

  public user: UserInfo;
    
  public messageContent: string;
    
  public conversationID: number;
    
  public timeReceived: string;

  public attachments: Array<MessageAttachment>;
}
