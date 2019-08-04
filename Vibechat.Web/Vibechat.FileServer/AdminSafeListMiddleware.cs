using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Vibechat.FileServer
{
    public class AdminSafeListMiddleware : IMiddleware
    {
        public AdminSafeListMiddleware(IConfiguration config)
        {
            _adminSafeList = config["AdminSafeList"];
        }

        private string _adminSafeList { get; }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method != "GET")
            {
                var remoteIp = context.Connection.RemoteIpAddress;

                var ip = _adminSafeList.Split(';');

                var normalizedIp = remoteIp.ToString().Replace("::ffff:", string.Empty);

                var badIp = true;

                foreach (var address in ip)
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
                    context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    return;
                }
            }

            await next.Invoke(context);
        }
    }
}