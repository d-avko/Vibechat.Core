"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Cache = /** @class */ (function () {
    function Cache() {
    }
    Cache.OnUserLoggedIn = function (credentials) {
        this.UserCache = credentials.info;
        this.JwtToken = credentials.token;
        localStorage.setItem('token', this.JwtToken);
        localStorage.setItem('user', JSON.stringify(credentials.info));
        this.IsAuthenticated = true;
    };
    Cache.TryAuthenticate = function () {
        var token = localStorage.getItem('token');
        var user = JSON.parse(localStorage.getItem('user'));
        if (token == null || user == null) {
            return false;
        }
        this.UserCache = user;
        this.JwtToken = token;
        this.IsAuthenticated = true;
        return true;
    };
    Cache.LogOut = function () {
        localStorage.clear();
    };
    return Cache;
}());
exports.Cache = Cache;
//# sourceMappingURL=Cache.js.map