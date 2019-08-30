using System.ComponentModel.DataAnnotations;
using Vibechat.Shared.DTO.Messages;

namespace Vibechat.DataLayer.DataModels
{
    public class AttachmentKindDataModel
    {
        [Key] public AttachmentKind Kind { get; set; }
    }
}