export class ConversationRequest {
    public constructor(init ?: Partial<ConversationRequest>){
        (<any>Object).assign(this, init);
    }

  public UserId: string
}
