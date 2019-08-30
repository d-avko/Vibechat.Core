using System.Collections.Generic;
using Vibechat.Shared.DTO.Users;

namespace Vibechat.Shared.ApiModels.Users_Info
{
    public class UsersByNickNameResultApiModel
    {
        public List<AppUserDto> UsersFound { get; set; }
    }
}