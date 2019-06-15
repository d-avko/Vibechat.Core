import { Injectable } from "@angular/core";
import * as crypto from "crypto-js";

class authKeys {
  public ids: Array<string>;
  public keys: Array<string>;
}
@Injectable({
  providedIn: 'root'
})
export class SecureChatsService {

  public GetAuthKey(userOrGroupId: string) {
    let container = <authKeys>JSON.parse(localStorage.getItem('authKeys'));

    if (!container) {
      return null;
    }

    for (let i = 0; i < container.ids.length; ++i) {

      if (container.ids[i] === userOrGroupId) {
        return container.keys[i];
      }
     
    }

    return null;
  }

  public StoreAuthKey(authKey: string, userId: string) {
    let container = <authKeys>JSON.parse(localStorage.getItem('authKeys'));

    if (!container) {
      return;
    }

    container.ids.push(userId);
    container.keys.push(authKey);

    localStorage.setItem(JSON.stringify(container), 'authKeys');
  }

  //auth key is sha256 of first 1024 bits of auth key.

  public AuthKeyExists(authKeyId: string) : boolean {
    let container = <authKeys>JSON.parse(localStorage.getItem('authKeys'));

    if (!container) {
      return null;
    }
    for (let i = 0; i < container.keys.length; ++i) {

      if (crypto.SHA256(container.keys[i].substr(0, container.keys[i].length / 2)).ciphertext === authKeyId) {
        return true;
      }
    }

    return false;
  }
}
