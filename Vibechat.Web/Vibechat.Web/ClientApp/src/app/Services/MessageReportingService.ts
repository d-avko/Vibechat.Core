import { SnackBarHelper } from "../Snackbar/SnackbarHelper";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class MessageReportingService {
  constructor(private snackbar: SnackBarHelper) {}

  public OnConnected(): void {
    this.snackbar.openSnackBar("Connected.", 1);
  }

  public OnDisconnected(): void {
    this.snackbar.openSnackBar("Disconnected...", 1.5);
  }

  public OnConnecting() {
    this.snackbar.openSnackBar("Connecting...", 1.5);
  }

  public OnError(error: string): void {
    this.snackbar.openSnackBar(error, 2);
  }
}
