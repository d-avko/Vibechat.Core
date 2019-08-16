import {AfterViewInit, Component} from '@angular/core';

export enum Translation{
  LastMsgNoMessages,
  MessageSenderYou,
  LastMsgForwarded,
  ChatEventJoined,
  ChatEventBanned,
  ChatEventKicked,
  ChatEventInvited,
  ChatEventLeft,
  ChatEventUser,
  ChatRoleCreator,
  ChatRoleModerator,
  UserTyping,
  And,
  ManyPeopleTyping,
  AttachmentImage,
  AttachmentFile,
  AttachmentUnknown,
  Today,
  Yesterday,
  ManyDaysAgo,
  ChatMembers,
  Online,
  Offline,
  LastSeenLessThan5,
  LastSeenHours,
  LastSeenMinutes,
  LastSeenDays,
  ThisWeek,
  WeekAgo,
  TwoWeeksAgo,
  Connected,
  Disconnected,
  Reconnecting,
  Connecting,
  OnActionWhileDisconnected,
  WaitingForUser,
  ReEnterChat,
  WrongSms,
  CouldntSendSms,
  UsersSearchNoResults,
  FilesTooBig,
  DialogFailedToCreate,
  ServerReturnedCode,
  WithError,
  NoInternet,
  ApiError,
  WrongMessagesToForward,
  SomeFilesWereNotUploaded,
  CouldntViewUserProfile,
  WrongUserDataLength,
  EnterAtLeast,
  SymbolsToEnter,
  FileFailedToUpload
}

@Component({
  selector: 'app-translate',
  templateUrl: './translate.component.html'
})
export class TranslateComponent implements AfterViewInit {
  constructor() {

  }



  private _translations: HTMLCollection;

  private set _Translations(value: HTMLCollection){
    if(!this._translations || !this._translations.length){
      this._translations = value;
    }
  }

  private static translationsStatic: HTMLCollection;

  private static set TranslationsStatic(value: HTMLCollection){
    if(!this.translationsStatic || !this.translationsStatic.length){
      this.translationsStatic = value;
    }
  }

  public static GetTranslationStatic(type: Translation){
    this.TranslationsStatic = window.document.getElementById('translations-list').children;

    for(let i = 0; i < this.translationsStatic.length; ++i){
      if(this.translationsStatic[i].id == Translation[type]){
        return (<HTMLParagraphElement>this.translationsStatic[i]).innerText;
      }
    }

    return '';
  }

  ngAfterViewInit(): void {
    this._Translations = window.document.getElementById('translations-list').children;
    TranslateComponent.TranslationsStatic = window.document.getElementById('translations-list').children;
  }

  ngOnInit() {
  }

  public GetTranslation(type: Translation){
    this._Translations = window.document.getElementById('translations-list').children;

    for(let i = 0; i < this._translations.length; ++i){
      if(this._translations[i].id == Translation[type]){
        return (<HTMLParagraphElement>this._translations[i]).innerText;
      }
    }

    return '';
  }

}
