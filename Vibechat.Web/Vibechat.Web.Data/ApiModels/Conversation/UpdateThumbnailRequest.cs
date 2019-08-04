using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Vibechat.Web.Data.ApiModels.Conversation
{
    public class UpdateThumbnailRequest
    {
        [FromForm(Name = "thumbnail")] public IFormFile thumbnail { get; set; }

        [FromForm(Name = "conversationId")] public int conversationId { get; set; }
    }
}