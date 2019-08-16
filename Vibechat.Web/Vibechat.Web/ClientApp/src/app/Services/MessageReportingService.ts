import {SnackBarHelper} from "../Snackbar/SnackbarHelper";
import {Injectable} from "@angular/core";
import {TranslateComponent, Translation} from "../translate/translate.component";

@Injectable({
  providedIn: 'root'
})
export class MessageReportingService {
  constructor(private snackbar: SnackBarHelper) {}

  public OnConnected(): void {
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.Connected), 1);
  }

  public OnDisconnected(): void {
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.Disconnected), 3);
  }

  public TryingToReconnect(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.Reconnecting), 3);
  }

  public OnConnecting() {
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.Connecting), 1.5);
  }

  public OnSendWhileDisconnected() {
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.OnActionWhileDisconnected), 2.5);
  }

  public OnError(error: string): void {
    this.snackbar.openSnackBar(error, 2);
  }

  public OnWaitingForUserToComeOnline() {
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.WaitingForUser), 3);
  }

  public OnFailedToSubsribeToUserStatusChanges() {
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.ReEnterChat), 3);
  }

  public WrongUserDataLength(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.WrongUserDataLength));
  }

  public EnterAtLeast(symbolsCount: number){
    this.snackbar.openSnackBar(`${TranslateComponent.GetTranslationStatic(Translation.EnterAtLeast)} 
    ${symbolsCount} ${TranslateComponent.GetTranslationStatic(Translation.SymbolsToEnter)}.`);
  }

  public DisplayMessage(msg: string) {
    this.snackbar.openSnackBar(msg, 2);
  }

  public FileFailedToUpload(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.FileFailedToUpload));
  }

  public SomeFilesWereNotUploaded(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.SomeFilesWereNotUploaded));
  }

  public CouldntViewUserProfile(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.CouldntViewUserProfile));
  }

  public WrongMessagesToForward(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.WrongMessagesToForward));
  }

  public WrongSmsCode(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.WrongSms));
  }

  public CouldntSendSms(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.CouldntSendSms));
  }

  public SearchUsersNoResults(){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.UsersSearchNoResults), 2);
  }

  public FilesTooBig(maxSize: number){
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.FilesTooBig)
      + " " + maxSize.toString() + "MB", 2);
  }

  public DialogFailedToCreate() {
    this.snackbar.openSnackBar(
      TranslateComponent.GetTranslationStatic(Translation.DialogFailedToCreate), 2);
  }

  public ServerReturnedError(code: number, error: any) {
    let x = TranslateComponent.GetTranslationStatic(Translation.ServerReturnedCode);
    let y = TranslateComponent.GetTranslationStatic(Translation.WithError);
    this.snackbar.openSnackBar(`${x} ${code} ${y} ${error}`);
  }

  public NoInternet() {
    this.snackbar.openSnackBar(TranslateComponent.GetTranslationStatic(Translation.NoInternet), 2);
  }

  public ApiError(errorMessage: string) {
    this.snackbar.openSnackBar(`${TranslateComponent.GetTranslationStatic(Translation.ApiError)}: ${errorMessage}`);
  }
}
