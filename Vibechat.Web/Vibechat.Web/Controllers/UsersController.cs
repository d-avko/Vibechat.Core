using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.ApiModels;
using VibeChat.Web.ApiModels;
using VibeChat.Web.ChatData;

namespace VibeChat.Web.Controllers
{
    public class UsersController : Controller
    {
        protected ApplicationDbContext mContext;

        protected UserManager<UserInApplication> mUserManager;

        public UsersController(ApplicationDbContext context, UserManager<UserInApplication> userManager)
        {
            mContext = context;
            mUserManager = userManager;
        }

        #region Users info
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/GetById")]
        public async Task<ResponseApiModel<UserByIdApiResponseModel>> GetUserById([FromBody]UserByIdApiModel userId)
        {
            if (userId == null)
            {
                return new ResponseApiModel<UserByIdApiResponseModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = "Provided user was null"
                };
            }

            var FoundUser = await mContext.Users.FindAsync(userId.Id).ConfigureAwait(false);

            if (FoundUser == null)
            {
                return new ResponseApiModel<UserByIdApiResponseModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = "User was not found"
                };
            }


            return new ResponseApiModel<UserByIdApiResponseModel>()
            {
                IsSuccessfull = true,
                Response = new UserByIdApiResponseModel()
                {
                    User = new UserInfo()
                    {
                        Id = FoundUser.Id,
                        ImageUrl = FoundUser.ProfilePicImageURL,
                        LastName = FoundUser.LastName,
                        LastSeen = FoundUser.LastSeen,
                        Name = FoundUser.FirstName,
                        UserName = FoundUser.UserName,
                        ProfilePicRgb = FoundUser.ProfilePicRgb,
                        ConnectionId = FoundUser.ConnectionId,
                        IsOnline = FoundUser.IsOnline
                    }
                }
            };

        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("api/Users/FindByNickname")]
        public ResponseApiModel<UsersByNickNameResultApiModel> FindUsersByNickName([FromBody]UsersByNickNameApiModel credentials)
        {
            if (credentials.UsernameToFind == null)
            {
                return new ResponseApiModel<UsersByNickNameResultApiModel>()
                {
                    IsSuccessfull = false,
                    ErrorMessage = "Nickname was null"
                };
            }

            var result = mUserManager.Users.Where(user => user.UserName.Contains(credentials.UsernameToFind)).ToList();

            if (result.Count() == 0)
            {
                return new ResponseApiModel<UsersByNickNameResultApiModel>()
                {
                    ErrorMessage = "Noone was found.",
                    IsSuccessfull = false
                };
            }

            return new ResponseApiModel<UsersByNickNameResultApiModel>()
            {
                IsSuccessfull = true,
                Response = new UsersByNickNameResultApiModel()
                {
                    UsersFound = result.Select((FoundUser) => new FoundUser
                    {
                        ID = FoundUser.Id,
                        Username = FoundUser.UserName,
                        ProfilePicRgb = FoundUser.ProfilePicRgb,
                        FirstName = FoundUser.FirstName,
                        LastName = FoundUser.LastName
                    }).ToList()
                }
            };
        }
        #endregion
    }
}
