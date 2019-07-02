import { Component, Input, Output, EventEmitter, ViewChild } from "@angular/core";
import { ConversationTemplate } from "../../Data/ConversationTemplate";
import { MatFormField } from "@angular/material";
import { UserInfo } from "../../Data/UserInfo";

@Component({
  selector: 'input-view',
  templateUrl: './input.component.html',
  styleUrls: ['./input.component.css']
})
export class InputComponent {

  @Output() public OnSendMessage = new EventEmitter<string>();

  @Output() public OnUploadImages = new EventEmitter<FileList>();

  @Input() public User: UserInfo;

  @ViewChild(MatFormField) inputfield: MatFormField;

  constructor() {

  }

  @Input() public Conversation: ConversationTemplate;

  public SendMessage() {

    if (this.inputfield._control.value == null || this.inputfield._control.value == '') {
      return;
    }

    this.OnSendMessage.emit(this.inputfield._control.value);

    this.inputfield._control.value = '';
  }

  public UploadImages(event: any) {
    this.OnUploadImages.emit(event.target.files);
    event.target.files = null;
  }
}
