import { ChatRoleDto } from "../Roles/ChatRoleDto";

export class UserInfo {

  public id: string;

  public name: string;

  public lastName: string;

  public userName: string;

  public lastSeen: Date;

  public imageUrl: string;

  public fullImageUrl: string;

  public connectionId: string;

  public isOnline: boolean;

  public isMessagingRestricted: boolean;

  public isBlocked: boolean;

  public isBlockedInConversation: boolean;

  public chatRole: ChatRoleDto;
}
