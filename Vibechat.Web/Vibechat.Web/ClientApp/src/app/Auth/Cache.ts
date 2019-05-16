import { LoginResponse } from "../ApiModels/LoginResponse";
import { UserInfo } from "../Data/UserInfo";
import { ConversationTemplate } from "../Data/ConversationTemplate";

export class Cache {
  public static UserCache: UserInfo;

  public static CurrentConversation: ConversationTemplate;

  public static JwtToken: string;

  public static IsAuthenticated: boolean;

  public static OnUserLoggedIn(credentials: LoginResponse): void {
    this.UserCache = credentials.info;
    this.JwtToken = credentials.token;
    localStorage.setItem('token', this.JwtToken);
    localStorage.setItem('user', JSON.stringify(credentials.info));
    this.IsAuthenticated = true;
  }

  public static TryAuthenticate(): boolean {
    let token = localStorage.getItem('token');
    let user = <UserInfo>JSON.parse(localStorage.getItem('user'));

    if (token == null || user == null) {
      return false;
    }

    this.UserCache = user;
    this.JwtToken = token;
    this.IsAuthenticated = true;

    return true;
  }

  public static LogOut(): void {
    localStorage.clear();
  }

}
