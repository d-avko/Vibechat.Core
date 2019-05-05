"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Cache = /** @class */ (function () {
    function Cache() {
    }
    Cache.OnUserLoggedIn = function (credentials) {
        this.UserCache = credentials.info;
        this.JwtToken = credentials.token;
        this.IsAuthenticated = true;
    };
    return Cache;
}());
exports.Cache = Cache;
//# sourceMappingURL=Cache.js.map