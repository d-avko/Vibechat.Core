import { MessageReceivedModel } from "../Shared/MessageReceivedModel";

export interface MessageReceivedDelegate {
  (data: MessageReceivedModel): void;
}
