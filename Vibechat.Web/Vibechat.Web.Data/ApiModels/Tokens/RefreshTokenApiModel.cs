using System;
using System.Collections.Generic;
using System.Text;

namespace Vibechat.Web.Data.ApiModels.Tokens
{
    public class RefreshTokenApiModel
    {
        public string RefreshToken { get; set; }

        public string UserId { get; set; }
    }
}
