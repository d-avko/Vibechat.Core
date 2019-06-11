import { BehaviorSubject } from "rxjs";
import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class ThemesService {

  constructor() {
    let theme = localStorage.getItem('theme');

    if (theme != undefined) {
      this.changeTheme(theme);
    }

  }
  public currentThemeIndex: number = -1;
  public currentThemeName: string;

  private themes = new Array<string>(...['white', 'dark']);
  private themesClassNames = new Array<string>(...['white-theme', 'dark-theme']);

  public changeTheme(name: string) {
    if (name == this.currentThemeName) {
      return;
    }

    let index = this.themes.findIndex(x => x == name);

    if (index == -1) {
      return;
    }

    let body = document.getElementsByTagName<'body'>('body').item(0);

    if (this.currentThemeIndex != -1) {
      body.classList.remove(this.themesClassNames[this.currentThemeIndex]);
    }

    body.classList.add(this.themesClassNames[index]);
    this.currentThemeIndex = index;
    this.currentThemeName = this.themes[index];
    localStorage.removeItem('theme');
    localStorage.setItem('theme', this.currentThemeName);
  }
}
