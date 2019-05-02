using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vibechat.Web.ApiModels
{
    public class ResponseApiModel<T>
    {
        public bool IsSuccessfull { get; set; }

        public string ErrorMessage { get; set; }

        public T Response { get; set; }
    }
}
