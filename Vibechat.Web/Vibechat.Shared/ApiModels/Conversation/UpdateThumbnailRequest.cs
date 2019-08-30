using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Vibechat.Shared.ApiModels.Conversation
{
    public class UpdateThumbnailRequest
    {
        [FromForm(Name = "thumbnail")] public IFormFile thumbnail { get; set; }
    }
}