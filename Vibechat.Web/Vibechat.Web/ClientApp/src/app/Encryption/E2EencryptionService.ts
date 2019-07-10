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

  public static DhMinKeyLength: number = 512;

  public static DhMaxKeyLength: number = 1024;

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
    return <ChatMessage>JSON.parse(decrypted.toString(crypto.enc.Utf8));
  }

  public GenerateDhPrivate(): biginteger.BigInteger {
    let min = biginteger(2).pow(E2EencryptionService.DhMinKeyLength);
    let max = biginteger(2).pow(E2EencryptionService.DhMaxKeyLength);

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
