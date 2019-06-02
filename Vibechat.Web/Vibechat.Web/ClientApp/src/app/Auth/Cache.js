"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Cache = /** @class */ (function () {
    function Cache() {
    }
    Cache.OnUserLoggedIn = function (credentials) {
        this.UserCache = credentials.info;
        localStorage.setItem('token', credentials.token);
        localStorage.setItem('refreshtoken', credentials.refreshToken);
        localStorage.setItem('user', JSON.stringify(credentials.info));
        this.IsAuthenticated = true;
        this.token = credentials.token;
    };
    Cache.TryAuthenticate = function () {
        var token = localStorage.getItem('token');
        var refreshToken = localStorage.getItem('refreshtoken');
        var user = JSON.parse(localStorage.getItem('user'));
        if (token == null || user == null || refreshToken == null) {
            return false;
        }
        this.UserCache = user;
        this.IsAuthenticated = true;
        this.token = token;
        return true;
    };
    Cache.LogOut = function () {
        localStorage.clear();
    };
    return Cache;
}());
exports.Cache = Cache;
//# sourceMappingURL=Cache.js.map