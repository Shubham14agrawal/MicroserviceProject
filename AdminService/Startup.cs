using GreenPipes;
using Jaeger;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using MassTransit;
using MassTransit.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;
using Steeltoe.Discovery.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AdminService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            List<Common.Order> orderStatus = new List<Common.Order>();
            services.AddSingleton<List<Common.Order>>(orderStatus);
            services.AddDiscoveryClient(Configuration);
            services.AddHealthChecks();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSingleton<ITracer>(serviceProvider =>
            {
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                Jaeger.Configuration.SenderConfiguration.DefaultSenderResolver = new SenderResolver(loggerFactory).RegisterSenderFactory<ThriftSenderFactory>();

                var config = Jaeger.Configuration.FromEnv(loggerFactory);
                ITracer tracer = config.GetTracer();
                GlobalTracer.Register(tracer);

                return tracer;
            });
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                                   builder => builder.SetIsOriginAllowed(hostName => true)
                                    .AllowAnyMethod()
                                    .AllowAnyHeader()
                                    .AllowCredentials()
                                  );
            });
            services.AddMassTransit(x => {
                x.AddConsumer<OrderConsumer>();
                x.AddConsumer<OrderDetailConsumer>();

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg => {

                    cfg.Host(new Uri($"rabbitmq://{Configuration["RabbitMQHostName"]}"), hostConfig => {
                        hostConfig.Username("guest");
                        hostConfig.Password("guest");
                    });

                    cfg.ReceiveEndpoint("orderDetails", ep => {
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(mr => mr.Interval(2, 100));

                        ep.ConfigureConsumer<OrderConsumer>(provider);
                    });

                    cfg.ReceiveEndpoint("ProviderUpdateStatus", ep => {
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(mr => mr.Interval(2, 100));

                        ep.ConfigureConsumer<OrderDetailConsumer>(provider);
                    });
                }));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //discovery
            app.UseDiscoveryClient();
            //app.UseHttpsRedirection();

            //app.UseMvc();
            app.UseHealthChecks("/info");

            var bus = app.ApplicationServices.GetService<IBusControl>();
            var busHandle = TaskUtil.Await(() =>
            {
                return bus.StartAsync();
            });
        }
    }
}
