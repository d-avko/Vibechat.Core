import { ChatMessage } from "../Data/ChatMessage";
import * as crypto from "crypto-js";
import { Injectable } from "@angular/core";
import { SecureChatsService } from "./SecureChatsService";

class authKeys {
  public ids: Array<string>;
  public keys: Array<string>;
}

@Injectable({
  providedIn: 'root'
})
export class E2EencryptionService {

  constructor(private secureChatsService: SecureChatsService) {}

  //returns null if no key was found in local storage for provided group or user id.
  public Encrypt(userId: string, message: ChatMessage): string {
    let authKey = this.secureChatsService.GetAuthKey(userId);
    if (!authKey) {
      return null;
    }

    //simple symmetric encryption.

   return crypto.AES.encrypt(JSON.stringify(message), authKey).ciphertext;
  }

  //returns null if no auth key was found in local storage.
  public Decrypt(userId: string, encrypted: string): ChatMessage {
    let authKey = this.secureChatsService.GetAuthKey(userId);
    if (!authKey) {
      return null;
    }

    return <ChatMessage>JSON.parse(crypto.AES.decrypt(encrypted, authKey).toString());
  }
}
