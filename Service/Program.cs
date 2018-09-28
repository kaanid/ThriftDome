using AspectCore.Extensions.DependencyInjection;
using Kaa.ThriftDemo.Service;
using Kaa.ThriftDemo.Service.Thrift;
using Kaa.ThriftDemo.ThriftManage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using shared.d;
using System;
using System.Linq;
using System.Threading;

namespace ConsoleApp33ThriftService
{
    class Program
    {

        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .UseConsoleLifetime()
                .UseServiceProviderFactory(new AspectCoreServiceProviderFactory())
                .ConfigureHostConfiguration(conf =>
                {
                    conf
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);

                })
                .ConfigureAppConfiguration((context, conf) =>
                {
                    var env = context.HostingEnvironment;
                    conf
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((context,logging) => {
                    logging
                        .AddConfiguration(context.Configuration.GetSection("Logging"))
                        .AddConsole()
                        .AddDebug();
                })
                .ConfigureServices(serviceColl => {
                    serviceColl.AddSingleton<IThriftServiceStatistics, ThriftServiceStatistics>();
                    serviceColl.AddSingleton<Calculator.IAsync, ThriftService>();
                    serviceColl.AddSingleton<IHostedService, ThriftHost>();
                })
                .Build();

            var log=host.Services.GetRequiredService<ILogger<Program>>();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                Console.WriteLine($"UnhandledException,app is terminating:{e.IsTerminating} exception:{e.ExceptionObject}");
                log.LogError(e.ExceptionObject as Exception, $"UnhandledException,app is terminating:{e.IsTerminating}");
            };

            using (var source = new CancellationTokenSource())
            {
                log.LogInformation("Press any key to stop...");

                host.RunAsync(source.Token);
                Console.ReadLine();
                source.Cancel();
            }
        }
    }
}
