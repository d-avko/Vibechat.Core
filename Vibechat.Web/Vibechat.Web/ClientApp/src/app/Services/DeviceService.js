"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var ua_parser_js_1 = require("ua-parser-js");
var DeviceService = /** @class */ (function () {
    function DeviceService() {
        this.isSecureChatsSupported = false;
        this.CheckIfSecureChatsSupported();
        this.deviceId = (window.devicePixelRatio * window.screen.colorDepth * window.screen.pixelDepth).toString();
    }
    DeviceService.prototype.GetDeviceId = function () {
        return this.deviceId + this.browserName;
    };
    DeviceService.prototype.CheckIfSecureChatsSupported = function () {
        var browserInfo = new ua_parser_js_1.UAParser().getBrowser();
        var name = browserInfo.name;
        var versionNumber = Number.parseInt(browserInfo.version.substr(0, browserInfo.version.indexOf(".")));
        this.browserName = name;
        switch (name) {
            case "Chrome": {
                if (versionNumber >= 67) {
                    this.isSecureChatsSupported = true;
                }
            }
            case "Mozilla": {
                if (versionNumber >= 68) {
                    this.isSecureChatsSupported = true;
                }
            }
            case "Firefox": {
                if (versionNumber >= 68) {
                    this.isSecureChatsSupported = true;
                }
            }
            case "Opera": {
                if (versionNumber >= 54) {
                    this.isSecureChatsSupported = true;
                }
            }
            case "Android Browser": {
                if (versionNumber >= 67) {
                    this.isSecureChatsSupported = true;
                }
            }
            default: {
            }
        }
    };
    return DeviceService;
}());
exports.DeviceService = DeviceService;
//# sourceMappingURL=DeviceService.js.map