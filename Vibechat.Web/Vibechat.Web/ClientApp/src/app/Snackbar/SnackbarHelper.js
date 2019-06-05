"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var SnackBarHelper = /** @class */ (function () {
    function SnackBarHelper(snackBar) {
        this.snackBar = snackBar;
        this.durationInSeconds = 4.5;
    }
    SnackBarHelper.prototype.openSnackBar = function (message, duration) {
        if (duration === void 0) { duration = this.durationInSeconds; }
        this.snackBar.open(message, null, {
            duration: this.durationInSeconds * 1000,
        });
    };
    return SnackBarHelper;
}());
exports.SnackBarHelper = SnackBarHelper;
//# sourceMappingURL=SnackbarHelper.js.map