import { UAParser } from "ua-parser-js";

export class DeviceService {

  constructor() {
    this.Initialize();
    this.deviceId = (window.devicePixelRatio * window.screen.colorDepth * window.screen.pixelDepth).toString();
  }

  public isSecureChatsSupported: boolean = false;

  private deviceId : string;

  private browserName: string;

  public GetDeviceId() {
    return this.deviceId + this.browserName;
  }

  public Initialize() {
    let browserInfo = new UAParser().getBrowser();
    this.browserName = browserInfo.name;
  }
}
