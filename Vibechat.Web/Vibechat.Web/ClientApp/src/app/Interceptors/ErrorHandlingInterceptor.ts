//import {
//  HttpInterceptor,
//  HttpRequest,
//  HttpHandler,
//  HttpEvent,
//  HttpErrorResponse
//} from '@angular/common/http';
//import 'rxjs/add/operator/do';
//import { Observable } from 'rxjs';
//import { Injectable } from '@angular/core';
//import { Router } from '@angular/router';
//import { Cache } from '../Auth/Cache';

//@Injectable()
//class JWTInterceptor implements HttpInterceptor {

//  constructor(private router: Router) { }

//  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

//    req = req.clone({ headers: req.headers.set('Authorization', 'Bearer ' + Cache.JwtToken) });

//    return next.handle(req).do((event: HttpEvent<any>) => {
//    }, (err: any) => {
//      if (err instanceof HttpErrorResponse) {
//        if (err.status === 401) {
//          this.router.navigateByUrl('/login');
//        }
//      }
//    });
//  }
//}
