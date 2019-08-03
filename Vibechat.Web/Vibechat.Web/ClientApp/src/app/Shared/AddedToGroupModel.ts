import { UserInfo } from "../Data/UserInfo";

export class AddedToGroupModel {
  public constructor(init?: Partial<AddedToGroupModel>) {
    (<any>Object).assign(this, init);
  }

  public chatId: number;
  public user: UserInfo;
}
