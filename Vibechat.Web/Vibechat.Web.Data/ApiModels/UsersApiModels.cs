using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Vibechat.Web.ApiModels
{
    public class ChangeNameRequest
    {
        public string newName { get; set; }
    }

    public class UpdateUserInfoRequest
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class ChangeUserIsPublicStateRequest
    {
        public string userId { get; set; }
    }

    public class BlockRequest
    {
        public string userId { get; set; }

        //there could not exist a conversation with user we want to block.
        public int? conversationId { get; set; }
    }

    public class UpdateProfilePictureResponse
    {
        public string ThumbnailUrl { get; set; }

        public string FullImageUrl { get; set; }
    }

    public class UpdateProfilePictureRequest
    {
        [FromForm(Name = "picture")] public IFormFile picture { get; set; }
    }

    public class UserInfoRequest
    {
        public string userId { get; set; }
    }


    public class IsBannedRequest
    {
        public string userid { get; set; }

        public string byWhom { get; set; }
    }

    public class UnbanRequest
    {
        /// <summary>
        ///     user to unban
        /// </summary>
        public string userId { get; set; }
    }
}