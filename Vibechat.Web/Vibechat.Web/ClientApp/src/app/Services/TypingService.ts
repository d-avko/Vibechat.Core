import { Injectable } from "@angular/core";

export class TypingUserEntry {
  public constructor(init?: Partial<TypingUserEntry>) {
    (<any>Object).assign(this, init);
  }
  id: string;
  firstName: string;
}

export class DictionaryEntry<K, V> {
  public constructor(init?: Partial<DictionaryEntry<K, V>>) {
    (<any>Object).assign(this, init);
  }

  public key: K;
  public value: V;
}

@Injectable()
export class TypingService {

  public static TypingDelay: number = 4500;

  constructor() {
    this.TypingUsers = new Array<DictionaryEntry<number, Array<TypingUserEntry>>>();
  }

  public TypingUsers: Array<DictionaryEntry<number, Array<TypingUserEntry>>>;

  public OnTyping(userFirstName: string, userId: string, chatId: number) {

    let users = this.TypingUsers.find(x => x.key == chatId);

    if (!users) {
      users = new DictionaryEntry<number, Array<TypingUserEntry>>({ key: chatId });
      this.TypingUsers.push(users);
    }

    if (!users.value) {
      users.value = new Array<TypingUserEntry>();
    }

    let userIndex = users.value.findIndex(x => x.id == userId);

    if (userIndex != -1) {
      //user was already typing, remove him from typing list and re-add
      users.value.splice(userIndex, 1);
    }

    users.value.push(new TypingUserEntry({ firstName: userFirstName, id: userId }));
    setTimeout(() => this.RemoveUserFromTypingList(userId, chatId), TypingService.TypingDelay);
  }

  public GetUsersTyping(chatId: number) {
    let chat = this.TypingUsers.find(x => x.key == chatId);

    if (!chat) {
      return null;
    }

    return chat.value;
  }

  private RemoveUserFromTypingList(userId: string, chatId: number) {
    let users = this.TypingUsers.find(x => x.key == chatId);
    let userIndex = users.value.findIndex(x => x.id == userId);

    if (userIndex === -1) {
      return;
    }

    users.value.splice(userIndex, 1);
  }
}
