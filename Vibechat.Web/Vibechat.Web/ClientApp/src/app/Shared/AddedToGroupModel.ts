import { AppUser } from "../Data/AppUser";

export class AddedToGroupModel {
  public constructor(init?: Partial<AddedToGroupModel>) {
    (<any>Object).assign(this, init);
  }

  public chatId: number;
  public user: AppUser;
}
