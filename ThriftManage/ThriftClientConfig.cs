using System;
using System.Collections.Generic;
using System.Text;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ThriftClientConfig
    {
        public string Name { set; get; }
        public string ServiceName { set; get; }
        public IP IP { set; get; }
        public string Consul { set; get; }
    }

    public class IP
    {
        public string Host { set; get; }
        public int Port { set; get; }
    }
}
