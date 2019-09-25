using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Vibechat.DataLayer.DataModels
{
    public class UserConnectionDataModel
    {
        [Key]
        public string ConnectionId { get; set; }

        public string UserID { get; set; }

        [ForeignKey("UserID")] public virtual AppUser User { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            return (obj as UserConnectionDataModel)?.ConnectionId == this.ConnectionId;
        }

        public static UserConnectionDataModel Create(string connectionId, string userId)
        {
            return new UserConnectionDataModel()
            {
                ConnectionId = connectionId,
                UserID = userId
            };
        }
    }
}
