import { ApiRequestsBuilder } from "../Requests/ApiRequestsBuilder";
import { Injectable } from "@angular/core";


@Injectable()
export class DownloadsService {
  constructor(private rq: ApiRequestsBuilder) { }

  public async DownloadFile(url: string) {
    let file = await this.rq.DownloadFile(url);
    let blob = new Blob([file]);
    let URL = window.URL.createObjectURL(blob);
    let pwa = window.open(URL);

    if (!pwa || pwa.closed || typeof pwa.closed == 'undefined') {
      alert('Please disable your Pop-up blocker and try again.');
    }
  }
}
