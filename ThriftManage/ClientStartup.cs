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

namespace ThriftManage
{
    public class ClientStartup
    {
        private static ConcurrentDictionary<Type, object> concurrentDict = new ConcurrentDictionary<Type, object>();
        public static async Task<T> GetByCache<T>(string jsonPath,CancellationToken cancellationToken, bool isOpen = false) where T: TBaseClient
        {
            var type = typeof(T);
            bool flag=concurrentDict.TryGetValue(type, out object obj);
            if(flag)
            {
                return obj as T;
            }

            Console.WriteLine($"GetByCache type:{type}");

            var newT = await Get<T>(jsonPath, cancellationToken, isOpen);
            concurrentDict.TryAdd(type,newT);
            return newT;
        }

        public static async Task<T> Get<T>(string jsonPath,CancellationToken cancellationToken,bool isOpen=false) where T : TBaseClient
        {
            var ipEndPoint = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 9090);
            var transport = GetTransport(Transport.TcpBuffered, ipEndPoint);
            var tProtocol = GetProtocol(Protocol.Binary, transport);

            Console.WriteLine($"GetByOpen transport:{transport} protocol:{tProtocol} ip:{ipEndPoint.Address.ToString()} port:{ipEndPoint.Port}");

            var client = CreateClient<T>(Protocol.Binary, tProtocol);
            if (isOpen)
            {
                await client.OpenTransportAsync(cancellationToken);
            }

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
               //client = new Calculator.Client(tProtocol);
            }
            return client as T;
        }

        private static TClientTransport GetTransport(Transport selectedTransport, IPEndPoint ipEndPoint =null,string uri=null,string namedPipe=null)
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
                    return new TSocketClientTransport(ipEndPoint.Address,ipEndPoint.Port);
                case Transport.NamedPipe:
                    return new TNamedPipeClientTransport(".test");
                case Transport.Http:
                    return new THttpClientTransport(new Uri(uri),null);
                case Transport.TcpBuffered:
                    return new TBufferedClientTransport(new TSocketClientTransport(ipEndPoint.Address, ipEndPoint.Port));
                case Transport.Framed:
                    return new TFramedClientTransport(new TSocketClientTransport(ipEndPoint.Address, ipEndPoint.Port));
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
    }
}
