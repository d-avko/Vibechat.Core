export class ConversationMessagesRequest
{
  public constructor(init?: Partial<ConversationMessagesRequest>) {
    (<any>Object).assign(this, init);
  }

  public ConversationID: number;

  public MesssagesOffset: number;

  public Count: number;
}

