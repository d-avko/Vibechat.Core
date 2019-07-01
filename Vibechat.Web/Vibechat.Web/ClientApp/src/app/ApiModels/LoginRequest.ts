export class LoginRequest {
  public constructor(init?: Partial<LoginRequest>) {
    (<any>Object).assign(this, init);
  }

  public UidToken: string;
  public PhoneNumber: string;
}
