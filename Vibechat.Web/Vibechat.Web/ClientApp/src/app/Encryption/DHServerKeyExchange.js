"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var DHServerKeyExchangeService = /** @class */ (function () {
    function DHServerKeyExchangeService(auth, connectionManager) {
        this.auth = auth;
        this.connectionManager = connectionManager;
        this.connectionManager.setDHServerKeyExchangeService(this);
    }
    DHServerKeyExchangeService.prototype.InitiateKeyExchange = function () {
    };
    return DHServerKeyExchangeService;
}());
exports.DHServerKeyExchangeService = DHServerKeyExchangeService;
//# sourceMappingURL=DHServerKeyExchange.js.map