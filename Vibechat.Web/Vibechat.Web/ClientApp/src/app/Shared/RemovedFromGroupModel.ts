export class RemovedFromGroupModel {
  public constructor(init?: Partial<RemovedFromGroupModel>) {
    (<any>Object).assign(this, init);
  }

  public conversationId: number
  public userId: string;
}
