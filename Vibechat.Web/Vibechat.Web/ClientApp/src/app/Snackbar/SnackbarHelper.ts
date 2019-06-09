import { MatSnackBar } from "@angular/material";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class SnackBarHelper {
  durationInSeconds = 4.5;

  constructor(private snackBar: MatSnackBar) { }

  public openSnackBar(message: string, duration : number = this.durationInSeconds) {
    this.snackBar.open(message, null, {
      duration: duration * 1000,
    });
  }
}
