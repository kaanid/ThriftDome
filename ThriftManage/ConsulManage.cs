using Consul;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ConsulManage:IDisposable
    {
        private readonly ConsulClient _client;

        public ConsulManage(Uri uri, ConsulClient client=null)
        {
            if(uri==null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (client == null || client.Config.Address!=uri)
            {
                _client = new ConsulClient(conf =>
                 {
                     conf.Address = uri;
                     conf.WaitTime = TimeSpan.FromSeconds(2);
                 });
            }
        }

        public async Task<bool> RegisterServiceAsync(ThriftServerConfig confi, CancellationToken cancellationToken)
        {
            var ip = Utils.LocalIPAddress().ToString();
            var ser = new AgentServiceRegistration
            {
                ID = GetServiceId(confi.Name,confi.Port,ip),
                Name = confi.Name,
                Port = confi.Port,
                Address = ip,
                Tags = confi.Consul.Tags,
                Meta = new Dictionary<string, string>() { { "a", "1" } },
                Check = confi.Consul.Check ? new AgentServiceCheck
                {
                    Interval = TimeSpan.FromSeconds(10),
                    TCP = $"{ip}:{confi.Port}",
                    Timeout = TimeSpan.FromSeconds(2),
                } : null
            };

            return await RegisterServiceAsync(ser,cancellationToken);
        }

        public async Task<bool> RegisterServiceAsync(AgentServiceRegistration model, CancellationToken cancellationToken)
        {
            var result=await _client.Agent.ServiceRegister(model, cancellationToken);
            if(result.StatusCode==System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        private string GetServiceId(string serverName, int port, string ip = null)
        {
            if (ip == null)
            {
                ip = Utils.LocalIPAddress().ToString();
            }
            return $"{serverName}_{ip}_{port}";
        }

        public async Task<bool> DeregisterServiceAsync(ThriftServerConfig confi, CancellationToken cancellationToken = default(CancellationToken))
        {
            var serverName = GetServiceId(confi.Name, confi.Port);
            return await DeregisterServiceAsync(serverName, cancellationToken);
        }

        public async Task<bool> DeregisterServiceAsync(string serverName,CancellationToken cancellationToken=default(CancellationToken))
        {
            var result = await _client.Agent.ServiceDeregister(serverName, cancellationToken);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> AddKVServiceMethod(string serverName,int port, string methodName,string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await AddKV($"Service-{serverName}/{Utils.LocalIPAddress().ToString()}/{methodName}",value,cancellationToken);
        }

        public async Task<bool> AddKVClientApp(string appName,string callServerName, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await AddKV($"Client-{appName}/{callServerName}", value, cancellationToken);
        }

        public async Task<bool> AddKV(string kvName,string value, CancellationToken cancellationToken = default(CancellationToken))
        {

            KVPair kv = new KVPair(kvName);
            kv.Value = Encoding.UTF8.GetBytes(value);
           
            var result = await _client.KV.Put(kv, WriteOptions.Default);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public async Task<ServiceEntry[]> GetHetachService(string serviceName, HealthStatus status = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            //http://127.0.0.1:8500/v1/health/service/web2

            var result = await _client.Health.Service(serviceName, cancellationToken);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && result.Response != null && result.Response.Length > 0)
            {
                if (status == null)
                    return result.Response;

                var list = result.Response.Where(m => m.Checks.All(m2=>m2.Status == HealthStatus.Passing)).ToArray();
                if (list.Length > 0)
                {
                    return list;
                }
            }
            return null;
        }

        public async Task<HealthCheck[]> GetHetachServiceCheckList(string serviceName, HealthStatus status=null, CancellationToken cancellationToken = default(CancellationToken))
        {
            //http://127.0.0.1:8500/v1/health/state/passing
            //http://127.0.0.1:8500/v1/health/checks/web2

            var result = await _client.Health.Checks(serviceName, cancellationToken);
            if (result.StatusCode == System.Net.HttpStatusCode.OK && result.Response != null && result.Response.Length>0)
            {
                if (status == null)
                    return result.Response;

                var list = result.Response.Where(m => m.Status == HealthStatus.Passing).ToArray();
                if (list.Length > 0)
                {
                    return list;
                }
            }
            return null;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
