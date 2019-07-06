import { Injectable } from "@angular/core"
import { ChatMessage } from "../Data/ChatMessage";
import { ConversationTemplate } from "../Data/ConversationTemplate";
import { AuthService } from "../Auth/AuthService";
import { AttachmentKinds } from "../Data/AttachmentKinds";

@Injectable({
  providedIn: 'root'
})
export class ConversationsFormatter{

  constructor(private auth: AuthService) {}

  public static MaxSymbols = 38;

  public static MaxPixelsDesktop = 1458;

  public static MinPixelsDesktop = 880;

  public static MaxSymbolsMobile = 20;

  public GetLastMessageFormatted(conversation: ConversationTemplate): string {

    if (conversation.messages == null || conversation.messages.length == 0) {
      return "No messages...";
    }

    let MaxSymbols = 0;

    if (window.innerWidth < ConversationsFormatter.MinPixelsDesktop) {
      MaxSymbols = ConversationsFormatter.MaxSymbolsMobile;
    } else {
      MaxSymbols = Math.floor((window.innerWidth * ConversationsFormatter.MaxSymbols * 0.75) / ConversationsFormatter.MaxPixelsDesktop);
    }

    let message = conversation.messages[conversation.messages.length - 1];

    let user = message.user.id == this.auth.User.id ? 'You' : message.user.userName;

    if (message.forwardedMessage) {
      return "Forwarded message";
    }

    if (message.isAttachment) {
      let msg = user + ": " + this.GetFormattedAttachmentName(message.attachmentInfo.attachmentKind);

      if (msg.length > MaxSymbols) {
        msg = msg.slice(0, MaxSymbols) + "...";
      }

      return msg;
    }


    let msg = user + ": " + message.messageContent;

    if (msg.length > MaxSymbols) {
      msg = msg.slice(0, MaxSymbols) + "...";
    }

    return msg;
  }

  public GetBytesAmountFormatted(amount: number) {
    switch (true) {
      case amount < 1024: {
        return ` ${amount} Bytes.`
      }
      case (amount >= 1024) && (amount < 1024 * 1024): {
        return `${Math.floor(amount / 1024)} kB.`
      }
      case (amount >= 1024 * 1024) && (amount < 1024 * 1024 * 1024):{
        return `${Math.floor(amount / (1024 * 1024))} MB.`
      }
      case (amount >= 1024 * 1024 * 1024): {
        return `${Math.floor(amount / (1024 * 1024 * 1024))} GB.` 
      }
    }
  }

  public GetFormattedAttachmentName(name: string) : string {
    switch (name) {
      case AttachmentKinds.Image: {
        return "Image"
      }
      case AttachmentKinds.File: {
        return "File"
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

    return this.DaysSinceEventFormatted((<Date>message.timeReceived));
  }

  public GetMessagesDateStripFormatted(message: ChatMessage) : string {
    return this.DaysSinceEventFormatted((<Date>message.timeReceived));
  }

  public GetMessagesUnreadFormatted(amount: number): string {
    if (!amount) {
      return '';
    }

    switch (true) {
      case amount <= 1000: {
        return amount.toString();
      }
      default: {
        return Math.floor(amount / 1000).toString() + 'K';
      }
    }
  }

  private DaysSinceEventFormatted(eventDate: Date): string {
    let currentTime = new Date();
    let hoursSinceReceived = (currentTime.getTime() - eventDate.getTime()) / (1000 * 60 * 60);
    let daysSinceReceived = hoursSinceReceived / 24;
    let hoursSinceMidnight = currentTime.getHours();

    switch (true) {                 // this is for the case when user've sent message right in 00:00:00
      case hoursSinceReceived <= hoursSinceMidnight + 0.001: {
        return "Today"
      }
      case daysSinceReceived <= 2: {
        return "Yesterday";
      }
      case daysSinceReceived > 2: {
        return Math.floor(daysSinceReceived).toString() + " days ago";
      }
    }
  }

  public GetConversationMembersFormatted(conversation: ConversationTemplate) {
    let membersAmount = conversation.participants == null ? 0 : conversation.participants.length;

    return membersAmount.toString() + " Member(s)";
  }

  public GetLastSeenFormatted(dateString: string, isOnline: boolean): string {
    if (isOnline) {
      return "Online";
    }

    let date = new Date(dateString).getTime();

    let daysSinceOnline = (new Date().getTime() - date) / (1000 * 60 * 60 * 24);

    let result = "Offline. Last seen: ";

    switch (true) {
      case daysSinceOnline <= 1: {

        let hoursSinceOnline = (new Date().getTime() - date) / (1000 * 60 * 60);

        switch (true) {
          case hoursSinceOnline <= 1: {
            let minutesSinceOnline = (new Date().getTime() - date) / (1000 * 60);

            if (minutesSinceOnline <= 10) {
              return result + "less than 10 minutes ago.";
            }

            return result + Math.floor(minutesSinceOnline).toString() + " minutes ago";
          }
          default: {

            return result + Math.floor(hoursSinceOnline).toString() + " hours ago";

          }
        }

      }
      case daysSinceOnline <= 2: {
        return result + "Yesterday";
      }
      case daysSinceOnline > 2: {
        return result + Math.floor(daysSinceOnline).toString() + " days ago";
      }
    }
  }

  public GetFormattedDateForAttachments(attachments: Array<ChatMessage>) {
    let date = new Date();
    let attachmentDate = (<Date>attachments[0].timeReceived);
                                      //take zero element, as it's the most recent attachment
    let daysSinceReceived = (date.getTime() - attachmentDate.getTime()) / (1000 * 60 * 60 * 24);

    switch (true) {
      case daysSinceReceived <= 7: {
        return "This week";
      }
      case daysSinceReceived <= 14: {
        return "A week ago.";
      }
      case daysSinceReceived <= 21: {
        return "Two weeks ago.";
      }
      default: {
        return attachmentDate.toLocaleDateString();
      }
    }
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
