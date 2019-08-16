import {Component, ViewChild} from "@angular/core"
import {Message} from "../Data/Message";
import {Chat} from "../Data/Chat";
import {AuthService} from "../Services/AuthService";
import {AttachmentKind} from "../Data/AttachmentKinds";
import {TypingService} from "../Services/TypingService";
import {AppUser} from "../Data/AppUser";
import {ChatRole} from "../Roles/ChatRole";
import {MessageType} from "../Data/MessageType";
import {ChatEvent} from "../Data/ChatEvent";
import {ChatEventType} from "../Data/ChatEventType";
import {TranslateComponent, Translation} from "../translate/translate.component";

@Component({
  selector: 'formatter',
  template: '' +
    '<div style="display: none">' +
    '<app-translate></app-translate>'+
    '</div>'
})
export class ConversationsFormatter {

  @ViewChild(TranslateComponent, {static: true}) public translations;

  constructor(private auth: AuthService,
              private typing: TypingService) {
  }

  public static MaxSymbols = 38;

  public static MaxPixelsDesktop = 1458;

  public static MinPixelsDesktop = 880;

  public static MaxSymbolsMobile = 20;

  public static MaxSymbolsForNameInTypingStripe = 20;

  public _IsMobileDevice(){
    return window.innerWidth < ConversationsFormatter.MinPixelsDesktop;
  }

  public static IsMobileDevice() {
    return window.innerWidth < ConversationsFormatter.MinPixelsDesktop;
  }
  public GetLastMessageFormatted(message: Message): string {

    if (!message) {
      return this.translations.GetTranslation(Translation.LastMsgNoMessages);
    }

    let MaxSymbols = 0;

    if (ConversationsFormatter.IsMobileDevice()) {
      MaxSymbols = ConversationsFormatter.MaxSymbolsMobile;
    } else {
      MaxSymbols = Math.floor((window.innerWidth * ConversationsFormatter.MaxSymbols * 0.75)
        / ConversationsFormatter.MaxPixelsDesktop);
    }

    let user;
    if(!message.user){
      user = '';
    }else{
      user = message.user.id == this.auth.User.id ? this.translations.GetTranslation(Translation.MessageSenderYou) : message.user.userName;
    }

    switch (message.type) {
      case MessageType.Attachment: {
        let msg = user + ": " + this.GetFormattedAttachmentName(message.attachmentInfo.attachmentKind);

        if (msg.length > MaxSymbols) {
          msg = msg.slice(0, MaxSymbols) + "...";
        }

        return msg;
      }
      case MessageType.Forwarded: {
        return this.translations.GetTranslation(Translation.LastMsgForwarded);
      }
      case MessageType.Event: {
        let msg = this.GetChatEventFormatted(message.event);
        if (msg.length > MaxSymbols) {
          msg = msg.slice(0, MaxSymbols) + "...";
        }

        return msg;
      }
      case MessageType.Text: {
        let msg = user + ": " + message.messageContent;

        if (msg.length > MaxSymbols) {
          msg = msg.slice(0, MaxSymbols) + "...";
        }

        return msg;
      }
    }
  }

  public GetChatEventFormatted(event: ChatEvent): string {
    let result = this.translations.GetTranslation(Translation.ChatEventUser);
    switch (event.type) {
      case ChatEventType.Joined: {
        return result + ` ${event.actorName} ${this.translations.GetTranslation(Translation.ChatEventJoined)}.`;
      }
      case ChatEventType.Banned:{
        return result + ` ${event.userInvolvedName} ${this.translations.GetTranslation(Translation.ChatEventBanned)} ${event.actorName}`;
      }
      case ChatEventType.Invited:{
          return result + ` ${event.userInvolvedName} ${this.translations.GetTranslation(Translation.ChatEventInvited)} ${event.actorName}`;
      }
      case ChatEventType.Left:{
        return result + `${event.actorName} ${this.translations.GetTranslation(Translation.ChatEventLeft)}`;
      }
      case ChatEventType.Kicked:{
        return result + ` ${event.userInvolvedName} ${this.translations.GetTranslation(Translation.ChatEventKicked)} ${event.actorName}`;
      }
    }
  }

