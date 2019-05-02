import { MatSnackBar } from "@angular/material";
import { Component } from "@angular/core";

/**
 * @title Snack-bar with a custom component
 */
@Component({
  selector: 'snack-bar'
})
export class SnackBarHelper {
  durationInSeconds = 2.5;

  constructor(private snackBar: MatSnackBar) { }

  public openSnackBar(message: string) {
    this.snackBar.open(message, null, {
      duration: this.durationInSeconds * 1000,
    });
  }
}
