import { UserInfo } from "./UserInfo";
import { ChatMessage } from "./ChatMessage";

export class ConversationTemplate {

  public conversationID: number;

  public dialogueUser: UserInfo;

  public name: string;

  public thumbnailUrl: string;

  public fullImageUrl: string;

  public isGroup: boolean;

  public creator: UserInfo;

  public messages: Array<ChatMessage>;

  public participants: Array<UserInfo>;

  public isMessagingRestricted: boolean;

  public authKeyId: string; 

  public messagesUnread: number;
}
