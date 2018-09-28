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
using System.Reflection;
using Consul;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ServerStartup
    {
        private static async Task RegisterConsul(ThriftServerConfig config,ILogger log, CancellationToken cancellationToken)
        {
            if (config.Consul!=null)
            {
                log.LogInformation($"Incompetent registration on the Consul");

                //api
                var consulClinet = new ConsulManage(config.GetConsulUri());
                await consulClinet.RegisterServiceAsync(config,cancellationToken);
            }
        }

        public static async Task Stop(ThriftServerConfig config, ILogger log, CancellationToken cancellationToken)
        {
            if (config.Consul != null)
            {
                log.LogInformation($"Incompetent deregistration on the Consul");

                //api
                var consulClinet = new ConsulManage(config.GetConsulUri());
                await consulClinet.DeregisterServiceAsync(config,cancellationToken);
            }
        }

        public static async Task Init<T,T2>(ThriftServerConfig config,T handler, CancellationToken cancellationToken) 
            where T2 : ITAsyncProcessor
        {
            var fabric = new LoggerFactory()
                .AddConsole(LogLevel.Information)
                .AddDebug(LogLevel.Trace);

            ILogger Logger = fabric.CreateLogger(nameof(ServerStartup));
            
            Transport transport=Transport.TcpBuffered;
            Protocol protocol = Protocol.Binary;
            int port = config.Port;


            var typeT2 = typeof(T2);
            //var handler = new T();

            ITAsyncProcessor processor = null;
            TServerTransport serverTransport = null;

            switch (transport)
            {
                case Transport.Tcp:
                    serverTransport = new TServerSocketTransport(port);
                    break;
                case Transport.TcpBuffered:
                    serverTransport = new TServerSocketTransport(port, 10000, true);
                    break;
                case Transport.NamedPipe:
                    serverTransport = new TNamedPipeServerTransport(".test");
                    break;
                case Transport.TcpTls:
                    serverTransport = new TTlsServerSocketTransport(port, false, GetCertificate(), ClientCertValidator, LocalCertificateSelectionCallback);
                    break;
                case Transport.Framed:
                    serverTransport = new TServerFramedTransport(port);
                    break;
                case Transport.Http:
                    throw new NotSupportedException(nameof(Transport.Http));
            }

            ITProtocolFactory inputProtocolFactory;
            ITProtocolFactory outputProtocolFactory;

            object objProcessor = null;
            switch (protocol)
            {
                case Protocol.Binary:
                    inputProtocolFactory = new TBinaryProtocol.Factory();
                    outputProtocolFactory = new TBinaryProtocol.Factory();

                    objProcessor = Activator.CreateInstance(typeT2, new object[] { handler });
                    //var assm = Assembly.Load("Kaa.ThriftDemo.Service.Thrift");
                    //var typeT2 =assm.GetType("Kaa.ThriftDemo.Service.Thrift.Calculator+AsyncProcessor");
                    //var type = typeof(T).GetInterfaces()[0];
                    //var obj=typeT2.GetConstructor(new Type[] { type }).Invoke(new object[] { handler });

                    break;
                case Protocol.Compact:
                    inputProtocolFactory = new TCompactProtocol.Factory();
                    outputProtocolFactory = new TCompactProtocol.Factory();

                    //processor = new Calculator.AsyncProcessor(handler);
                    objProcessor = Activator.CreateInstance(typeT2, new object[] { handler });
                    break;
                case Protocol.Json:
                    inputProtocolFactory = new TJsonProtocol.Factory();
                    outputProtocolFactory = new TJsonProtocol.Factory();

                    objProcessor = Activator.CreateInstance(typeT2, new object[] { handler });
                    break;
                case Protocol.Multiplexed:
                    inputProtocolFactory = new TBinaryProtocol.Factory();
                    outputProtocolFactory = new TBinaryProtocol.Factory();

                    objProcessor = Activator.CreateInstance(typeT2, new object[] { handler });
                    var calcProcessor = objProcessor as ITAsyncProcessor;

                    var multiplexedProcessor = new TMultiplexedProcessor();
                    multiplexedProcessor.RegisterProcessor(typeof(T).FullName, calcProcessor);

                    //multiplexedProcessor.RegisterProcessor(nameof(SharedService), calcProcessorShared);

                    processor = multiplexedProcessor;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null);
            }

            if (objProcessor == null)
                throw new ArgumentNullException(nameof(objProcessor), "objProcessor is null");

            processor = objProcessor as ITAsyncProcessor;
            if(processor==null)
                throw new ArgumentNullException(nameof(processor), "processor is null");

            try
            {
                Logger.LogInformation($"Selected TAsyncServer with {serverTransport} transport, {processor} processor and {inputProtocolFactory} protocol factories");

                var server = new AsyncBaseServer(processor, serverTransport, inputProtocolFactory, outputProtocolFactory, fabric);

                Logger.LogInformation($"Starting the server port:{port} transport:{transport} protocol:{protocol} ...");

                await server.ServeAsync(cancellationToken);


                Utils.LocalIPListPrint();
                await RegisterConsul(config, Logger,cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogInformation(ex, ex.Message);
                throw;
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
