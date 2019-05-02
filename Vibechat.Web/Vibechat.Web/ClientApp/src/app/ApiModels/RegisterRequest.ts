export class RegisterRequest {
  public constructor(init?: Partial<RegisterRequest>) {
    (<any>Object).assign(this, init);
  }

  public Email: string;
  public Password: string;
  public UserName: string;
  public FirstName: string;
  public LastName: string;
}
