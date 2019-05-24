import { Cache } from "../Auth/Cache";
import { Injectable } from "@angular/core"
import { ChatMessage } from "../Data/ChatMessage";
import { ConversationTemplate } from "../Data/ConversationTemplate";

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

    if (message.isAttachment) {
      return user + ": " + this.GetFormattedAttachmentName(message.attachmentInfo.attachmentKind);
    }

    let messageContent = '';

    if (message.messageContent.length <= ConversationsFormatter.messageMaxLength) {
      messageContent = message.messageContent;
    }
    else {
      messageContent = message.messageContent.slice(0, ConversationsFormatter.messageMaxLength) + "...";
    }

    return user + ": " + messageContent;
  }

  public GetFormattedAttachmentName(name: string) : string {
    switch (name) {
      case "img": {
        return "Image"
      }
      default: {
        return "Unknown attachment"
      }
    }
  }

  public GetLastMessageDateFormatted(conversation: ConversationTemplate) {

    if (conversation.messages == null || conversation.messages.length == 0) {
      return '';
    }

    let message = conversation.messages[conversation.messages.length - 1];

    let daysSinceReceived = (new Date().getTime() - (<Date>message.timeReceived).getTime()) / (1000 * 60 * 60 * 24);

    switch (true) {
      case daysSinceReceived <= 1: {
        return "Today"
      }
      case daysSinceReceived <= 2: {
        return "Yesterday";
      }
      case daysSinceReceived > 2: {
        return daysSinceReceived.toPrecision(1) + " days ago";
      }
    }
  }

  public GetMessagesDateStripFormatted(message: ChatMessage) {

    let daysSinceReceived = (new Date().getTime() - (<Date>message.timeReceived).getTime()) / (1000 * 60 * 60 * 24);

    switch (true) {
      case daysSinceReceived <= 1: {
        return "Today";
      }
      case daysSinceReceived <= 2: {
        return "Yesterday";
      }
      case daysSinceReceived > 2: {
        return daysSinceReceived.toPrecision(1) + " days ago";
      }
    }
  }

  public GetConversationMembersFormatted(conversation: ConversationTemplate) {
    let membersAmount = conversation.participants == null ? 0 : conversation.participants.length;

    return membersAmount.toString() + " Member(s)";
  }

  public GetConversationNameFormatted(conversation: ConversationTemplate) {

    if (!conversation.isGroup) {
      return conversation.dialogueUser.userName;
    }

    return conversation.name;
  }

  public GetMessageTimeFormatted(message: ChatMessage) {
    return (<Date>message.timeReceived).toLocaleTimeString();
  }
}
