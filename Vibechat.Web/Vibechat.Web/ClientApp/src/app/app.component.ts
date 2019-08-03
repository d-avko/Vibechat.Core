import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LoadingScreenService } from './Loading/LoadingScreenService';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  animations: []
})
export class AppComponent {
  constructor(public loading: LoadingScreenService) {
    this.loading.startLoading();
  }
  prepareRoute(outlet: RouterOutlet) {
    return outlet && outlet.activatedRouteData && outlet.activatedRouteData['animation'];
  }
}
