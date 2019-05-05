export class ConversationsRequest {
    public constructor(init ?: Partial<ConversationsRequest>){
        (<any>Object).assign(this, init);
    }

  public UserId: string
}
