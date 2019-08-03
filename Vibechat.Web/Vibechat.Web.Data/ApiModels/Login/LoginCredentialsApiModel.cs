using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibechat.Web.ApiModels
{
    /// <summary>
    /// Credentials to pass to an API to log in, receive Jwt token back
    /// </summary>
    public class LoginCredentialsApiModel
    {
        public string UidToken { get; set; }

        public string PhoneNumber { get; set; }
    }
}
