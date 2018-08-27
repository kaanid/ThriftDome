using Kaa.ThriftDemo.Service.Thrift;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Server;
using Thrift.Transport;

namespace Kaa.ThriftDemo.Framework.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ThriftService handler = new ThriftService();
                Calculator.Processor processor = new Calculator.Processor(handler);

                TServerTransport serverTransport = new TServerSocket(9090);
                TServer server = new TSimpleServer(processor, serverTransport);

                //server = new TThreadPoolServer(processor, serverTransport);

                Console.WriteLine("Starting the server...");
                server.Serve();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            Console.WriteLine("done.");
        }
    }
}
