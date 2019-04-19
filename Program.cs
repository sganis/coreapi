using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coreapi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();

            //var config = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("hosting.json", optional: true)
            //    .AddEnvironmentVariables(prefix: "ASPNETCORE_")
            //    .AddCommandLine(args)
            //    .Build();

            //    var host = new WebHostBuilder()
            //        .UseUrls("http://0.0.0.0:5000")
            //        .UseEnvironment("Development")
            //        .UseConfiguration(config)
            //        .UseKestrel()
            //        .UseContentRoot(Directory.GetCurrentDirectory())
            //        .UseIISIntegration()
            //        .UseStartup<Startup>()
            //        .Build();

            //host.Run();

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //.UseUrls("http://*:5000/")
                //.UseKestrel()
                //.UseContentRoot(Directory.GetCurrentDirectory())
                //.UseIISIntegration()
                //.UseKestrel()
                //.UseUrls("http://0.0.0.0:5000")
                .UseStartup<Startup>()
                .UseHttpSys(options =>
                 {
                     options.Authentication.Schemes =
                         AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                     options.Authentication.AllowAnonymous = true;
                     options.UrlPrefixes.Add("http://+:5000/");
                 });
                



    }
}
