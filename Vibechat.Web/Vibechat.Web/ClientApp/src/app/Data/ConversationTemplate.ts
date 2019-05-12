import { UserInfo } from "./UserInfo";
import { ChatMessage } from "./ChatMessage";
import { Cache } from "../Auth/Cache";

export class ConversationTemplate {

  public conversationID: number;

  public dialogueUser: UserInfo;

  public name: string;

  public imageUrl: string;

  public isGroup: boolean;

  public messages: Array<ChatMessage>;

  public participants: Array<UserInfo>;

}