import { ConversationTemplate } from "./ConversationTemplate";
import { Cache } from "../Auth/Cache";
import { Injectable } from "@angular/core"

@Injectable({
  providedIn: 'root'
})
export class ConversationsFormatter{

  public static messageMaxLength = 22;

  public GetLastMessageFormatted(conversation: ConversationTemplate): string {

    if (conversation.messages == null || conversation.messages.length == 0) {
      return "No messages for this conversation...";
    }

    let message = conversation.messages[conversation.messages.length - 1];

    let user = message.user.id == Cache.UserCache.id ? 'You' : message.user.userName;

    let messageContent = message.messageContent.length <= ConversationsFormatter.messageMaxLength
      ? message.messageContent
      : message.messageContent.slice(0, ConversationsFormatter.messageMaxLength) + "...";

    return user + ": " + messageContent;
  }

  public GetLastMessageDateFormatted(conversation: ConversationTemplate) {

    if (conversation.messages == null || conversation.messages.length == 0) {
      return '';
    }

    let messageDate = conversation.messages[conversation.messages.length - 1].timeReceived;

    let x = new Date().getTime();

    let y = Date.parse(messageDate);

    let daysSinceReceived = (x - y) / (1000 * 60 * 60 * 24);

    switch (true) {
      case daysSinceReceived <= 1: {
        return "Today";
      }
      case daysSinceReceived == 2: {
        return "Yesterday";
      }
      case daysSinceReceived > 2: {
        return daysSinceReceived.toPrecision(1) + " days ago";
      }
    }
  }
}
