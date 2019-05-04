import { UserInfo } from "./UserInfo";

export class ChatMessage {

    public user: UserInfo;
    
    public messageContent: string;
    
    public conversationID: number;
    
    public timeReceived: Date;
}
