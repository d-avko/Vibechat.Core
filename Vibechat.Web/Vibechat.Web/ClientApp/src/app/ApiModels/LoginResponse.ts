import { AppUser } from "../Data/AppUser";

export class LoginResponse {
  token: string;
  refreshToken: string;
  info: AppUser;
  isNewUser: boolean;
}
