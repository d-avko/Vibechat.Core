import {AfterContentChecked, AfterContentInit, Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {ChatEvent} from "../Data/ChatEvent";
import {ChatEventType} from "../Data/ChatEventType";

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
    if(!this.event){
      return;
    }

    switch (this.event.type) {
      case ChatEventType.Joined: {
        this.firstUsername = this.event.actorName;
        this.firstUserid = this.event.actor;
        this.action = 'joined the chat.';
        return;
      }
      case ChatEventType.Banned:{
        this.firstUsername = this.event.userInvolvedName;
        this.firstUserid = this.event.userInvolved;
        this.action = ' was banned by ';
        this.secondUsername = this.event.actorName;
        this.secondUserId = this.event.actor;
        return;
      }
      case ChatEventType.Invited:{
        this.firstUsername = this.event.userInvolvedName;
        this.firstUserid = this.event.userInvolved;
        this.action = ' was invited by ';
        this.secondUsername = this.event.actorName;
        this.secondUserId = this.event.actor;
        return;
      }
      case ChatEventType.Left:{
        this.firstUsername = this.event.actorName;
        this.firstUserid = this.event.actor;
        this.action = 'has left the chat.';
        return;
      }
      case ChatEventType.Kicked:{
        this.firstUsername = this.event.userInvolvedName;
        this.firstUserid = this.event.userInvolved;
        this.action = ' was kicked by ';
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
