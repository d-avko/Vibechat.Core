export class LoginRequest {
  public constructor(init?: Partial<LoginRequest>) {
    (<any>Object).assign(this, init);
  }

  public UserNameOrEmail: string;
  public Password: string
}
