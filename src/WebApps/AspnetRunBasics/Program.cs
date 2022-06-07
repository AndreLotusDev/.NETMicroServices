using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Reflection;

namespace AspnetRunBasics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, config) =>
                {
                    config
                            .Enrich.WithMachineName()
                            .Enrich.FromLogContext()
                            .Enrich.WithProperty("Enviroment", context.HostingEnvironment.EnvironmentName)
                            .WriteTo.Elasticsearch(
                                new ElasticsearchSinkOptions(new Uri(context.Configuration["ElasticConfigs:Uri"]))
                                {
                                    IndexFormat = $"applogs-{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow}",
                                    AutoRegisterTemplate = true,
                                    NumberOfShards = 2,
                                    NumberOfReplicas = 1
                                }
                            )
                            .ReadFrom.Configuration(context.Configuration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
