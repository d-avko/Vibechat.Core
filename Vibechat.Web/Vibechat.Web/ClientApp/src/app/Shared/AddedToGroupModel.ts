import { ConversationTemplate } from "../Data/ConversationTemplate";
import { UserInfo } from "../Data/UserInfo";

export class AddedToGroupModel {
  public constructor(init?: Partial<AddedToGroupModel>) {
    (<any>Object).assign(this, init);
  }

  public conversation: ConversationTemplate
  public user: UserInfo;
}
