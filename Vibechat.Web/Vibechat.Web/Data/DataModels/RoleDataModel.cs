using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vibechat.Web.Data.Conversations;

namespace Vibechat.Web.Data.DataModels
{
    public class RoleDataModel
    {
        [Key]
        public ChatRole Id { get; set; }
    }
}
