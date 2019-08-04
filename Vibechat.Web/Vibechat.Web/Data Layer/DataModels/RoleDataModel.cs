using System.ComponentModel.DataAnnotations;
using Vibechat.Web.Data.Conversations;

namespace Vibechat.Web.Data.DataModels
{
    public class RoleDataModel
    {
        [Key] public ChatRole Id { get; set; }
    }
}