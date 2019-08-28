import {AuthService} from "./AuthService";
import {Api} from "./Api/api.service";
import {AppUser} from "../Data/AppUser";
import {Injectable} from "@angular/core";
import {BanEvent, SignalrConnection} from "../Connections/signalr-connection.service";
import {UploadsApi} from "./Api/UploadsApi";

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  constructor(private api: Api,
              private connectionManager: SignalrConnection,
              private auth: AuthService,
              private uploads: UploadsApi) { }

  public async GetById(userId: string){
    let result = await this.api.GetUserById(userId);

    if(!result.isSuccessfull){
      return null;
    }

    return result.response;
  }

  //Fetches user info of specified user. If current user id specified,
  //updates user entry in AuthService.
  public async UpdateUserInfo(userId: string) : Promise<AppUser> {
    let result = await this.api.GetUserById(userId);

    if (!result.isSuccessfull) {
      return null;
    }

    if (result.response.id == this.auth.User.id) {
      this.auth.User = result.response;
    }

    return result.response;
  }

  public async FindUsersByUsername(name: string): Promise<Array<AppUser>>{
    let result = await this.api.FindUsersByUsername(name);

    if (!result.isSuccessfull) {
      return null;
    }

    if (result.response.usersFound == null) {
      return new Array<AppUser>();

    } else {
      return [...result.response.usersFound];
    }
  }

  public async ChangeProfileVisibility(){
    let res = await this.api.ChangeUserPublicVisibility();

    if(res.isSuccessfull){
      this.auth.User.isPublic = !this.auth.User.isPublic;
    }
  }

  public async UpdateContacts() {
    let contacts = await this.api.GetContacts();

    if (!contacts.isSuccessfull) {
      return;
    }

    if (!contacts.response) {
      this.auth.Contacts = new Array<AppUser>();
    } else {
      this.auth.Contacts = contacts.response;
    }
  }

  public HasContactWith(user: AppUser) {
    return this.auth.Contacts.findIndex(x => x.id == user.id) != -1;
  }

  public async AddToContacts(user: AppUser) {
    let result = await this.api.AddToContacts(user.id);

    if (!result.isSuccessfull) {
      return;
    }

    this.auth.Contacts.push(user);
  }

  public async RemoveFromContacts(user: AppUser) {
    let result = await this.api.RemoveFromContacts(user.id);

    if (!result.isSuccessfull) {
      return;
    }

    let contactIndex = this.auth.Contacts.findIndex(x => x.id == user.id);

    if (contactIndex == -1) {
      return;
    }

    this.auth.Contacts.splice(contactIndex, 1);
  }

  public async BlockUser(user: AppUser) {
    let result = await this.connectionManager.BlockUser(user.id, BanEvent.Banned);

    if (!result) {
      return;
    }

    user.isBlocked = true;
  }

  public async UnblockUser(user: AppUser) {
    let result = await this.connectionManager.BlockUser(user.id, BanEvent.Unbanned);

    if (!result) {
      return;
    }

    user.isBlocked = false;
  }

  public async ChangeLastname(name: string) {
    let result = await this.api.ChangeCurrentUserLastName(name);

    if (!result.isSuccessfull) {
      return;
    }

    this.auth.User.lastName = name;
  }

  public async ChangeName(name: string) {
    let result = await this.api.ChangeCurrentUserName(name);

    if (!result.isSuccessfull) {
      return;
    }

    this.auth.User.name = name;
  }

  public async ChangeUsername(name: string) {
    let result = await this.api.ChangeUsername(name);

    if (!result.isSuccessfull) {
      return;
    }

    this.auth.User.userName = name;
  }

  public async UpdateProfilePicture(file: File, progressCallback: (value: number) => void) {
    let result = await this.uploads.UploadUserProfilePicture(file, progressCallback);

    if (!result.isSuccessfull) {
      return;
    }

    this.auth.User.imageUrl = result.response.thumbnailUrl;
    this.auth.User.fullImageUrl = result.response.fullImageUrl;
  };
}
