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
        private static readonly ILogger Logger = new LoggerFactory().AddConsole().AddDebug().CreateLogger(nameof(Client));
        private static IConfiguration _config = null;

        static void Main(string[] args)
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            if (args.Any(x => x.StartsWith("-help", StringComparison.OrdinalIgnoreCase)))
            {
                DisplayHelp();
                return;
            }

            Logger.LogInformation("Starting client...");

            using (var source = new CancellationTokenSource())
            {
                RunAsync(args, source.Token).GetAwaiter().GetResult();
                Thread.Sleep(2000);
                //source.Cancel();
                RunAsync(args, source.Token).GetAwaiter().GetResult();
            }

            Thread.Sleep(500);

        }

        private static async Task RunAsync(string[] args,CancellationToken cancellationToken)
        {
            var clientConfigList = _config.GetSection("ThriftService").Get<List<ThriftClientConfig>>();
            var clientConfig = clientConfigList.FirstOrDefault(m => m.ServiceName.EndsWith(nameof(Calculator.Client)));

            var client =await ClientStartup.GetByCache<Calculator.Client>(clientConfig, cancellationToken,true);
            await ExecuteCalculatorClientTest(cancellationToken, client);

            //Task.WaitAll(tasks);

            //await Task.FromResult(1);
            await Task.CompletedTask;

        }

        private static async Task ExecuteSharedServiceClientOperations(CancellationToken cancellationToken,SharedService.Client client)
        {
            await client.OpenTransportAsync(cancellationToken);

            Logger.LogInformation($"{client.ClientId} SharedService GetStructAsync(1)");
            var log = await client.getStructAsync(1, cancellationToken);
            Logger.LogInformation($"{client.ClientId} SharedService Value: {log.Value}");
        }

        private static async Task ExecuteCalculatorClientTest(CancellationToken cancellationToken, Calculator.IAsync client)
        {
            Stopwatch sw = Stopwatch.StartNew();

            int max = 100000;
            foreach(var i in Enumerable.Range(0, max))
            {
                var sum = await client.addAsync(1, 1, cancellationToken);
                //Logger.LogInformation($"{client.ClientId} AddAsync(1,1)={sum}");
            }

            sw.Stop();
            Logger.LogInformation($"ExecuteCalculatorClientTest AddAsync(1,1) do:{max} ms:{sw.ElapsedMilliseconds}");
        }

        private static async Task ExecuteCalculatorClientOperations(CancellationToken cancellationToken,Calculator.Client client)
        {
            await client.OpenTransportAsync(cancellationToken);

            Logger.LogInformation($"{client.ClientId} PingAsync()");
            await client.pingAsync(cancellationToken);

            Logger.LogInformation($"{client.ClientId} AddAsync(1,1)");
            var sum=await client.addAsync(1, 1, cancellationToken);
            Logger.LogInformation($"{client.ClientId} AddAsync(1,1)={sum}");

            var work = new Work
            {
                Op = Operation.DIVIDE,
                Num1 = 1,
                Num2 = 0
            };

            try
            {
                Logger.LogInformation($"{client.ClientId} CalculateAsync(1)");
                var n=await client.calculateAsync(1, work, cancellationToken);
                Logger.LogInformation($"{client.ClientId} Whoa we can divide by 0 n:{n}");
            }
            catch (InvalidOperation io)
            {
                Logger.LogInformation($"{client.ClientId} Invalid operation: " + io);
            }

            Logger.LogInformation($"{client.ClientId} GetStructAsync(1)");
            var log = await client.getStructAsync(1, cancellationToken);
            Logger.LogInformation($"{client.ClientId} Check log: {log.Value}");

            Logger.LogInformation($"{client.ClientId} ZipAsync() with delay 100mc on server side");
            await client.zipAsync(cancellationToken);
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
