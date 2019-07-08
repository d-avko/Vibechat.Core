import { UserInfo } from "./UserInfo";
import { ChatMessage } from "./ChatMessage";

export class DhPublicKey {
  public modulus: string;
  public generator: string;
}

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

  public isSecure: boolean;

  public authKeyId: string;

  public publicKey: DhPublicKey; 

  public messagesUnread: number;

  public deviceId: string;
}
