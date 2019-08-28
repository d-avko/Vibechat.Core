import {AppUser} from "./AppUser";
import {Message} from "./Message";
import {ChatRoleDto} from "../Roles/ChatRoleDto";

export class DhPublicKey {
  public modulus: string;
  public generator: string;
}

export class Chat {

  public id: number;

  public dialogueUser: AppUser;

  public name: string;

  public thumbnailUrl: string;

  public fullImageUrl: string;

  public isGroup: boolean;

  public messages: Array<Message>;

  public participants: Array<AppUser>;

  public isMessagingRestricted: boolean;

  public chatRole: ChatRoleDto;

  public isSecure: boolean;

  public authKeyId: string;

  public publicKey: DhPublicKey;

  public messagesUnread: number;

  public deviceId: string;

  public isPublic: boolean;

  public clientLastMessageId: number;

  public lastMessage: Message
}
