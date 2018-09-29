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
using Consul;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class TClientModel
    {
        public TBaseClient Client { set; get; }
        public DateTime FlushTime { set; get; } = DateTime.Now;
        public int Seconds { set; get; } = 60;
        public bool IsClose { set; get; }
    }
    public class ClientStartup
    {
        private static ConcurrentDictionary<Type, TClientModel> concurrentDict = new ConcurrentDictionary<Type, TClientModel>();
        private static Timer liveWatch = null;
        public static async Task<T> GetByCache<T>(ThriftClientConfig config, CancellationToken cancellationToken, string appName, bool isOpen = false) where T : TBaseClient
        {
            var type = typeof(T);
            bool flag = concurrentDict.TryGetValue(type, out TClientModel obj);
            if (flag)
            {
                var client = obj.Client as T;
                if (client != null)
                    return client;

                //bool isLive =await CheckConnectLiveAsync(obj);
                //if(isLive)
                //{
                //    var client = obj.Client as T;
                //    if (client != null)
                //        return client;
                //}
            }

            //Console.WriteLine($"GetByCache type:{type} FlushTime:{obj?.FlushTime}");

            var newT = await Get<T>(config, cancellationToken, appName, isOpen);
            var newModel = new TClientModel() {
                Client = newT,
            };

            concurrentDict.AddOrUpdate(type, newModel, (typeV, objV) => newModel);
            CheckLiveTask();
            //Console.ReadLine();

            return newT;
        }

        private static void CheckLiveTask()
        {
            if (liveWatch != null)
                return;

            int count = 0;
            liveWatch = new Timer(obj => {
                Console.WriteLine($"CheckLiveTask time:{DateTime.Now}..... Count:{count++}");
                
                if(concurrentDict.Count>0)
                {
                    foreach(var m in concurrentDict.Values)
                    {
                        var t=CheckConnectLiveAsync(m, true);
                    }
                }
                if(count>int.MaxValue-100)
                {
                    count = 0;
                }
            }, null, 6000, 10000);
        }

        private static async Task<bool> CheckConnectLiveAsync(TClientModel model,bool isLiveCheck=false)
        {
            if (model.Client.InputProtocol.Transport.IsOpen)
            {
                bool flag = true;
                if (isLiveCheck || model.FlushTime < DateTime.Now.AddSeconds(-model.Seconds))
                {
                    try
                    {
                        model.FlushTime = DateTime.Now;
                        //检查链接是否断开
                        await model.Client.OutputProtocol.WriteMessageBeginAsync(new Thrift.Protocols.Entities.TMessage("livecheck", Thrift.Protocols.Entities.TMessageType.Oneway, 0));
                        await model.Client.OutputProtocol.WriteMessageEndAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        flag = false;
                    }
                }

                if (flag)
                    return flag;

                model.Client.Dispose();
            }
            return false;
        }

        private static async Task<IPEndPoint> GetIPEndPointFromConsul(ThriftClientConfig config)
        {
            Console.WriteLine($"GetIPEndPointFromConsul read... dns:{config.ConsulService}");
            IPHostEntry host = await Dns.GetHostEntryAsync(config.ConsulService);
            if (host == null || host.AddressList.Length == 0)
            {
                throw new ArgumentNullException(config.ConsulService, "Consul dns is null");
            }
            var len = host.AddressList.Length;
            IPAddress address = host.AddressList[(len - 1) % (1 + DateTime.Now.Second)];

            return new IPEndPoint(address, config.Port);
        }

        private static async Task<IPEndPoint> GetIPEndPointFromConsulSupplierMorePort(ThriftClientConfig config, CancellationToken cancellationToken)
        {
            Console.WriteLine($"GetIPEndPointFromConsulSupplierMorePort read... MorePort");

            using (var consulClinet = new ConsulManage(config.GetConsulUri()))
            {
                var listService = await consulClinet.GetHetachService(config.Name, HealthStatus.Passing, cancellationToken);
                if (listService == null)
                    throw new ArgumentNullException(config.Name, "Consul no Health service");
                var n = DateTime.Now.Second% listService.Length;
                var model = listService[n];

                IPAddress address = IPAddress.Parse(model.Service.Address);

                return new IPEndPoint(address, model.Service.Port);
            }
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
                //直连
                //ipEndPoint = await GetIPEndPointFromConsul(config);

                //dns
                //ipEndPoint = new IPEndPoint(0, config.Port);
                //hostName = config.ConsulService;
                //多端口
                ipEndPoint = await GetIPEndPointFromConsulSupplierMorePort(config, cancellationToken);
            }


            var transport = GetTransport(Transport.TcpBuffered, ipEndPoint,hostName:hostName);
            var tProtocol = GetProtocol(Protocol.Binary, transport);

            Console.WriteLine($"GetByOpen transport:{transport} protocol:{tProtocol} ip:{ipEndPoint.Address.ToString()} port:{ipEndPoint.Port} hostName:{hostName}");

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
                using (var consulClinet = new ConsulManage(config.GetConsulUri()))
                {
                    await consulClinet.AddKVClientApp(appName, config.Name, DateTime.Now.ToString(), cancellationToken);
                }
            }
        }
    }
}
