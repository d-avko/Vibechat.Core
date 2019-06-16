import { AuthService } from "../Auth/AuthService";
import { ConnectionManager } from "../Connections/ConnectionManager";
import { Injectable, EventEmitter } from "@angular/core";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { ConversationsService } from "../Services/ConversationsService";
import { SecureChatsService } from "./SecureChatsService";
import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { E2EencryptionService } from "./E2EencryptionService";
import * as biginteger from "big-integer";

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
  public busy: boolean;
  public secondUserS: string; // g^a mod p
  public thisUserS: string; // g^b mod p
  public thisUserPrivateKey: biginteger.BigInteger;
}

@Injectable({
  providedIn: 'root'
})
export class DHServerKeyExchangeService {
  constructor(
    private secureChatsService: SecureChatsService,
    private enc: E2EencryptionService,
    private connectionManager: ConnectionManager,
    private api: ApiRequestsBuilder) {
    this.connectionManager.setDHServerKeyExchangeService(this);
  }

  private chatService: ConversationsService;
  private PendingParams = new Array<DictionaryEntry<string, KeyExchangeEntry>>();

  public setChatService(c: ConversationsService) {
    this.chatService = c;
  }

  public InitiateKeyExchange(conversation: ConversationTemplate) {
    let entry = this.PendingParams.find(x => x.key == conversation.dialogueUser.id);

    if (!entry) {
      entry = new DictionaryEntry<string, KeyExchangeEntry>(
        {
          key: conversation.dialogueUser.id,
          value: new KeyExchangeEntry()
        });

      this.PendingParams.push(entry);
    }

    if (entry.value.secondUserS || entry.value.busy) {
      return;
    }

    entry.value.thisUserPrivateKey = this.enc.GenerateDhPrivate();

    let toSend = this.enc.GenerateDhS(
      conversation.publicKey.modulus,
      conversation.publicKey.generator,
      entry.value.thisUserPrivateKey);

    entry.value.busy = true;
    this.connectionManager.SendDhParam(toSend, conversation.dialogueUser.id, conversation.conversationID);
  }


  public async OnIntermidiateParamsReceived(s: string, sentBy: string, chatId: number) {
    let entry = this.PendingParams.find(x => x.key == sentBy);

    if (!entry) {
      entry = new DictionaryEntry<string, KeyExchangeEntry>(
        {
          key: sentBy,
          value: new KeyExchangeEntry()
        });

      this.PendingParams.push(entry);
    }

    entry.value.secondUserS = s;

    let chat = this.chatService.Conversations.find(x => x.conversationID == chatId);
    //only creator could initiate key exchange. Here send generated param to creator.
    if (chat.creator.id == sentBy) {

      entry.value.thisUserPrivateKey = this.enc.GenerateDhPrivate();

      let toSend = this.enc.GenerateDhS(
        chat.publicKey.modulus,
        chat.publicKey.generator,
        entry.value.thisUserPrivateKey);

      this.connectionManager.SendDhParam(toSend, chat.dialogueUser.id, chat.conversationID);
    }

    let authKey = this.enc.CalculatePrivateKey(s, chat.publicKey.modulus, entry.value.thisUserPrivateKey);
    let authKeyId = this.secureChatsService.GetAuthKeyId(authKey);

    //got response, update authkeyId
    if (chat.creator.id != sentBy) {
      await this.api.UpdateAuthKeyId(authKeyId, chat.conversationID);
    }

    chat.authKeyId = authKeyId;
    this.secureChatsService.StoreAuthKey(authKey, chat.dialogueUser.id);
    entry.value.busy = false;
  }
}
