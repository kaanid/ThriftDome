using Kaa.ThriftDemo.Service.Thrift;
using Kaa.ThriftDemo.ThriftManage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using shared.d;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocols;
using Thrift.Transports;
using Thrift.Transports.Client;

namespace Client
{
    class Program
    {
        private static readonly ILogger Logger = new LoggerFactory()
            .AddConsole()
            .AddDebug()
            .CreateLogger(nameof(Client));
        private static IConfiguration _config = null;

        static void Main(string[] args)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("kaa.service1.config.json",true,true)
                //.AddJsonFile("kaa.service2.config.json", true, true)
                .Build();

            if (args.Any(x => x.StartsWith("-help", StringComparison.OrdinalIgnoreCase)))
            {
                DisplayHelp();
                return;
            }

            //var str=Environment.GetEnvironmentVariable("aaa");

            Logger.LogInformation("Starting client...");

            using (var source = new CancellationTokenSource())
            {
                RunAsync(args, source.Token).GetAwaiter().GetResult();

                Thread.Sleep(2000);
                RunAsync(args, source.Token).GetAwaiter().GetResult();


                Thread.Sleep(2000);
                RunAsync(args, source.Token).GetAwaiter().GetResult();

                source.CancelAfter(1000);
            }

            Thread.Sleep(500);

        }

        private static async Task RunAsync(string[] args,CancellationToken cancellationToken)
        {
            //var clientConfigList = _config.GetSection("ThriftService").Get<List<ThriftClientConfig>>();
            //var clientConfig = clientConfigList.FirstOrDefault(m => m.ServiceName.EndsWith(nameof(Calculator.Client)));
            //var clientConfig2 = _config.Get<List<ThriftClientConfig>>();
            var clientConfig = _config.Get<ThriftClientConfig>();
            var appName = _config["AppName"].ToString();

            var client =await ClientStartup.GetByCache<Calculator.Client>(clientConfig, cancellationToken, appName,true);
            //var client = await ClientStartup.Get<Calculator.Client>(clientConfig, cancellationToken, appName, true);
            await ExecuteCalculatorClientTest(cancellationToken, client);

            await Task.CompletedTask;

        }

        private static async Task ExecuteCalculatorClientTest(CancellationToken cancellationToken, Calculator.IAsync client)
        {
            Stopwatch sw = Stopwatch.StartNew();

            int max = 10000;
            foreach(var i in Enumerable.Range(0, max))
            {
                try
                {
                    var sum = await client.addAsync(1, 1, cancellationToken);
                    if (sum != 2)
                    {
                        Logger.LogInformation($" AddAsync(1,1)={sum}");
                    }
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
            }

            sw.Stop();
            Logger.LogInformation($"ExecuteCalculatorClientTest AddAsync(1,1) do:{max} ms:{sw.ElapsedMilliseconds}");
        }

        private static void DisplayHelp()
        {
            Logger.LogInformation(@"
                Usage: 
                    Client.exe -help
                        will diplay help information 
                    Client.exe -tr:<transport> -pr:<protocol> -mc:<numClients>
                        will run client with specified arguments (tcp transport and binary protocol by default) and with 1 client
                Options:
                    -tr (transport): 
                        tcp - (default) tcp transport will be used (host - ""localhost"", port - 9090)
                        tcpbuffered - buffered transport over tcp will be used (host - ""localhost"", port - 9090)
                        namedpipe - namedpipe transport will be used (pipe address - "".test"")
                        http - http transport will be used (address - ""http://localhost:9090"")        
                        tcptls - tcp tls transport will be used (host - ""localhost"", port - 9090)
                        framed - tcp framed transport will be used (host - ""localhost"", port - 9090)
                    -pr (protocol): 
                        binary - (default) binary protocol will be used
                        compact - compact protocol will be used
                        json - json protocol will be used
                        multiplexed - multiplexed protocol will be used
                    -mc (multiple clients):
                        <numClients> - number of multiple clients to connect to server (max 100, default 1)
                Sample:
                    Client.exe -tr:tcp -p:binary
                ");
        }
    }
}
