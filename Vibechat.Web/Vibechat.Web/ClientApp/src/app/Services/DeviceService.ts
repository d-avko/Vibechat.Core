import { UAParser } from "ua-parser-js";

export class DeviceService {

  constructor() {
    this.CheckIfSecureChatsSupported();
    this.deviceId = (window.devicePixelRatio * window.screen.colorDepth * window.screen.pixelDepth).toString();
  }

  public isSecureChatsSupported: boolean = false;

  private deviceId : string;

  private browserName: string;

  public GetDeviceId() {
    return this.deviceId + this.browserName;
  }

  public CheckIfSecureChatsSupported() {
    let browserInfo = new UAParser().getBrowser();
    let name = browserInfo.name;
    let versionNumber = Number.parseInt(browserInfo.version.substr(0, browserInfo.version.indexOf(".")));
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
  }
}
