export class ChangeUserInfoRequest {
  public constructor(init?: Partial<ChangeUserInfoRequest>) {
    (<any>Object).assign(this, init);
  }

 // public PhoneNumber: string;
  public UserName: string;
  public FirstName: string;
  public LastName: string;
}
