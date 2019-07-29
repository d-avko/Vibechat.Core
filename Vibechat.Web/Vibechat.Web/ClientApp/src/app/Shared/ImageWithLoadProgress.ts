import { ImageScalingService } from "../Services/ImageScalingService";

export class ImageWithLoadProgress {
  constructor(private scaling: ImageScalingService) {
    this.internalImg = new Image();
  }

  public internalImg: HTMLImageElement;
  public completedPercentage: number = 0;
  public isDownloading: boolean = false;
  public xmlHTTP: XMLHttpRequest;

  public load(url: string) {
    var internalThis = this.internalImg;
    var thisImg = this;

    this.xmlHTTP = new XMLHttpRequest();

    this.xmlHTTP.open('GET', url, true);
    this.xmlHTTP.responseType = 'arraybuffer';

    this.xmlHTTP.onload = function (e) {
      var blob = new Blob([this.response]);

      internalThis.onload = () => {
        let d = thisImg.scaling.AdjustFullSizedImageDimensions(internalThis.width, internalThis.height);
        internalThis.width = d.width;
        internalThis.height = d.height;
      }

      internalThis.src = window.URL.createObjectURL(blob);

      setTimeout(() => thisImg.isDownloading = false, 500);
    };

    this.xmlHTTP.onprogress = (e) => {
      this.completedPercentage = (e.loaded / e.total) * 100;
    };

    this.xmlHTTP.onloadstart = () => {
      this.completedPercentage = 0;
      this.isDownloading = true;
    };

    this.xmlHTTP.onerror = () => {
      this.isDownloading = false;
    }

    this.xmlHTTP.onabort = () => {
      this.isDownloading = false;
    }

    this.xmlHTTP.send();
  }
}
