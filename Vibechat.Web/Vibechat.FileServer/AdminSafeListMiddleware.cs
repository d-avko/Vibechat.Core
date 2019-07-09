using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Vibechat.FileServer
{
    public class AdminSafeListMiddleware : IMiddleware
    {
        private string _adminSafeList { get; set; }

        public AdminSafeListMiddleware(IConfiguration config)
        {
            _adminSafeList = config["AdminSafeList"];
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method != "GET")
            {
                var remoteIp = context.Connection.RemoteIpAddress;

                string[] ip = _adminSafeList.Split(';');

                var normalizedIp = remoteIp.ToString().Replace("::ffff:", string.Empty);

                var badIp = true;

                foreach (string address in ip)
                {
                    var testIp = IPAddress.Parse(address);

                    if (testIp.ToString().Equals(normalizedIp))
                    {
                        badIp = false;
                        break;
                    }
                }

                if (badIp)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return;
                }
            }

            await next.Invoke(context);
        }
    }
}
