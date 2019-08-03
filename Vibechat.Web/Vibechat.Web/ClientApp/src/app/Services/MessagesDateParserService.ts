//import { ChatMessage } from "../Data/ChatMessage";
//import { Injectable } from "@angular/core";

//@Injectable({
//  providedIn: 'root'
//})
//export class MessagesDateParserService {
//  public ParseStringDatesInMessages(messages: Array<ChatMessage>) :  void {
//    messages.forEach((msg) => {

//      msg.timeReceived = new Date(msg.timeReceived);

//      if (msg.forwardedMessage != null) {
//        msg.forwardedMessage.timeReceived = new Date(msg.forwardedMessage.timeReceived)
//      }
//    });
//  }

//  public ParseStringDateInMessage(message: ChatMessage) : void {
//    message.timeReceived = new Date(message.timeReceived);

//    if (message.forwardedMessage != null) {
//      message.forwardedMessage.timeReceived = new Date(message.forwardedMessage.timeReceived)
//    }
//  }
//}
