using System;
using System.Collections.Generic;
using System.Text;

namespace Kaa.ThriftDemo.Service
{
    public interface ISample
    {
        void Call();
    }
    [Interceptor]
    public class Sammpleclass : ISample
    {
        public void Call()
        {
            Console.WriteLine("Call");
        }
    }
}
