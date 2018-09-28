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

namespace Kaa.ThriftDemo.ThriftManage
{
    public class TClientModel
    {
        public TBaseClient Client { set; get; }
        public DateTime FlushTime { set; get; } = DateTime.Now;
        public int Seconds { set; get; } = 20;
        public bool IsClose { set; get; }
    }
    public class ClientStartup
    {
        private static ConcurrentDictionary<Type, TClientModel> concurrentDict = new ConcurrentDictionary<Type, TClientModel>();

        public static T Service<T>(T service) where T : TBaseClient
        {
            return service;
        }

        public static async Task<T> GetByCache<T>(ThriftClientConfig config, CancellationToken cancellationToken,string appName, bool isOpen = false) where T: TBaseClient
        {
            var type = typeof(T);
            bool flag=concurrentDict.TryGetValue(type, out TClientModel obj);
            if(flag)
            {
                var client = obj.Client as T;
                if (obj.Client.InputProtocol.Transport.IsOpen && obj.FlushTime>DateTime.Now.AddSeconds(-obj.Seconds))
                {
                    if (client != null)
                        return client;
                }
                else if(obj.Client.InputProtocol.Transport.IsOpen)
                {
                    obj.Client.Dispose();
                }
            }

            Console.WriteLine($"GetByCache type:{type} FlushTime:{obj?.FlushTime}");

            var newT = await Get<T>(config, cancellationToken,appName, isOpen);
            var newModel = new TClientModel() {
                Client = newT,
            };

            concurrentDict.AddOrUpdate(type, newModel, (typeV,objV)=> newModel);
            return newT;
        }

        private static IPEndPoint GetIPEndPointFromConsul(ThriftClientConfig config)
        {
            Console.WriteLine($"GetIPEndPointFromConsul read... dns:{config.ConsulService}");
            IPHostEntry host = Dns.GetHostEntry(config.ConsulService);
            if(host==null || host.AddressList.Length==0)
            {
                throw new ArgumentNullException(config.ConsulService, "Consul dns is null");
            }
            var len = host.AddressList.Length;
            IPAddress address = host.AddressList[(len-1)%(1+DateTime.Now.Second)];

            return new IPEndPoint(address, config.Port);
        }

        private static IPEndPoint GetIPEndPointFromConfig(ThriftClientConfig config)
        {
            if(string.IsNullOrEmpty(config.IPHost))
            {
                throw new ArgumentNullException(nameof(config.IPHost));
            }

            return new IPEndPoint(IPAddress.Parse(config.IPHost), config.Port);
        }

        public static async Task<T> Get<T>(ThriftClientConfig config, CancellationToken cancellationToken, string appName, bool isOpen=false) where T : TBaseClient
        {
            IPEndPoint ipEndPoint = null;
            //var ipEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 9090);

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if(config.Port==0)
                throw new ArgumentNullException(nameof(config.Port));

            string hostName = null;
            if (string.IsNullOrWhiteSpace(config.ConsulService))
            {
                ipEndPoint = GetIPEndPointFromConfig(config);
            }
            else
            {
                //ipEndPoint =GetIPEndPointFromConsul(config);
                ipEndPoint = new IPEndPoint(0, config.Port);
                hostName = config.ConsulService;
            }


            var transport = GetTransport(Transport.TcpBuffered, ipEndPoint,hostName:hostName);
            var tProtocol = GetProtocol(Protocol.Binary, transport);

            Console.WriteLine($"GetByOpen transport:{transport} protocol:{tProtocol} ip:{ipEndPoint.Address.ToString()} port:{ipEndPoint.Port}");

            var client = CreateClient<T>(Protocol.Binary, tProtocol);
            if (isOpen)
            {
                await client.OpenTransportAsync(cancellationToken);
            }

            //consul
            await RegisterConsul(config,appName,cancellationToken);

            return client;
        }

        private static T CreateClient<T>(Protocol protocol,TProtocol tProtocol) where T: TBaseClient
        {
           object client = null;
            if (protocol != Protocol.Multiplexed)
            {
                //Console.WriteLine(typeof(T));
                //client= typeof(T).GetConstructor(new Type[] {typeof(TProtocol) }).Invoke(new object[] { tProtocol });
                client = Activator.CreateInstance(typeof(T), new object[] { tProtocol });
            }
            else
            {
                var multiplex = new TMultiplexedProtocol(tProtocol, nameof(T).Replace(".Client",""));
                client = Activator.CreateInstance(typeof(T), new object[] { multiplex });
            }

            return client as T;
        }

        private static TClientTransport GetTransport(Transport selectedTransport, IPEndPoint ipEndPoint =null,string uri=null,string namedPipe=null,string hostName=null)
        {
            switch (selectedTransport)
            {
                case Transport.Tcp:
                case Transport.TcpBuffered:
                case Transport.Framed:
                case Transport.TcpTls:
                    if (ipEndPoint == null)
                        throw new ArgumentNullException(nameof(ipEndPoint), "ipEndPoint must not is null");
                    break;
                case Transport.NamedPipe:
                    if (namedPipe == null)
                        throw new ArgumentNullException(nameof(namedPipe), "NamedPipe must not is null");
                    break;
                case Transport.Http:
                    if (uri == null)
                        throw new ArgumentNullException(nameof(uri), "uri must not is null");
                    break;
            }

            switch (selectedTransport)
            {
                case Transport.Tcp:
                    return new TSocketClientTransportSupportDns(ipEndPoint.Address,ipEndPoint.Port,hostName: hostName);
                case Transport.NamedPipe:
                    return new TNamedPipeClientTransport(".test");
                case Transport.Http:
                    return new THttpClientTransport(new Uri(uri),null);
                case Transport.TcpBuffered:
                    return new TBufferedClientTransport(new TSocketClientTransportSupportDns(ipEndPoint.Address, ipEndPoint.Port, hostName: hostName));
                case Transport.Framed:
                    return new TFramedClientTransport(new TSocketClientTransportSupportDns(ipEndPoint.Address, ipEndPoint.Port, hostName: hostName));
                default:
                    throw new NotSupportedException(selectedTransport.ToString());
            }
        }

        private static TProtocol GetProtocol(Protocol selectedProtocol, TClientTransport transport)
        {
            switch (selectedProtocol)
            {
                case Protocol.Binary:
                    return new TBinaryProtocol(transport);
                case Protocol.Compact:
                    return new TCompactProtocol(transport);
                case Protocol.Json:
                    return new TJsonProtocol(transport);
                case Protocol.Multiplexed:
                    return new TBinaryProtocol(transport);
                default:
                    throw new NotSupportedException(selectedProtocol.ToString());
            }
        }

        private static async Task RegisterConsul(ThriftClientConfig config,string appName, CancellationToken cancellationToken)
        {
            if (config.Consul != null)
            {
                Console.WriteLine($"Incompetent registration on the Consul");

                //api
                var consulClinet = new ConsulManage(config.GetConsulUri());
                await consulClinet.AddKVClientApp(appName, config.Name,DateTime.Now.ToString(), cancellationToken);
            }
        }
    }
}
