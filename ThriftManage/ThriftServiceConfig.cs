using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ThriftServerConfig
    {
        public string Name { set; get; }
        public string ServiceName { set; get; }
        public int Port { set; get; }
        public ConsulConfig Consul { set; get; }
        
        public Uri GetConsulUri()
        {
            if(Consul!=null)
            {
                var uri = new Uri(Consul.Url);
                return uri;
            }
            return null;
        }
    }

    public class ConsulConfig
    {
        public string[] Tags { set; get; }
        public bool Check { set; get; }

        public string Url { set; get; }
    }
}