  public GetUserRoleFormatted(user: AppUser) {
    switch (user.chatRole.role) {
      case ChatRole.Creator: {
        return this.translations.GetTranslation(Translation.ChatRoleCreator);
      }
      case ChatRole.Moderator: {
        return this.translations.GetTranslation(Translation.ChatRoleModerator);
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
      return firstUserName + " "+ this.translations.GetTranslation(Translation.UserTyping);
    } else {
      return `${firstUserName} ${this.translations.GetTranslation(Translation.And)} 
      ${users.length - 1} ${this.translations.GetTranslation(Translation.ManyPeopleTyping)}`;
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
      case (amount >= 1024 * 1024) && (amount < 1024 * 1024 * 1024): {
        return `${Math.floor(amount / (1024 * 1024))} MB.`
      }
      case (amount >= 1024 * 1024 * 1024): {
        return`${Math.floor(amount / (1024 * 1024 * 1024))} GB.`
      }
    }
  }

  public GetFormattedAttachmentName(kind: AttachmentKind): string {
    switch (kind) {
      case AttachmentKind.Image: {
        return this.translations.GetTranslation(Translation.AttachmentImage);
      }
      case AttachmentKind.File: {
        return this.translations.GetTranslation(Translation.AttachmentFile);
      }
      default: {
        return this.translations.GetTranslation(Translation.AttachmentUnknown);
      }
    }
  }

  public GetLastMessageDateFormatted(conversation: Chat) {

    if (!conversation.lastMessage) {
      return '';
    }

    if (typeof conversation.lastMessage.timeReceived !== 'object') {
      conversation.lastMessage.timeReceived = new Date(<string>conversation.lastMessage.timeReceived);
    }

    return this.DaysSinceEventFormatted((<Date>conversation.lastMessage.timeReceived));
  }

  public GetMessagesDateStripFormatted(message: Message): string {
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

    switch (true) {    // this is for the case when user've sent message right in 00:00:00
      case daysSinceReceived <= ConversationsFormatter.MAX_FORMATTABLE_DAYS: {
        let currentDay = currentTime.getDay();
        let eventDay = eventDate.getDay();
        let realDaysBetween: number;

        if (currentDay < eventDay) {
          realDaysBetween = currentDay + Math.abs(eventDay - ConversationsFormatter.DAYS_IN_A_WEEK);
        } else {
          realDaysBetween = currentDay - eventDay;
        }

        switch (realDaysBetween) {
          case 0: {
            return this.translations.GetTranslation(Translation.Today);
          }
          case 1: {
            return this.translations.GetTranslation(Translation.Yesterday);
          }
          default: {
            return realDaysBetween.toString() + " " + this.translations.GetTranslation(Translation.ManyDaysAgo);
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

    return membersAmount.toString() + " " + this.translations.GetTranslation(Translation.ChatMembers);
  }

  public GetLastSeenFormatted(dateString: string, isOnline: boolean): string {
    if (isOnline) {
      return this.translations.GetTranslation(Translation.Online);
    }

    let date = new Date(dateString).getTime();

    let daysSinceOnline = (new Date().getTime() - date) / (1000 * 60 * 60 * 24);

    let result = this.translations.GetTranslation(Translation.Offline) + " ";

    switch (true) {
      case daysSinceOnline <= 1: {

        let hoursSinceOnline = (new Date().getTime() - date) / (1000 * 60 * 60);

        switch (true) {
          case hoursSinceOnline <= 1: {
            let minutesSinceOnline = (new Date().getTime() - date) / (1000 * 60);

            if (minutesSinceOnline <= 5) {
              return result + this.translations.GetTranslation(Translation.LastSeenLessThan5);
            }

            return result + Math.floor(minutesSinceOnline).toString() + " " +
              this.translations.GetTranslation(Translation.LastSeenMinutes);
          }
          default: {

            return result + Math.floor(hoursSinceOnline).toString() + " " +
              this.translations.GetTranslation(Translation.LastSeenHours);

          }
        }

      }
      case daysSinceOnline <= 2: {
        return result + this.translations.GetTranslation(Translation.Yesterday);
      }
      case daysSinceOnline > 2: {
        return result + Math.floor(daysSinceOnline).toString() + " " +
          this.translations.GetTranslation(Translation.LastSeenDays);
      }
    }
  }

  public GetFormattedDateForAttachments(attachments: Array<Message>) {
    let date = new Date();

    if (!<Date>attachments[0].timeReceived) {
      attachments[0].timeReceived = new Date(<string>attachments[0].timeReceived);
    }

    let attachmentDate = <Date>attachments[0].timeReceived;
    //take zero element, as it's the most recent attachment
    let daysSinceReceived = (date.getTime() - attachmentDate.getTime()) / (1000 * 60 * 60 * 24);

    switch (true) {
      case daysSinceReceived <= 7: {
        return this.translations.GetTranslation(Translation.ThisWeek);
      }
      case daysSinceReceived <= 14: {
        return this.translations.GetTranslation(Translation.WeekAgo);
      }
      case daysSinceReceived <= 21: {
        return this.translations.GetTranslation(Translation.TwoWeeksAgo);
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
