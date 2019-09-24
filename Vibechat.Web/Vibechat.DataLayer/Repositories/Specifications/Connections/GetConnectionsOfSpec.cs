using System;
using System.Collections.Generic;
using System.Text;
using Vibechat.DataLayer.DataModels;

namespace Vibechat.DataLayer.Repositories.Specifications.Connections
{
    public class GetConnectionsOfSpec : BaseSpecification<UserConnectionDataModel>
    {
        public GetConnectionsOfSpec(string userId)
            : base(entry => entry.UserID == userId)
        {
        }
    }
}
