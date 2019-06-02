import { LoginResponse } from "../ApiModels/LoginResponse";
import { UserInfo } from "../Data/UserInfo";
import { ConversationTemplate } from "../Data/ConversationTemplate";

export class Cache {
  public static UserCache: UserInfo;

  public static token: string;

  public static CurrentConversation: ConversationTemplate;

  public static IsAuthenticated: boolean;

  public static OnUserLoggedIn(credentials: LoginResponse): void {
    this.UserCache = credentials.info;
    localStorage.setItem('token', credentials.token);
    localStorage.setItem('refreshtoken', credentials.refreshToken);
    localStorage.setItem('user', JSON.stringify(credentials.info));
    this.IsAuthenticated = true;
    this.token = credentials.token;
  }

  public static TryAuthenticate(): boolean {
    let token = localStorage.getItem('token');
    let refreshToken = localStorage.getItem('refreshtoken');
    let user = <UserInfo>JSON.parse(localStorage.getItem('user'));

    if (token == null || user == null || refreshToken == null) {
      return false;
    }

    this.UserCache = user;
    this.IsAuthenticated = true;
    this.token = token;
    return true;
  }

  public static LogOut(): void {
    localStorage.clear();
  }

}
