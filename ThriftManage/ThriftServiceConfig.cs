﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Kaa.ThriftDemo.ThriftManage
{
    public class ThriftServerConfig
    {
        public string Name { set; get; }
        public string ServiceName { set; get; }
        public int Port { set; get; }
        public string Consul { set; get; }
    }
}