import {UserInfo} from "./UserInfo";
import {Message} from "./Message";
import {ChatRoleDto} from "../Roles/ChatRoleDto";

export class DhPublicKey {
  public modulus: string;
  public generator: string;
}

export class Chat {

  public id: number;

  public dialogueUser: UserInfo;

  public name: string;

  public thumbnailUrl: string;

  public fullImageUrl: string;

  public isGroup: boolean;

  public messages: Array<Message>;

  public participants: Array<UserInfo>;

  public isMessagingRestricted: boolean;

  public chatRole: ChatRoleDto;

  public isSecure: boolean;

  public authKeyId: string;

  public publicKey: DhPublicKey;

  public messagesUnread: number;

  public deviceId: string;

  public canReceiveMessages: boolean;

  public clientLastMessageId: number;

  public lastMessage: Message
}
