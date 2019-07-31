"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var LoadingScreenService = /** @class */ (function () {
    function LoadingScreenService() {
    }
    LoadingScreenService.prototype.startLoading = function () {
        this.isLoading = true;
    };
    LoadingScreenService.prototype.stopLoading = function () {
        this.isLoading = false;
    };
    return LoadingScreenService;
}());
exports.LoadingScreenService = LoadingScreenService;
//# sourceMappingURL=LoadingScreenService.js.map