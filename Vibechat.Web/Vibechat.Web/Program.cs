using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Vibechat.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var currentDir = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("hosting.json", optional: false)
              .AddJsonFile($"hosting.{environment}.json", optional: true)
              .AddEnvironmentVariables()
              .Build();


            var host = WebHost.CreateDefaultBuilder(args)
              .UseConfiguration(config)
              .UseContentRoot(currentDir)
              .UseStartup<Startup>()
              .UseKestrel(options =>
              {
                  options.ConfigureHttpsDefaults((opts) =>
                  {
                      var certificateSettings = config.GetSection("certificateSettings");
                      var certificateFileName = certificateSettings.GetValue<string>("filename");
                      var certificatePassword = certificateSettings.GetValue<string>("password");
                      var cert = new X509Certificate2(certificateFileName, certificatePassword);
                      opts.ServerCertificate = cert;
                  });

              })
              .Build();

            return host;
        }
    }
}
