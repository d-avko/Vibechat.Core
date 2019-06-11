"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var ThemesService = /** @class */ (function () {
    function ThemesService() {
        this.themes = new (Array.bind.apply(Array, [void 0].concat(['white', 'dark'])))();
        this.themesClassNames = new (Array.bind.apply(Array, [void 0].concat(['white-theme', 'dark-theme'])))();
    }
    ThemesService.prototype.changeTheme = function (name) {
        var index = this.themes.findIndex(function (x) { return x == name; });
        if (index == -1) {
            return;
        }
        var body = document.getElementsByTagName('body').item(0);
        var _loop_1 = function (Class) {
            if (this_1.themesClassNames.findIndex(function (x) { return x == Class; }) == -1) {
                body.classList.remove(Class);
            }
        };
        var this_1 = this;
        for (var Class in body.classList) {
            _loop_1(Class);
        }
        body.classList.add(this.themesClassNames[index]);
        this.currentTheme = this.themesClassNames[index];
    };
    return ThemesService;
}());
exports.ThemesService = ThemesService;
//# sourceMappingURL=ThemesService.js.map