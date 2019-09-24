using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.BusinessLogic.Extensions
{
    public static class EntitiesExtensions
    {
        public static string[] ToConnectionIds(this ICollection<UserConnectionDataModel> value)
        {
            if(value == null || value.Count.Equals(0))
            {
                return new string[0];
            }

            return value.Select(x => x.ConnectionId).ToArray();
        }
    }
}
