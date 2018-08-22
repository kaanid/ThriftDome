using Kaa.ThriftDemo.ThriftManage.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocols;
using Thrift.Transports;
using Thrift.Transports.Client;
using Kaa.ThriftDemo.ThriftManage;
using Microsoft.Extensions.Logging;
using Thrift.Transports.Server;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Net.Security;
using Thrift.Server;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ServerStartup
    {

        public static async Task RunSelectedConfigurationAsync<T>(Transport transport, Protocol protocol,int port, CancellationToken cancellationToken) where T:new()
        {
            var fabric = new LoggerFactory()
                .AddConsole(LogLevel.Trace)
                .AddDebug(LogLevel.Trace);

            ILogger Logger = fabric.CreateLogger(nameof(RunSelectedConfigurationAsync));

            var handler = new T();

            ITAsyncProcessor processor = null;
            TServerTransport serverTransport = null;

            switch (transport)
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

            switch (protocol)
            {
                case Protocol.Binary:
                    inputProtocolFactory = new TBinaryProtocol.Factory();
                    outputProtocolFactory = new TBinaryProtocol.Factory();
                    //var obj= Activator.CreateInstance(typeof("asfd",true),new object[] { handler });
                    var type= Type.GetType($"{typeof(T).ReflectedType.ToString()}.AsyncProcessor");
                    var obj = Activator.CreateInstance(type, new object[] { handler });
                    processor = obj as ITAsyncProcessor;
                    break;
                case Protocol.Compact:
                    inputProtocolFactory = new TCompactProtocol.Factory();
                    outputProtocolFactory = new TCompactProtocol.Factory();
                    //processor = new Calculator.AsyncProcessor(handler);
                    break;
                case Protocol.Json:
                    inputProtocolFactory = new TJsonProtocol.Factory();
                    outputProtocolFactory = new TJsonProtocol.Factory();
                    //processor = new Calculator.AsyncProcessor(handler);
                    break;
                case Protocol.Multiplexed:
                    inputProtocolFactory = new TBinaryProtocol.Factory();
                    outputProtocolFactory = new TBinaryProtocol.Factory();

                    //var calcProcessor = new Calculator.AsyncProcessor(handler);

                    //var multiplexedProcessor = new TMultiplexedProcessor();
                    ///multiplexedProcessor.RegisterProcessor(nameof(Calculator), calcProcessor);
                    //multiplexedProcessor.RegisterProcessor(nameof(SharedService), calcProcessorShared);

                    //processor = multiplexedProcessor;
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

        private static bool ClientCertValidator(object sender, X509Certificate certificate,
            X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static X509Certificate LocalCertificateSelectionCallback(object sender,
            string targetHost, X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return GetCertificate();
        }
    }
}
