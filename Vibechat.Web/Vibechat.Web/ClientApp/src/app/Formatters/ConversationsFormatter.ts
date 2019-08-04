import {Injectable} from "@angular/core"
import {ChatMessage} from "../Data/ChatMessage";
import {Chat} from "../Data/Chat";
import {AuthService} from "../Auth/AuthService";
import {AttachmentKind} from "../Data/AttachmentKinds";
import {TypingService} from "../Services/TypingService";
import {UserInfo} from "../Data/UserInfo";
import {ChatRole} from "../Roles/ChatRole";

@Injectable({
  providedIn: 'root'
})
export class ConversationsFormatter{

  constructor(private auth: AuthService, private typing: TypingService) {}

  public static MaxSymbols = 38;

  public static MaxPixelsDesktop = 1458;

  public static MinPixelsDesktop = 880;

  public static MaxSymbolsMobile = 20;

  public static MaxSymbolsForNameInTypingStripe = 20;

  public IsMobileDevice() {
    return window.innerWidth < ConversationsFormatter.MinPixelsDesktop;
  }

  public GetLastMessageFormatted(conversation: Chat): string {

    if (conversation.messages == null || conversation.messages.length == 0) {
      return "No messages...";
    }

    let MaxSymbols = 0;

    if (this.IsMobileDevice()) {
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

  public GetUserRoleFormatted(user: UserInfo) {
    switch (user.chatRole.role) {
      case ChatRole.Creator: {
        return "Creator";
      }
      case ChatRole.Moderator: {
        return "Moderator";
      }
      default: {
        return "";
      }
    }
  }

  public GetUsersTypingFormatted(chatId: number) {
    let users = this.typing.GetUsersTyping(chatId);

    if (!users || !users.length) {
      return "";
    }

    let firstUserName = users[0].firstName.substr(0, ConversationsFormatter.MaxSymbolsForNameInTypingStripe);

    if (users.length === 1) {
      return firstUserName + " is typing";
    } else {
      return `${firstUserName} and ${users.length - 1} more people are typing`;
    }
  }

  public IsAnyUserTyping(chatId: number): boolean {
    let users = this.typing.GetUsersTyping(chatId);
    return users != null && users.length != 0;
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

  public GetFormattedAttachmentName(kind: AttachmentKind) : string {
    switch (kind) {
      case AttachmentKind.Image: {
        return "Image"
      }
      case AttachmentKind.File: {
        return "File"
      }
      default: {
        return "Unknown attachment"
      }
    }
  }

  public GetLastMessageDateFormatted(conversation: Chat) {

    if (conversation.messages == null || conversation.messages.length == 0) {
      return '';
    }

    let message = conversation.messages[conversation.messages.length - 1];

    if (typeof message.timeReceived !== 'object') {
      message.timeReceived = new Date(<string>message.timeReceived);
    }

    return this.DaysSinceEventFormatted((<Date>message.timeReceived));
  }

  public GetMessagesDateStripFormatted(message: ChatMessage): string {
    if (typeof message.timeReceived !== 'object') {
      message.timeReceived = new Date(<string>message.timeReceived);
    }

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

  public static DAYS_IN_A_WEEK = 7;

  public static MAX_FORMATTABLE_DAYS = 5;

  private DaysSinceEventFormatted(eventDate: Date): string {
    let currentTime = new Date();
    let hoursSinceReceived = Math.floor((currentTime.getTime() - eventDate.getTime()) / (1000 * 60 * 60));
    let daysSinceReceived = Math.floor(hoursSinceReceived / 24);
    let hoursSinceMidnight = currentTime.getHours();

    switch (true) {    // this is for the case when user've sent message right in 00:00:00
      case hoursSinceReceived <= hoursSinceMidnight + 0.001: {
        return "Today"
      }
      case daysSinceReceived <= ConversationsFormatter.MAX_FORMATTABLE_DAYS: {
        let currentDay = currentTime.getDay();
        let eventDay = eventDate.getDay();
        let realDaysBetween : number;

        if (currentDay < eventDay) {
          realDaysBetween = currentDay + Math.abs(eventDay - ConversationsFormatter.DAYS_IN_A_WEEK);
        } else {
          realDaysBetween = currentDay - eventDay;
        }

        switch (realDaysBetween) {
          case 1: {
            return "Yesterday";
          }
          default: {
            return realDaysBetween.toString() + " days ago";
          }
        }
      }
      default: {
        return eventDate.toLocaleDateString();
      }
    }
  }

  public GetConversationMembersFormatted(conversation: Chat) {
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

    if (!<Date>attachments[0].timeReceived) {
      attachments[0].timeReceived = new Date(<string>attachments[0].timeReceived);
    }

    let attachmentDate = <Date>attachments[0].timeReceived;
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

  public GetConversationNameFormatted(conversation: Chat) {

    if (!conversation.isGroup) {
      return conversation.dialogueUser.userName;
    }

    return conversation.name;
  }
}
