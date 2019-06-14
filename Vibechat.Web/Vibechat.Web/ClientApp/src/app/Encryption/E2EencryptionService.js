"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var crypto = require("crypto-js");
var E2EencryptionService = /** @class */ (function () {
    function E2EencryptionService() {
    }
    E2EencryptionService.prototype.GetAuthKey = function (userOrGroupId) {
        return localStorage.getItem('authKey' + userOrGroupId);
    };
    //returns null if no key was found in local storage for provided group or user id.
    E2EencryptionService.prototype.Encrypt = function (userOrGroupId, message) {
        var authKey = this.GetAuthKey(userOrGroupId);
        if (!authKey) {
            return null;
        }
        return crypto.AES.encrypt(JSON.stringify(message), authKey).ciphertext;
    };
    //returns null if no auth key was found in local storage.
    E2EencryptionService.prototype.Decrypt = function (userOrGroupId, encrypted) {
        var authKey = this.GetAuthKey(userOrGroupId);
        if (!authKey) {
            return null;
        }
        return JSON.parse(crypto.AES.decrypt(encrypted, authKey).toString());
    };
    return E2EencryptionService;
}());
exports.E2EencryptionService = E2EencryptionService;
//# sourceMappingURL=E2EencryptionService.js.map