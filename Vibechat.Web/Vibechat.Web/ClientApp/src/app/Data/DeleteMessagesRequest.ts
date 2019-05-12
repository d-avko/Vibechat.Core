export class DeleteMessagesRequest {
  public constructor(init?: Partial<DeleteMessagesRequest>) {
    (<any>Object).assign(this, init);
  }

  public MessagesId: Array<number>;

  public ConversationId: number;
}
