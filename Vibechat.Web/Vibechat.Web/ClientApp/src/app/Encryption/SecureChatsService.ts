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

  public GetAuthKeyId(authKey: string) {
    let raw = crypto.SHA256(authKey.substr(0, authKey.length / 2));
    return crypto.enc.Base64.stringify(raw);
  }

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
      container = new authKeys();
      container.ids = new Array<string>();
      container.keys = new Array<string>();
    }

    container.ids.push(userId);
    container.keys.push(authKey);

    localStorage.removeItem('authKeys');
    localStorage.setItem('authKeys', JSON.stringify(container));
  }

  //auth key is sha256 of first half of auth key.

  public AuthKeyExists(authKeyId: string) : boolean {
    let container = <authKeys>JSON.parse(localStorage.getItem('authKeys'));

    if (!container) {
      return false;
    }
    for (let i = 0; i < container.keys.length; ++i) {

      let raw = crypto.SHA256(container.keys[i].substr(0, container.keys[i].length / 2));
      if (crypto.enc.Base64.stringify(raw) === authKeyId) {
        return true;
      }
    }

    return false;
  }
}
