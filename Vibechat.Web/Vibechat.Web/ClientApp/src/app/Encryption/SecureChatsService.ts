import { Injectable } from "@angular/core";
import * as crypto from "crypto-js";
import {UserInfoData} from "../Dialogs/UserInfoDialog";

class key{
  id: string;
  key: string;
}

class authKeys {
  public keys: Array<key>;
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

    for(let key of container.keys){
      if(key.id == userOrGroupId){
        return key.key;
      }
    }
    return null;
  }

  public StoreAuthKey(authKey: string, userId: string) {
    let container = <authKeys>JSON.parse(localStorage.getItem('authKeys'));

    if (!container) {
      container = new authKeys();
      container.keys = new Array<key>();
    }

    this.EnsureNoDuplicateKeys(container, userId);

    let authenticationKey = new key();
    authenticationKey.id = userId;
    authenticationKey.key = authKey;

    container.keys.push(authenticationKey);

    localStorage.removeItem('authKeys');
    localStorage.setItem('authKeys', JSON.stringify(container));
  }

  public EnsureNoDuplicateKeys(container: authKeys, id: string){
    container.keys.forEach((key,index, obj) => {
      if(key.id == id){
        obj.splice(index, 1);
      }
    });
  }

  //auth key is sha256 of first half of auth key.

  public AuthKeyExists(authKeyId: string) : boolean {
    let container = <authKeys>JSON.parse(localStorage.getItem('authKeys'));

    if (!container) {
      return false;
    }

    for (let i = 0; i < container.keys.length; ++i) {

      let raw = crypto.SHA256(container.keys[i].key.substr(0, container.keys[i].key.length / 2));
      if (crypto.enc.Base64.stringify(raw) === authKeyId) {
        return true;
      }
    }

    return false;
  }
}
