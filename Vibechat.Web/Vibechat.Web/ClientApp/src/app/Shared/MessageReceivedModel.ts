import {Message} from "../Data/Message";

export class MessageReceivedModel {
  public constructor(init?: Partial<MessageReceivedModel>) {
    (<any>Object).assign(this, init);
  }

  public senderId: string

  public message: Message

  public conversationId: number

  public secure: boolean;
}
