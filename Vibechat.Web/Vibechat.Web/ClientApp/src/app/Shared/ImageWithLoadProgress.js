"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var ImageWithLoadProgress = /** @class */ (function () {
    function ImageWithLoadProgress(scaling) {
        this.scaling = scaling;
        this.completedPercentage = 0;
        this.isDownloading = false;
        this.internalImg = new Image();
    }
    ImageWithLoadProgress.prototype.load = function (url) {
        var _this = this;
        var internalThis = this.internalImg;
        var thisImg = this;
        this.xmlHTTP = new XMLHttpRequest();
        this.xmlHTTP.open('GET', url, true);
        this.xmlHTTP.responseType = 'arraybuffer';
        this.xmlHTTP.onload = function (e) {
            var blob = new Blob([this.response]);
            internalThis.onload = function () {
                var d = thisImg.scaling.AdjustFullSizedImageDimensions(internalThis.width, internalThis.height);
                internalThis.width = d.width;
                internalThis.height = d.height;
            };
            internalThis.src = window.URL.createObjectURL(blob);
            setTimeout(function () { return thisImg.isDownloading = false; }, 500);
        };
        this.xmlHTTP.onprogress = function (e) {
            _this.completedPercentage = (e.loaded / e.total) * 100;
        };
        this.xmlHTTP.onloadstart = function () {
            _this.completedPercentage = 0;
            _this.isDownloading = true;
        };
        this.xmlHTTP.onerror = function () {
            _this.isDownloading = false;
        };
        this.xmlHTTP.onabort = function () {
            _this.isDownloading = false;
        };
        this.xmlHTTP.send();
    };
    return ImageWithLoadProgress;
}());
exports.ImageWithLoadProgress = ImageWithLoadProgress;
//# sourceMappingURL=ImageWithLoadProgress.js.map