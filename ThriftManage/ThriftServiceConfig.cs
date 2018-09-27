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
        
    }

    public class ConsulConfig
    {
        public string[] Tags { set; get; }
        public bool Check { set; get; }
    }
}
