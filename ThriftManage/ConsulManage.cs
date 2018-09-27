using Consul;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ConsulManage:IDisposable
    {
        private readonly ConsulClient _client;

        public ConsulManage(Uri uri)
        {
            if(uri==null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            _client= new ConsulClient(conf =>
            {
                conf.Address = uri;
                conf.WaitTime = TimeSpan.FromSeconds(2);
            });
        }

        public async Task<bool> RegisterServiceAsync(ThriftServerConfig confi, CancellationToken cancellationToken)
        {
            var ip = Utils.LocalIPAddress().ToString();
            var ser = new AgentServiceRegistration
            {
                ID = GetServiceId(confi.Name,ip),
                Name = confi.Name,
                Port = confi.Port,
                Address = ip,
                Tags = confi.Consul.Tags,
                Meta = new Dictionary<string, string>() { { "a", "1" } },
                Check = confi.Consul.Check ? new AgentServiceCheck
                {
                    Interval = TimeSpan.FromSeconds(4),
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

        private string GetServiceId(string serverName, string ip = null)
        {
            if (ip == null)
            {
                ip = Utils.LocalIPAddress().ToString();
            }
            return $"{serverName}_{ip}";
        }

        public async Task<bool> DeregisterServiceAsync(ThriftServerConfig confi, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await DeregisterServiceAsync(confi.Name,cancellationToken);
        }

        public async Task<bool> DeregisterServiceAsync(string serverName,CancellationToken cancellationToken=default(CancellationToken))
        {
            var result = await _client.Agent.ServiceDeregister(GetServiceId(serverName), cancellationToken);
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
