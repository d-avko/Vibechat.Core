import { ChatMessage } from "../Data/ChatMessage";
import * as crypto from "crypto-js";
import { Injectable } from "@angular/core";
import { SecureChatsService } from "./SecureChatsService";
import * as biginteger from "big-integer";

class authKeys {
  public ids: Array<string>;
  public keys: Array<string>;
}

@Injectable({
  providedIn: 'root'
})
export class E2EencryptionService {

  constructor(private secureChatsService: SecureChatsService) {}

  public DhMinKeyLength: number = 100;

  public DhMaxKeyLength: number = 320;

  public static keySize = 256;

  public static ivSize = 128;

  public static iterations = 100;

  public Encrypt(userId: string, message: ChatMessage) {
    let authKey = this.secureChatsService.GetAuthKey(userId);
    if (!authKey) {
      return null;
    }

    let salt = crypto.lib.WordArray.random(128 / 8);

    let key = crypto.PBKDF2(authKey, salt, {
      keySize: E2EencryptionService.keySize / 32,
      iterations: E2EencryptionService.iterations
    });

    let iv = crypto.lib.WordArray.random(128 / 8);

    let encrypted = crypto.AES.encrypt(JSON.stringify(message), key, {
      iv: iv,
      padding: crypto.pad.Pkcs7,
      mode: crypto.mode.CBC
    });

    // salt, iv will be hex 32 in length
    // append them to the ciphertext for use  in decryption
    let transitmessage = salt.toString() + iv.toString() + encrypted.toString();
    return transitmessage;
  }

  public Decrypt(userId: string, transitmessage: string) {
    let authKey = this.secureChatsService.GetAuthKey(userId);
    if (!authKey) {
      return null;
    }

    let salt = crypto.enc.Hex.parse(transitmessage.substr(0, 32));
    let iv = crypto.enc.Hex.parse(transitmessage.substr(32, 32))
    let encrypted = transitmessage.substring(64);

    let key = crypto.PBKDF2(authKey, salt, {
      keySize: E2EencryptionService.keySize / 32,
      iterations: E2EencryptionService.iterations
    });

    let decrypted = crypto.AES.decrypt(encrypted, key, {
      iv: iv,
      padding: crypto.pad.Pkcs7,
      mode: crypto.mode.CBC

    });
    let x = decrypted.toString(crypto.enc.Utf8);
    return <ChatMessage>JSON.parse(decrypted.toString(crypto.enc.Utf8));
  }

  ////returns null if no key was found in local storage for provided group or user id.
  //public Encrypt(userId: string, message: ChatMessage): string {
  //  let authKey = this.secureChatsService.GetAuthKey(userId);
  //  if (!authKey) {
  //    return null;
  //  }

  //  let secretKey = crypto.SHA256(authKey);
  //  let encrypted = crypto.AES.encrypt(JSON.stringify(message), secretKey.words, { mode: crypto.mode.CBC });
  //  return crypto.enc.Base64.stringify(encrypted.ciphertext);
  //}

  ////returns null if no auth key was found in local storage.
  //public Decrypt(userId: string, encrypted: string): ChatMessage {
  //  let authKey = this.secureChatsService.GetAuthKey(userId);
  //  if (!authKey) {
  //    return null;
  //  }

  //  let secretKey = crypto.SHA256(authKey);
  //  let parsed = crypto.enc.Base64.parse(encrypted);
  //  let decrypted = crypto.AES.decrypt(parsed.words, secretKey.words, { mode: crypto.mode.CBC }).toString();
  //  return <ChatMessage>JSON.parse(decrypted);
  //}

  public GenerateDhPrivate(): biginteger.BigInteger {
    let min = biginteger(2).pow(100);
    let max = biginteger(2).pow(320);

    return biginteger.randBetween(min, max);
  }

  public GenerateDhS(mod: string, g: string, privateKey: biginteger.BigInteger): string {
    let modulus = biginteger(mod);
    let generator = biginteger(g);

    return generator.modPow(privateKey, modulus).toString();
  }

  public CalculatePrivateKey(s: string, modulus: string, privateKey: biginteger.BigInteger): string {
    return biginteger(s).modPow(privateKey, biginteger(modulus)).toString();
  }
}
