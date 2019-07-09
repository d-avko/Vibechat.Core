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

  @Output() public OnUploadFile = new EventEmitter<File>();

  @Output() public OnViewUserInfo = new EventEmitter<UserInfo>();

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

  public UploadFile(event: Event) {
    this.OnUploadFile.emit((<HTMLInputElement>event.target).files[0]);
    this.ResetInput(<HTMLInputElement>event.target);
  }

  public ViewUserInfo() {
    this.OnViewUserInfo.emit(this.User);
  }

  public ResetInput(input: HTMLInputElement) {
    input.value = '';

    if (!/safari/i.test(navigator.userAgent)) {
      input.type = '';
      input.type = 'file';
    }
  }

  public UploadImages(event: Event) {
    this.OnUploadImages.emit((<HTMLInputElement>event.target).files);
    this.ResetInput(<HTMLInputElement>event.target);
  }
}
