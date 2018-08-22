using System;
using tutorial.c;
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

namespace ConsoleApp33ThriftService
{
    class Program
    {
        private static readonly ILogger Logger = new LoggerFactory().AddConsole(LogLevel.Trace).CreateLogger(nameof(ConsoleApp33ThriftService));
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            args = args ?? new string[0];

            args = new string[] { "-pr:Binary", "-tr:TcpBuffered" };

            if (args.Any(x => x.StartsWith("-help", StringComparison.OrdinalIgnoreCase)))
            {
                DisplayHelp();
                return;
            }

            using (var source = new CancellationTokenSource())
            {

                Logger.LogInformation("Press any key to stop...");

                RunAsync(args, source.Token).Wait();

                Console.ReadLine();
                source.Cancel();
            }

        }

        private static async Task RunAsync(string[] args, CancellationToken token)
        {
            var selectedTransport = GetTransport(args);
            var selectedProtocol = GetProtocol(args);

            if(selectedTransport==Transport.Http)
            {
                //new HttpServerSample()
            }
            else
            {
                await RunSelectedConfigurationAsync(selectedTransport, selectedProtocol, token);
            }

        }

        private static async Task RunSelectedConfigurationAsync(Transport transport,Protocol protocol,CancellationToken cancellationToken)
        {
            var fabric = new LoggerFactory().AddConsole(LogLevel.Trace).AddDebug(LogLevel.Trace);
            var handler = new ThriftService();
            ITAsyncProcessor processor = null;
            TServerTransport serverTransport = null;

            switch(transport)
            {
                case Transport.Tcp:
                    serverTransport = new TServerSocketTransport(9090);
                    break;
                case Transport.TcpBuffered:
                    serverTransport = new TServerSocketTransport(9090, 10000, true);
                    break;
                case Transport.NamedPipe:
                    serverTransport = new TNamedPipeServerTransport(".test");
                    break;
                case Transport.TcpTls:
                    serverTransport = new TTlsServerSocketTransport(9090, false, GetCertificate(), ClientCertValidator, LocalCertificateSelectionCallback);
                    break;
                case Transport.Framed:
                    serverTransport = new TServerFramedTransport(9090);
                    break;
            }

            ITProtocolFactory inputProtocolFactory;
            ITProtocolFactory outputProtocolFactory;

            switch(protocol)
            {
                case Protocol.Binary:
                    inputProtocolFactory = new TBinaryProtocol.Factory();
                    outputProtocolFactory = new TBinaryProtocol.Factory();
                    processor = new Calculator.AsyncProcessor(handler);
                    break;
                case Protocol.Compact:
                    inputProtocolFactory = new TCompactProtocol.Factory();
                    outputProtocolFactory = new TCompactProtocol.Factory();
                    processor = new Calculator.AsyncProcessor(handler);
                    break;
                case Protocol.Json:
                    inputProtocolFactory = new TJsonProtocol.Factory();
                    outputProtocolFactory = new TJsonProtocol.Factory();
                    processor = new Calculator.AsyncProcessor(handler);
                    break;
                case Protocol.Multiplexed:
                    inputProtocolFactory = new TBinaryProtocol.Factory();
                    outputProtocolFactory = new TBinaryProtocol.Factory();

                    var calcHandler = new ThriftService();
                    var calcProcessor = new Calculator.AsyncProcessor(calcHandler);

                    var calcHandlerShared = new SharedServiceAsyncHandler();
                    var calcProcessorShared = new SharedService.AsyncProcessor(calcHandlerShared);


                    var multiplexedProcessor = new TMultiplexedProcessor();
                    multiplexedProcessor.RegisterProcessor(nameof(Calculator), calcProcessor);
                    multiplexedProcessor.RegisterProcessor(nameof(SharedService), calcProcessorShared);

                    processor = multiplexedProcessor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null);
            }

            try
            {
                Logger.LogInformation(
                    $"Selected TAsyncServer with {serverTransport} transport, {processor} processor and {inputProtocolFactory} protocol factories");

                var server = new AsyncBaseServer(processor, serverTransport, inputProtocolFactory, outputProtocolFactory, fabric);

                Logger.LogInformation("Starting the server ...");

                await server.ServeAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogInformation(ex, ex.Message);
            }
        }

        private static Protocol GetProtocol(string[] args)
        {
            var transport = args.FirstOrDefault(x => x.StartsWith("-pr"))?.Split(':')?[1];

            Enum.TryParse(transport, true, out Protocol selectedProtocol);

            return selectedProtocol;
        }

        private static Transport GetTransport(string[] args)
        {
            var transport = args.FirstOrDefault(x => x.StartsWith("-tr"))?.Split(':')?[1];

            Enum.TryParse(transport, true, out Transport selectedTransport);

            return selectedTransport;
        }

        private static void DisplayHelp()
        {
            Logger.LogInformation(@"
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

        private static X509Certificate2 GetCertificate()
        {
            // due to files location in net core better to take certs from top folder
            var certFile = GetCertPath(Directory.GetParent(Directory.GetCurrentDirectory()));
            return new X509Certificate2(certFile, "ThriftTest");
        }

        private static string GetCertPath(DirectoryInfo di, int maxCount = 6)
        {
            var topDir = di;
            var certFile =
                topDir.EnumerateFiles("ThriftTest.pfx", SearchOption.AllDirectories)
                    .FirstOrDefault();
            if (certFile == null)
            {
                if (maxCount == 0)
                    throw new FileNotFoundException("Cannot find file in directories");
                return GetCertPath(di.Parent, maxCount - 1);
            }

            return certFile.FullName;
        }

        private static X509Certificate LocalCertificateSelectionCallback(object sender,
            string targetHost, X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return GetCertificate();
        }

        private static bool ClientCertValidator(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }


        private enum Transport
        {
            Tcp,
            TcpBuffered,
            NamedPipe,
            Http,
            TcpTls,
            Framed
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
