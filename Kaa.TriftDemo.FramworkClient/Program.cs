using Kaa.ThriftDemo.Service.Thrift;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift;
using Thrift.Protocol;
using Thrift.Transport;

namespace Kaa.TriftDemo.FramworkClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async ()=> {
                await Run();
            }).Wait();
        }

        static async Task Run()
        {
            try
            {
                TTransport transport = new TSocket("localhost", 9090, 100000);
                TBufferedTransport transport2 = new TBufferedTransport(transport, 2048);
                TProtocol protocol = new TBinaryProtocol(transport2);

                Calculator.Client client = new Calculator.Client(protocol);

                transport.Open();

                Console.WriteLine($"Starting client... host:localhost port:9090");

                int testCount = 100000;
                Stopwatch sw = Stopwatch.StartNew();
                foreach (var m in Enumerable.Range(0, testCount))
                {
                    int sum = client.add(1, 1);
                    //Console.WriteLine("1+1={0}", sum);
                }
                sw.Stop();
                Console.WriteLine($"Execute client.add(1, 1) do:{testCount} ms:{sw.ElapsedMilliseconds}");

                //Work work = new Work();
                //work.Op = Operation.DIVIDE;
                //work.Num1 = 1;
                //work.Num2 = 0;
                //try
                //{
                //    int quotient = client.calculate(1, work);
                //    Console.WriteLine("Whoa we can divide by 0");
                //}
                //catch (InvalidOperation io)
                //{
                //    Console.WriteLine("Invalid operation: " + io.Why);
                //}

                transport.Close();
            }
            catch (TApplicationException x)
            {
                Console.WriteLine(x.StackTrace);
            }

            await Task.FromResult(1);
        }
    }
}
