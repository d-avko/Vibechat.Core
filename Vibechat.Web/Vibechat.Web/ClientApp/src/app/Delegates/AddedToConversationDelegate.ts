import { AddedToGroupModel } from "../Shared/AddedToGroupModel";

export interface AddedToConversationDelegate {
  (data: AddedToGroupModel): void;
}
