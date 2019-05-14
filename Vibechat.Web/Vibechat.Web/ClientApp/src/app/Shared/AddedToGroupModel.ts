import { ConversationTemplate } from "../Data/ConversationTemplate";

export class AddedToGroupModel {
  public constructor(init?: Partial<AddedToGroupModel>) {
    (<any>Object).assign(this, init);
  }

  public conversation: ConversationTemplate
  public userId: string;
}
