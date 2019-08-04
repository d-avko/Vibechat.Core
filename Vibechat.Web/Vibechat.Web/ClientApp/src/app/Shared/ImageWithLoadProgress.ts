import { ImageScalingService } from "../Services/ImageScalingService";
import { Inject, Injectable } from "@angular/core";

@Injectable()
export class ImageWithLoadProgress {
  constructor(private scaling: ImageScalingService) { }

  public internalImg: HTMLImageElement;
  public isDownloading: boolean = false;
  public xmlHTTP: XMLHttpRequest;

  public load(url: string) {
    this.isDownloading = true;
    this.internalImg = new Image();
    this.internalImg.onload = () => {
      let d = this.scaling.AdjustFullSizedImageDimensions(this.internalImg.width, this.internalImg.height);
      this.internalImg.width = d.width;
      this.internalImg.height = d.height;
      setTimeout(() => this.isDownloading = false, 500);
    };
    this.internalImg.src = url;
  }
}
