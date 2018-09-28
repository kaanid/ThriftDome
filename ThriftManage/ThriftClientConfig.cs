using System;
using System.Collections.Generic;
using System.Text;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ThriftClientConfig
    {
        public string Name { set; get; }
        public string ServiceName { set; get; }
        public string IPHost { set; get; }
        public int Port { set; get; }
        public string Consul { set; get; }
        public string ConsulService { set; get; }

        public Uri GetConsulUri()
        {
            if(!string.IsNullOrWhiteSpace(Consul))
            {
                return new Uri(Consul);
            }
            return null;
        }
    }
}
