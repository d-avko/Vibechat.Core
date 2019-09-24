import {AppUser} from "./AppUser";
import {ChatEventType} from "./ChatEventType";

export class ChatEvent{

  public constructor(init?: Partial<ChatEvent>) {
    (<any>Object).assign(this, init);
  }

  //Why or 'because of who' this event occured
  public actor: string;
  //banned, kicked, joined, invited, left
  public type: ChatEventType;

  public userInvolved: string;

  public actorName: string;

  public userInvolvedName: string;
}
