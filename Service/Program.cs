using System;
using shared.d;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Thrift;
using Thrift.Protocols;
using Thrift.Server;
using Thrift.Transports;
using Thrift.Transports.Server;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Net.Security;
using Kaa.ThriftDemo.Service.Thrift;
using Kaa.ThriftDemo.ThriftManage;
using Microsoft.Extensions.Configuration;
using System.Runtime.Loader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Kaa.ThriftDemo.Service;

namespace ConsoleApp33ThriftService
{
    class Program
    {

        static void Main(string[] args)
        {
            args = args ?? new string[0];
            if (args.Any(x => x.StartsWith("-help", StringComparison.OrdinalIgnoreCase)))
            {
                DisplayHelp();
                return;
            }

            var host = new HostBuilder()
                .UseConsoleLifetime()
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
                .ConfigureLogging(logging => {
                    logging
                        .AddConsole()
                        .AddDebug();
                })
                .ConfigureServices(serviceColl=> {
                    serviceColl.AddSingleton<IHostedService, ThriftHost>();
                })
                .Build();


            var log=host.Services.GetRequiredService<ILogger<Program>>();


            using (var source = new CancellationTokenSource())
            {
                log.LogInformation("Press any key to stop...");

                host.RunAsync(source.Token);
                Console.ReadLine();
                source.Cancel();
            }

            
        }

        private static void DisplayHelp()
        {
            Console.WriteLine(@"
            Usage: 
                Server.exe -help
                    will diplay help information 
                Server.exe -tr:<transport> -pr:<protocol>
                    will run server with specified arguments (tcp transport and binary protocol by default)
            Options:
                -tr (transport): 
                    tcp - (default) tcp transport will be used (host - ""localhost"", port - 9090)
                    tcpbuffered - tcp buffered transport will be used (host - ""localhost"", port - 9090)
                    namedpipe - namedpipe transport will be used (pipe address - "".test"")
                    http - http transport will be used (http address - ""localhost:9090"")
                    tcptls - tcp transport with tls will be used (host - ""localhost"", port - 9090)
                    framed - tcp framed transport will be used (host - ""localhost"", port - 9090)
                -pr (protocol): 
                    binary - (default) binary protocol will be used
                    compact - compact protocol will be used
                    json - json protocol will be used
                    multiplexed - multiplexed protocol will be used
            Sample:
                Server.exe -tr:tcp 
            ");

        }
    }
}
