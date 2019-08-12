import {ChatEvent} from "./ChatEvent";
import {AppUser} from "./AppUser";

export class BannedEvent extends ChatEvent{
  public BannedUser: AppUser;
}
