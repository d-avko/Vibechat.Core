using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VibeChat.Web.ApiModels
{
     public class RegisterInformationApiModel
     {
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
     }
}
