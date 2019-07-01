export class RegisterRequest {
  public constructor(init?: Partial<RegisterRequest>) {
    (<any>Object).assign(this, init);
  }

  public PhoneNumber: string;
  public UserName: string;
  public FirstName: string;
  public LastName: string;
}
