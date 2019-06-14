import { AuthService } from "../Auth/AuthService";
import { ConnectionManager } from "../Connections/ConnectionManager";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class DHServerKeyExchangeService {
  constructor(private auth: AuthService, private connectionManager: ConnectionManager) {
    this.connectionManager.setDHServerKeyExchangeService(this);
  }

  public InitiateKeyExchange() {

  }
}
