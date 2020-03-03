using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestWebApplication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfiguration configuration, ILogger<Startup> logger)
        {
            var stanLogger = new MyLogger<Startup>("Stan",configuration, new LoggerExternalScopeProvider());

            var natsLogger = new MyLogger<Startup>("Nats",configuration,new LoggerExternalScopeProvider());

            var defaultMyLogger = new MyLogger<Startup>(configuration,new LoggerExternalScopeProvider());

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddConfiguration(configuration);
            });

            ILogger loggerInst = loggerFactory.CreateLogger<Startup>();


            app.Run(async (context) =>
            {
                //logger.Log(LogLevel.Warning,"Warning level");

                //stanLogger.Log(LogLevel.Trace, "Trace level");
                //stanLogger.Log(LogLevel.Debug, "Debug level");
                //stanLogger.Log(LogLevel.Information, "Information level");
                //stanLogger.Log(LogLevel.Warning, "Warning level");
                //stanLogger.Log(LogLevel.Error, "Error level");
                //stanLogger.Log(LogLevel.Critical, "Critical level");
                //stanLogger.Log(LogLevel.None, "None level");

                //defaultMyLogger.Log(LogLevel.Trace, "Trace level");
                //defaultMyLogger.Log(LogLevel.Debug, "Debug level");
                //defaultMyLogger.Log(LogLevel.Information, "Information level");
                //defaultMyLogger.Log(LogLevel.Warning, "Warning level");
                //defaultMyLogger.Log(LogLevel.Error, "Error level");
                //defaultMyLogger.Log(LogLevel.Critical, "Critical level");
                //defaultMyLogger.Log(LogLevel.None, "None level");

                Method1(stanLogger);

                //natsLogger.Log(LogLevel.Trace, "Trace level");
                //natsLogger.Log(LogLevel.Debug, "Debug level");
                //natsLogger.Log(LogLevel.Information, "Information level");
                //natsLogger.Log(LogLevel.Warning, "Warning level");
                //natsLogger.Log(LogLevel.Error, "Error level");
                //natsLogger.Log(LogLevel.Critical, "Critical level");
                //natsLogger.Log(LogLevel.None, "None level");


                await context.Response.WriteAsync("Hello world!");
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }

        private void Method1(ILogger<Startup> logger)
        {
            using (logger.BeginScope("Method1 Up"))
            {
                logger.LogInformation("Method1 LogInformation1");
                
                //using (logger.BeginScope("Method1 Down"))
                {
                    logger.LogInformation("Method1 LogInformation2");
                    
                    Method2(logger);
                    Method3(logger);
                }
            }
        }

        private void Method2(ILogger<Startup> logger)
        {
            //using (logger.BeginScope("Method2"))
            {
                logger.LogInformation("Method2 LogInformation");
            }
        }

        private void Method3(ILogger<Startup> logger)
        {
            //using (logger.BeginScope("Method3 Up"))
            {
                logger.LogInformation("Method3 LogInformation Up");

                Method4(logger);

                using (logger.BeginScope("Method3 Down"))
                {
                    logger.LogInformation("Method3 LogInformation Down");
                }
            }
        }

        private void Method4(ILogger<Startup> logger)
        {
            //using (logger.BeginScope("Method4"))
            {
                logger.LogInformation("Method4 LogInformation");
            }
        }
    }
}
