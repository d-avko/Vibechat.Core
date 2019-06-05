import { MatSnackBar } from "@angular/material";
import { Component } from "@angular/core";

@Component({
  selector: 'snackbar-custom'
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
