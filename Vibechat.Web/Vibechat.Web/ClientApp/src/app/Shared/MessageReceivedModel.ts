import { ChatMessage } from "../Data/ChatMessage";

export class MessageReceivedModel {
  public constructor(init?: Partial<MessageReceivedModel>) {
    (<any>Object).assign(this, init);
  }

  public senderId: string

  public message: ChatMessage

  public conversationId: number

  public secure: boolean;
}
