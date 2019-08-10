import {BehaviorSubject} from "rxjs";

export enum MessageViewOption {
  NoOption,
  ViewMessage,
  GotoMessage
}

export class MessageViewOptions {
  constructor(){
    this.Option = new BehaviorSubject<MessageViewOption>(MessageViewOption.NoOption);
  }
  public Option: BehaviorSubject<MessageViewOption>;
  public MessageToViewId: number = -1;
}
