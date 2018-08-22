using Kaa.ThriftDemo.Service.Thrift;
using Microsoft.Extensions.Logging;
using shared.d;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocols;
using Thrift.Transports;
using Thrift.Transports.Client;
using ThriftManage;

namespace Client
{
    class Program
    {
        private static readonly ILogger Logger = new LoggerFactory().AddConsole().AddDebug().CreateLogger(nameof(Client));
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            args = args ?? new string[0];
            args = new string[] { "-pr:Binary", "-tr:TcpBuffered" };
            Logger.LogInformation($"args:{string.Join(' ',args)}");

            if (args.Any(x => x.StartsWith("-help", StringComparison.OrdinalIgnoreCase)))
            {
                DisplayHelp();
                return;
            }

            Logger.LogInformation("Starting client...");

            using (var source = new CancellationTokenSource())
            {
                RunAsync(args, source.Token).GetAwaiter().GetResult();
                Thread.Sleep(1000);
                RunAsync(args, source.Token).GetAwaiter().GetResult();
            }

            Thread.Sleep(500);

        }

        private static async Task RunAsync(string[] args,CancellationToken cancellationToken)
        {
            var numClients = 1;

            Logger.LogInformation($"Selected # of clients: {numClients}");
            var transport = Transport.TcpBuffered;
            Logger.LogInformation($"Selected client transport:{transport}");
            var protocol = Protocol.Binary;
            Logger.LogInformation($"Selected client protocol: {protocol}");

            //var client= ClientStartup.Get<Calculator.Client>("");
            var client =await ClientStartup.GetByCache<Calculator.Client>("",cancellationToken,true);
            await ExecuteCalculatorClientTest(cancellationToken, client);

            //Task.WaitAll(tasks);

            //await Task.FromResult(1);
            await Task.CompletedTask;

        }

        private static async Task RunClientAsync(Tuple<Protocol,TProtocol> protocolTuple,CancellationToken cancellationToken)
        {
            try
            {
                var protocol = protocolTuple.Item2;
                var protocolType = protocolTuple.Item1;

                TBaseClient client = null;

                try
                {
                    if(protocolType!=Protocol.Multiplexed)
                    {
                        client = new Calculator.Client(protocol);

                        //await ExecuteCalculatorClientOperations(cancellationToken, (Calculator.Client)client);
                        await ExecuteCalculatorClientTest(cancellationToken, (Calculator.Client)client);

                        //Stopwatch sw2 = Stopwatch.StartNew();
                        //for (int i = 0; i < 10; i++)
                        //{   
                        //    await ExecuteCalculatorClientTest(cancellationToken, (Calculator.Client)client);
                        //}
                        //sw2.Stop();
                        //long ms = sw2.ElapsedMilliseconds;
                        //Logger.LogInformation($"RunClientAsync 10 do:10 ms:{sw2.ElapsedMilliseconds}");
                    }
                    else
                    {
                        var multiplex = new TMultiplexedProtocol(protocol, nameof(Calculator));
                        client = new Calculator.Client(multiplex);
                        await ExecuteCalculatorClientOperations(cancellationToken, (Calculator.Client)client);

                        multiplex = new TMultiplexedProtocol(protocol, nameof(SharedService));
                        client = new SharedService.Client(multiplex);

                        await ExecuteSharedServiceClientOperations(cancellationToken, (SharedService.Client)client);
                        //await ExecuteCalculatorClientOperations(cancellationToken, (Calculator.Client)client);


                    }
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex,$"{client?.ClientId}"+ex);
                }
                finally
                {
                    protocol.Transport.Close();
                    Logger.LogInformation($"finally");
                }
            }
            catch(TApplicationException x)
            {
                Logger.LogError(x, x.ToString());
            }
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

        private static Tuple<Protocol,TProtocol> GetProtocol(string[] args, TClientTransport transport)
        {
            var protocol = args.FirstOrDefault(x => x.StartsWith("-pr"))?.Split(':')?[1];

            Protocol selectedProtocol;
            if(Enum.TryParse(protocol,true,out selectedProtocol))
            {
                switch(selectedProtocol)
                {
                    case Protocol.Binary:
                        return new Tuple<Protocol, TProtocol>(selectedProtocol, new TBinaryProtocol(transport));
                    case Protocol.Compact:
                        return new Tuple<Protocol, TProtocol>(selectedProtocol, new TCompactProtocol(transport));
                    case Protocol.Json:
                        return new Tuple<Protocol, TProtocol>(selectedProtocol, new TJsonProtocol(transport));
                    case Protocol.Multiplexed:
                        return new Tuple<Protocol, TProtocol>(selectedProtocol, new TBinaryProtocol(transport));
                }
            }

            return new Tuple<Protocol, TProtocol>(selectedProtocol, new TBinaryProtocol(transport));
        }
        private static TClientTransport GetTransport(string[] args)
        {
            var transport = args.FirstOrDefault(x => x.StartsWith("-tr"))?.Split(':')?[1];

            //var ipAddress = new IPAddress(new byte[] { 10, 0, 70, 88 });
            var ipAddress = IPAddress.Loopback;

            Transport selectedTransport;
            if(Enum.TryParse(transport, out selectedTransport))
            {
                
                
                switch (selectedTransport)
                {
                    case Transport.Tcp:
                        return new TSocketClientTransport(ipAddress, 9090);
                    case Transport.NamedPipe:
                        return new TNamedPipeClientTransport(".test");
                    case Transport.Http:
                        return new THttpClientTransport(new Uri("http://localhost:9090"), null);
                    case Transport.TcpBuffered:
                        return new TBufferedClientTransport(new TSocketClientTransport(ipAddress, 9090));
                    case Transport.Framed:
                        return new TFramedClientTransport(new TSocketClientTransport(ipAddress, 9090));
                }
            }

            return new TSocketClientTransport(ipAddress, 9090);
        }

        private static int GetNumberOfClients(string[] args)
        {
            var numClients = args.FirstOrDefault(x => x.StartsWith("-mc"))?.Split(':')?[1];

            Logger.LogInformation($"Selected # of clients:{numClients}");

            int c = 0;

            if(int.TryParse(numClients,out c)&&(0<c)&&(c<=100))
            {
                return c;
            }

            return 1;

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

        private enum Transport
        {
            Tcp,
            NamedPipe,
            Http,
            TcpBuffered,
            Framed,
            TcpTls
        }

        private enum Protocol
        {
            Binary,
            Compact,
            Json,
            Multiplexed
        }
    }
}
