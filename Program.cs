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
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseHttpSysOrIISIntegration()
                //.UseKestrel()
                //.UseHttpSys(options =>
                // {
                //     //options.Authentication.Schemes =
                //     //    AuthenticationSchemes.NTLM | AuthenticationSchemes.Negotiate;
                //     //options.Authentication.AllowAnonymous = true;
                //     options.UrlPrefixes.Add("http://+:5000");
                // })
                .UseContentRoot(Directory.GetCurrentDirectory())
                //.UseIISIntegration()
                .UseStartup<Startup>()
                
                //.UseUrls("http://*:5000/")
                //.UseKestrel()
                //.UseContentRoot(Directory.GetCurrentDirectory())
                //.UseIISIntegration()
                //.UseKestrel()
                //.UseUrls("http://0.0.0.0:5000")


                //.UseStartup<Startup>()
                
                ;
                



    }
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseHttpSysOrIISIntegration(this IWebHostBuilder builder)
        {
            if (builder.GetSetting("UseIISIntegration") == null)
            {
                // Self hosted
                builder.UseHttpSys(options =>
                {
                    options.Authentication.Schemes = AuthenticationSchemes.NTLM |
                        AuthenticationSchemes.Negotiate;
                    //options.Authentication.AllowAnonymous = false;
                    //options.MaxConnections = 100;
                    //options.MaxRequestBodySize = 30000000;
                    options.UrlPrefixes.Add("http://+:5000");
                });
            }

            return builder;
        }
    }
}
