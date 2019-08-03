export class LoadingScreenService {
  public isLoading: boolean;

  public startLoading() {
    this.isLoading = true;
  }

  public stopLoading() {
    this.isLoading = false;
  }
}
