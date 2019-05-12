export class AddedToGroupModel {
  public constructor(init?: Partial<AddedToGroupModel>) {
    (<any>Object).assign(this, init);
  }

  public conversationId: number
  public userId: string;
}
