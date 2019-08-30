using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Vibechat.Shared.ApiModels
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

    public class UpdateProfilePictureResponse
    {
        public string ThumbnailUrl { get; set; }

        public string FullImageUrl { get; set; }
    }

    public class UpdateProfilePictureRequest
    {
        [FromForm(Name = "picture")] public IFormFile picture { get; set; }
    }
}