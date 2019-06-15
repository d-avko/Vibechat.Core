import { AuthService } from "../Auth/AuthService";
import { ConnectionManager } from "../Connections/ConnectionManager";
import { Injectable, EventEmitter } from "@angular/core";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ConversationsService } from "../Services/ConversationsService";
import { SecureChatsService } from "./SecureChatsService";
import * as crypto from "crypto-js";

class DictionaryEntry<K, V> {
  public constructor(init?: Partial<DictionaryEntry<K, V>>) {
    (<any>Object).assign(this, init);
  }

  public key: K;
  public value: V;
}

class KeyExchangeEntry {
  public constructor(init?: Partial<KeyExchangeEntry>) {
    (<any>Object).assign(this, init);
  }
  public secondUserS: string; // g^a mod p
  public thisUserS: string; // g^b mod p
  public thisUserPrivateKey: string;
}

@Injectable({
  providedIn: 'root'
})
export class DHServerKeyExchangeService {
  constructor(
    private chatService: ConversationsService,
    private secureChatsService: SecureChatsService,
    private connectionManager: ConnectionManager) {
    this.connectionManager.setDHServerKeyExchangeService(this);
  }

  private PendingParams = new Array<DictionaryEntry<string, KeyExchangeEntry>>();

  private OnKeysGenerated = new EventEmitter<number>();

  public InitiateKeyExchange(conversation: ConversationTemplate) {
    let entry = this.PendingParams.find(x => x.key == conversation.dialogueUser.id);

    if (!entry) {
      this.PendingParams.push(new DictionaryEntry<string, KeyExchangeEntry>(
        {
          key: conversation.dialogueUser.id,
          value: new KeyExchangeEntry()
        }));
    }

    if (entry.value.secondUserS) {
      return;
    }

    //generate private number
    //let x = conversation.publicKey.generator ^ (number) % conversation.publicKey.modulus
    //send x
  }


  public OnIntermidiateParamsReceived(s: string, sentBy: string, chatId: number) {
    let entry = this.PendingParams.find(x => x.key == sentBy);

    if (!entry) {
      this.PendingParams.push(new DictionaryEntry<string, KeyExchangeEntry>(
        {
          key: sentBy,
          value: new KeyExchangeEntry()
        }));
    }

    entry.value.secondUserS = s;

    let chat = this.chatService.Conversations.find(x => x.conversationID == chatId);

    //only creator could initiate key exchange. Here send generated param to creator.
    if (chat.creator.id == sentBy) {



    } else {
      //creator received generated param: calculate auth key and store it.
      
    }

    //if values not generated, generate and send
    //if generated, send existing
  }
}
