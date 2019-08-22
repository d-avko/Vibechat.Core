using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Vibechat.FileServer
{
    public class AdminSafeListMiddleware : IMiddleware
    {
        private readonly ILogger<AdminSafeListMiddleware> logger;

        public AdminSafeListMiddleware(IConfiguration config, ILogger<AdminSafeListMiddleware> logger)
        {
            this.logger = logger;
            _adminSafeList = config["AdminSafeList"];
        }

        private string _adminSafeList { get; }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method != "GET")
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                logger.LogInformation($"Remoteip : ${remoteIp.ToString()}");
                var ip = _adminSafeList.Split(';');
                logger.LogInformation($"Length of safe ips : ${ip.Length}, count ${ip.Count()}");
                var normalizedIp = remoteIp.ToString().Replace("::ffff:", string.Empty);
                logger.LogInformation($"Normalized ip: ${normalizedIp}");
                var badIp = true;

                foreach (var address in ip)
                {
                    var testIp = IPAddress.Parse(address);
                    logger.LogInformation($"Test ip: ${testIp.ToString()}");
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