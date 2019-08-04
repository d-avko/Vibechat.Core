using System.ComponentModel.DataAnnotations;
using Vibechat.Web.Data.Messages;

namespace VibeChat.Web.Data.DataModels
{
    public class AttachmentKindDataModel
    {
        [Key] public AttachmentKind Kind { get; set; }
    }
}