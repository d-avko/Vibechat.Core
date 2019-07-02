import { UserInfo } from "../Data/UserInfo";

export class LoginResponse {
  token: string;
  refreshToken: string;
  info: UserInfo;
  isNewUser: boolean;
}
