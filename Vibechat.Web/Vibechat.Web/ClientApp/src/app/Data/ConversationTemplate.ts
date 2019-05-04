import { UserInfo } from "./UserInfo";
import { ChatMessage } from "./Message";

export class ConversationTemplate {
  public conversationID: number;

  public dialogueUser: UserInfo;

  public name: string;

  public pictureBackground: string;

  public imageUrl: string;

  public isGroup: boolean;

  public messages: Array<ChatMessage>;

  public participants: Array<UserInfo>;

}
