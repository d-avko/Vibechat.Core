import {AfterContentChecked, AfterContentInit, Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {ChatEvent} from "../../../Data/ChatEvent";
import {ChatEventType} from "../../../Data/ChatEventType";
import {TranslateComponent, Translation} from "../../../translate/translate.component";

@Component({
  selector: 'chat-event',
  templateUrl: './chat-event.component.html',
  styleUrls: ['./chat-event.component.css']
})
export class ChatEventComponent implements AfterContentInit, AfterContentChecked, OnInit {
  @Input() event: ChatEvent;
  @Output() OnViewUserInfo = new EventEmitter<string>();

  firstUsername: string;
  secondUsername: string;
  firstUserid: string;
  secondUserId: string;
  action: string;

  ngAfterContentInit(): void {
    console.log(JSON.stringify(this.event));

    if(!this.event){
      return;
    }
    switch (this.event.type) {
      case ChatEventType.Joined: {
        this.firstUsername = this.event.actorName;
        this.firstUserid = this.event.actor;
        this.action =  TranslateComponent.GetTranslationStatic(Translation.ChatEventJoined);
        return;
      }
      case ChatEventType.Banned:{
        this.firstUsername = this.event.userInvolvedName;
        this.firstUserid = this.event.userInvolved;
        this.action = ` ${TranslateComponent.GetTranslationStatic(Translation.ChatEventBanned)} `;
        this.secondUsername = this.event.actorName;
        this.secondUserId = this.event.actor;
        return;
      }
      case ChatEventType.Invited:{
        this.firstUsername = this.event.userInvolvedName;
        this.firstUserid = this.event.userInvolved;
        this.action = ` ${TranslateComponent.GetTranslationStatic(Translation.ChatEventInvited)} `;
        this.secondUsername = this.event.actorName;
        this.secondUserId = this.event.actor;
        return;
      }
      case ChatEventType.Left:{
        this.firstUsername = this.event.actorName;
        this.firstUserid = this.event.actor;
        this.action = ` ${TranslateComponent.GetTranslationStatic(Translation.ChatEventLeft)}`;
        return;
      }
      case ChatEventType.Kicked:{
        this.firstUsername = this.event.userInvolvedName;
        this.firstUserid = this.event.userInvolved;
        this.action = ` ${TranslateComponent.GetTranslationStatic(Translation.ChatEventKicked)} `;
        this.secondUsername = this.event.actorName;
        this.secondUserId = this.event.actor;
        return;
      }
    }
  }

  ngOnInit() {
  }

  ngAfterContentChecked(): void {
  }

  ViewUser(id: string){
    this.OnViewUserInfo.emit(id);
  }

}
