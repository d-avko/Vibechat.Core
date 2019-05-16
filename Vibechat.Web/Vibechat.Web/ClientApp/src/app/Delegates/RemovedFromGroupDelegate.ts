import { RemovedFromGroupModel } from "../Shared/RemovedFromGroupModel";

export interface RemovedFromGroupDelegate{
(data: RemovedFromGroupModel) : void;
}