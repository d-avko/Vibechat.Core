import { LoginResponse } from "../ApiModels/LoginResponse";
import { UserInfo } from "../Data/UserInfo";

export class Cache {
  public static UserCache: UserInfo;

  public static JwtToken: string;

  public static IsAuthenticated: boolean;

  public static OnUserLoggedIn(credentials: LoginResponse): void {
    this.UserCache = credentials.info;
    this.JwtToken = credentials.token;
    this.IsAuthenticated = true;
  }
}
