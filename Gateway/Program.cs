using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Eureka;

namespace ServiceGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }


        public static IWebHost BuildWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
               .ConfigureAppConfiguration((hostingContext, config) =>
               {
                   config
                       .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                       .AddJsonFile("appsettings.json", true, true)
                       .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true,
                           true)
                       .AddJsonFile("OcelotConfig.json", false, false)
                       .AddEnvironmentVariables();
               })
               .ConfigureServices(services =>
               {

                   services.AddCors(options =>
                   {
                       options.AddPolicy("CorsPolicy",
                                           builder => builder.SetIsOriginAllowed(hostName => true)
                                            .AllowAnyMethod()
                                            .AllowAnyHeader()
                                            .AllowCredentials()
                                          );
                   });

                   services.AddOcelot()
                   .AddEureka();
               })
               .Configure(a =>
               {
                   a.UseOcelot().Wait();
                   a.UseDeveloperExceptionPage();
                   a.UseCors("CorsPolicy");

               })
               .Build();
        }
    }
}
