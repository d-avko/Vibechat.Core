using System.ComponentModel.DataAnnotations;
using Vibechat.Shared.DTO.Conversations;

namespace Vibechat.DataLayer.DataModels
{
    public class RoleDataModel
    {
        [Key] public ChatRole Id { get; set; }
    }
}